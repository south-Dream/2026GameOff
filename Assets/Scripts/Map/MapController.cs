using System.Collections.Generic;
using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 读取场景道路并使用预制体显示投影碰撞
    /// </summary>
    public class MapController
    {
        private readonly List<GameObject> worldObjects = new List<GameObject>();
        private readonly List<GameObject> flatObjects = new List<GameObject>();
        private readonly Dictionary<Collider, RoadData> sources = new Dictionary<Collider, RoadData>();
        private Transform root;
        private GameObject projectionPrefab;

        public void Init(Transform roadRoot, Transform projectionRoot, GameObject prefab, MapData data, MapMechanism[] mechanisms)
        {
            root = projectionRoot;
            projectionPrefab = prefab;
            data.roads.Clear();
            Physics.SyncTransforms();
            var colliders = new List<Collider>(roadRoot.GetComponentsInChildren<Collider>());
            foreach (MapMechanism mechanism in mechanisms)
                foreach (Collider segment in mechanism.Roads)
                    if (!colliders.Contains(segment)) colliders.Add(segment);
            foreach (Collider collider in colliders)
            {
                var road = new RoadData(collider.name, collider.bounds.center, collider.bounds.size);
                data.roads.Add(road);
                worldObjects.Add(collider.gameObject);
                sources.Add(collider, road);
            }
        }

        public void CreateProjection(List<ProjectionData> projection)
        {
            ClearProjection();
            foreach (ProjectionData piece in projection)
                flatObjects.Add(CreateRoad(piece));
            SetWorld(true);
            foreach (GameObject item in worldObjects) item.GetComponent<Renderer>().enabled = false;
            SetFlat(false, true);
        }

        public void SetFlat(bool collision, bool visible)
        {
            foreach (GameObject item in flatObjects)
            {
                item.GetComponent<Collider>().enabled = collision;
                item.GetComponent<Renderer>().enabled = visible;
            }
        }

        public void SetWorld(bool active)
        {
            foreach (GameObject item in worldObjects)
            {
                item.SetActive(active);
                item.GetComponent<Renderer>().enabled = active;
            }
        }

        /// <summary>
        /// 展开前确认落点位于圆弧道路的真实顶面
        /// </summary>
        public bool HasSurface(RoadData road, Vector3 target)
        {
            foreach (var source in sources)
            {
                if (source.Value != road || !(source.Key is MeshCollider meshCollider)) continue;
                Mesh mesh = meshCollider.sharedMesh;
                Vector3 point = meshCollider.transform.InverseTransformPoint(target);
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3 a = vertices[triangles[i]];
                    Vector3 b = vertices[triangles[i + 1]];
                    Vector3 c = vertices[triangles[i + 2]];
                    if (Vector3.Cross(b - a, c - a).y <= 0.000001f) continue;
                    float ab = (b.x - a.x) * (point.z - a.z) - (b.z - a.z) * (point.x - a.x);
                    float bc = (c.x - b.x) * (point.z - b.z) - (c.z - b.z) * (point.x - b.x);
                    float ca = (a.x - c.x) * (point.z - c.z) - (a.z - c.z) * (point.x - c.x);
                    if ((ab >= -0.0001f && bc >= -0.0001f && ca >= -0.0001f) ||
                        (ab <= 0.0001f && bc <= 0.0001f && ca <= 0.0001f)) return true;
                }
                return false;
            }
            return true;
        }
        public bool TryGetRoad(Collider collider, out RoadData road) => sources.TryGetValue(collider, out road);

        /// <summary>
        /// 判断落脚道路是否属于机关任一道路段
        /// </summary>
        public bool IsOnRoad(MapMechanism mechanism, RoadData road)
        {
            foreach (Collider segment in mechanism.Roads)
                if (sources.TryGetValue(segment, out RoadData source) && source == road) return true;
            return false;
        }

        /// <summary>
        /// 机关到位后更新原道路数据并保留来源对象
        /// </summary>
        public void RefreshRoads()
        {
            Physics.SyncTransforms();
            foreach (GameObject item in worldObjects)
            {
                Collider collider = item.GetComponent<Collider>();
                RoadData road = sources[collider];
                road.position = collider.bounds.center;
                road.size = collider.bounds.size;
            }
        }

        public void ClearProjection()
        {
            foreach (GameObject item in flatObjects)
            {
                item.SetActive(false);
                sources.Remove(item.GetComponent<Collider>());
                Object.Destroy(item);
            }
            flatObjects.Clear();
        }

        private GameObject CreateRoad(ProjectionData piece)
        {
            GameObject item = Object.Instantiate(projectionPrefab, root);
            item.name = "Projection_" + piece.source.name;
            item.transform.position = piece.position;
            item.transform.localScale = piece.size;
            sources.Add(item.GetComponent<Collider>(), piece.source);
            return item;
        }
    }
}
