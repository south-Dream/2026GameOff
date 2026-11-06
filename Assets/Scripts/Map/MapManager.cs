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
        private MapMechanism[] mechanisms;
        private MapMechanism movingMechanism;
        /// <summary>
        /// 仅承载已登台角色 远程召回时不带动玩家
        /// </summary>
        private bool carryPlayer;
        private bool worldMode = true;
        private MapController controller;
        private List<ProjectionData> projection;
        private SpaceAxis axis;
        private float plane;

        public MapData Data => data;
        public float Plane => plane;
        public bool IsMoving => movingMechanism != null;

        public void SetSoundPaused(bool paused)
        {
            foreach (MapMechanism mechanism in mechanisms) mechanism.SetSoundPaused(paused);
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

        /// <summary>
        /// 从指定观察面生成投影 当前道路始终优先
        /// </summary>
        public void CreateProjection(RoadData anchor, SpaceAxis selectedAxis, Vector3 direction)
        {
            worldMode = false;
            axis = selectedAxis;
            plane = axis == SpaceAxis.Y ? anchor.Top : anchor.position[(int)axis];
            projection = MapData.CreateProjection(data.roads, anchor, axis, direction);
            controller.CreateProjection(projection);
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

        /// <summary>
        /// 检查机关状态与道路占用 交互距离由当前按钮检测
        /// </summary>
        public bool CanUse(MapMechanism mechanism, PlayerManager player, out string message)
        {
            message = "";
            if (!worldMode) { message = "notice.unfoldFirst"; return false; }
            if (IsMoving) { message = "notice.moving"; return false; }
            if (mechanism == null || System.Array.IndexOf(mechanisms, mechanism) < 0)
            { message = "mechanism.wait"; return false; }
            player.RefreshGround();
            if (!player.Data.grounded) { message = "interact.grounded"; return false; }
            bool onPlatform = controller.IsOnRoad(mechanism, player.Data.road);
            if (mechanism.Data.carryPlayer && !onPlatform && !(mechanism.Data.allowCall && mechanism.IsRaised))
            { message = "notice.platform"; return false; }
            if (!mechanism.Data.carryPlayer && onPlatform)
            { message = "notice.bridge"; return false; }
            return true;
        }

        public bool TryUse(MapMechanism mechanism, PlayerManager player, out string message)
        {
            if (!CanUse(mechanism, player, out message)) return false;
            movingMechanism = mechanism;
            carryPlayer = mechanism.Data.carryPlayer && controller.IsOnRoad(mechanism, player.Data.road);
            mechanism.StartMove();
            player.SetControl(false);
            message = "notice.started";
            return true;
        }

        public void TickMechanism(PlayerManager player)
        {
            if (!IsMoving) return;
            Vector3 delta = movingMechanism.Tick(Time.deltaTime);
            if (carryPlayer) player.Carry(delta);
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
        /// 检查还原空间 原深度沿用已站稳位置 传空按新道路计算
        /// </summary>
        public bool TryRestore(Vector3 foot, RoadData road, float? depth, float radius, float height, out Vector3 target)
        {
            target = foot;
            if (road == null) return false;
            target = MapData.Restore(foot, road, axis, depth ?? road.position[(int)axis]);
            float xMargin = depth.HasValue && axis == SpaceAxis.X ? 0 : radius;
            float zMargin = depth.HasValue && axis == SpaceAxis.Z ? 0 : radius;
            if (Mathf.Abs(target.x - road.position.x) > road.size.x / 2 - xMargin ||
                Mathf.Abs(target.z - road.position.z) > road.size.z / 2 - zMargin) return false;
            if (!controller.HasSurface(road, target)) return false;
            Bounds player = new Bounds(target + Vector3.up * (height / 2 + 0.04f),
                new Vector3(radius * 2, height - 0.02f, radius * 2));
            foreach (RoadData other in data.roads)
                if (new Bounds(other.position, other.size).Intersects(player)) return false;
            return true;
        }

    }
}
