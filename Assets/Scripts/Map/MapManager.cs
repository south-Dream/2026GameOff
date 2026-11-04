using System.Collections.Generic;
using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 管理道路地图并提供投影与落脚位置查询
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private MapData data = new MapData();
        [SerializeField] private Transform roadRoot;
        [SerializeField] private Transform projectionRoot;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private GameObject projectionPrefab;
        [SerializeField] private GameObject worldDetails;
        [SerializeField] private Transform goalPoint;
        private MapMechanism[] mechanisms;
        private MapMechanism movingMechanism;
        private bool worldMode = true;
        private MapController controller;
        private List<ProjectionData> projection;
        private SpaceAxis axis;
        private float plane;

        public MapData Data => data;
        public float Plane => plane;
        public bool IsMoving => movingMechanism != null;

        public Vector3 ProjectionCenter
        {
            get
            {
                if (projection == null || projection.Count == 0) return data.spawn;
                Bounds bounds = new Bounds(projection[0].position, projection[0].size);
                foreach (ProjectionData piece in projection) bounds.Encapsulate(new Bounds(piece.position, piece.size));
                return bounds.center;
            }
        }

        public void Init()
        {
            data.spawn = spawnPoint.position;
            mechanisms = GetComponentsInChildren<MapMechanism>();
            controller = new MapController();
            controller.Init(roadRoot, projectionRoot, projectionPrefab, data, mechanisms);
            foreach (MapMechanism mechanism in mechanisms) mechanism.Init();
            foreach (MechanismButton button in GetComponentsInChildren<MechanismButton>(true)) button.Init(this);
        }

        public void Preview(RoadData anchor, SpaceAxis selectedAxis)
        {
            worldMode = false;
            axis = selectedAxis;
            plane = axis == SpaceAxis.Y ? anchor.Top : anchor.position[(int)axis];
            projection = MapData.CreateProjection(data.roads, anchor, axis);
            controller.CreatePreview(projection);
            if (worldDetails != null) worldDetails.SetActive(false);
        }

        public void Compress()
        {
            controller.SetWorld(false);
            controller.SetFlat(true, true);
            Physics.SyncTransforms();
        }

        public void Expand()
        {
            worldMode = true;
            controller.SetFlat(false, false);
            controller.SetWorld(true);
            if (worldDetails != null) worldDetails.SetActive(true);
            Physics.SyncTransforms();
        }

        public void CancelPreview()
        {
            worldMode = true;
            controller.SetFlat(false, false);
            controller.SetWorld(true);
            if (worldDetails != null) worldDetails.SetActive(true);
        }

        public bool IsAtGoal(Vector3 foot)
        {
            if (goalPoint == null) return false;
            Vector3 delta = foot - goalPoint.position;
            return Mathf.Abs(delta.y) < 0.2f && new Vector2(delta.x, delta.z).magnitude < 1.1f;
        }

        /// <summary>
        /// 检查操作距离与道路占用后启动机关 升降台要求玩家先登台
        /// </summary>
        public bool CanUse(MapMechanism mechanism, PlayerManager player, out string message)
        {
            message = "";
            if (!worldMode) { message = "请先展开空间"; return false; }
            if (IsMoving) { message = "机关正在运行"; return false; }
            if (mechanism == null || System.Array.IndexOf(mechanisms, mechanism) < 0)
            { message = "机关尚未准备好"; return false; }
            player.RefreshGround();
            if (!player.Data.grounded) { message = "请先站稳"; return false; }
            Vector3 hand = player.Position + Vector3.up * 0.8f;
            if (Vector3.Distance(hand, mechanism.Button.bounds.ClosestPoint(hand)) > mechanism.Data.useDistance)
            { message = "请靠近操作按钮"; return false; }
            controller.TryGetRoad(mechanism.Road, out RoadData road);
            bool onPlatform = player.Data.road == road;
            if (mechanism.Data.carryPlayer && !onPlatform)
            { message = "请先走上升降台"; return false; }
            if (!mechanism.Data.carryPlayer && onPlatform)
            { message = "请先离开桥面"; return false; }
            return true;
        }

        public bool TryUse(MapMechanism mechanism, PlayerManager player, out string message)
        {
            if (!CanUse(mechanism, player, out message)) return false;
            movingMechanism = mechanism;
            mechanism.StartMove();
            player.SetControl(false);
            message = "机关已启动";
            return true;
        }

        public void TickMechanism(PlayerManager player)
        {
            if (!IsMoving) return;
            Vector3 delta = movingMechanism.Tick(Time.deltaTime);
            if (movingMechanism.Data.carryPlayer) player.Carry(delta);
            if (movingMechanism.IsMoving) return;
            controller.RefreshRoads();
            player.RefreshGround();
            movingMechanism = null;
            player.SetControl(true);
        }

        public void ResetMechanisms()
        {
            foreach (MapMechanism mechanism in mechanisms) mechanism.ResetState();
            movingMechanism = null;
            controller.RefreshRoads();
        }

        public bool TryGetGround(Vector3 foot, float distance, out RoadData road, out Vector3 point)
        {
            road = null;
            point = foot;
            RaycastHit[] hits = Physics.RaycastAll(foot + Vector3.up * 0.15f, Vector3.down, distance + 0.15f);
            float nearest = float.MaxValue;
            int priority = int.MaxValue;
            foreach (RaycastHit hit in hits)
            {
                if (!controller.TryGetRoad(hit.collider, out RoadData source)) continue;
                int order = data.roads.IndexOf(source);
                if (hit.distance > nearest + 0.001f ||
                    (Mathf.Abs(hit.distance - nearest) <= 0.001f && order >= priority)) continue;
                nearest = hit.distance;
                priority = order;
                road = source;
                point = hit.point;
            }
            return road != null;
        }

        /// <summary>
        /// 查询完整角色落脚范围并检查原地图中的恢复空间
        /// </summary>
        public bool TryRestore(Vector3 foot, RoadData road, float radius, float height, out Vector3 target)
        {
            target = foot;
            if (road == null) return false;
            target = MapData.Restore(foot, road, axis);
            if (Mathf.Abs(target.x - road.position.x) > road.size.x / 2 - radius ||
                Mathf.Abs(target.z - road.position.z) > road.size.z / 2 - radius) return false;
            Bounds player = new Bounds(target + Vector3.up * (height / 2 + 0.04f),
                new Vector3(radius * 2, height - 0.02f, radius * 2));
            foreach (RoadData other in data.roads)
                if (new Bounds(other.position, other.size).Intersects(player)) return false;
            return true;
        }

    }
}
