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
            var colliders = new List<BoxCollider>(roadRoot.GetComponentsInChildren<BoxCollider>());
            foreach (MapMechanism mechanism in mechanisms)
                if (!colliders.Contains(mechanism.Road)) colliders.Add(mechanism.Road);
            foreach (BoxCollider collider in colliders)
            {
                var road = new RoadData(collider.name, collider.bounds.center, collider.bounds.size);
                data.roads.Add(road);
                worldObjects.Add(collider.gameObject);
                sources.Add(collider, road);
            }
        }

        public void CreatePreview(List<ProjectionData> projection)
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

        public bool TryGetRoad(Collider collider, out RoadData road) => sources.TryGetValue(collider, out road);

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
            item.name = "投影_" + piece.source.name;
            item.transform.position = piece.position;
            item.transform.localScale = piece.size;
            sources.Add(item.GetComponent<Collider>(), piece.source);
            return item;
        }
    }
}
