using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 保存道路尺寸与原始位置
    /// </summary>
    [Serializable]
    public class RoadData
    {
        public string name;
        public Vector3 position;
        public Vector3 size;

        public float Top => position.y + size.y * 0.5f;

        public RoadData(string name, Vector3 position, Vector3 size)
        {
            this.name = name;
            this.position = position;
            this.size = size;
        }
    }

    /// <summary>
    /// 保存投影碰撞段及其来源道路
    /// </summary>
    public class ProjectionData
    {
        /// <summary>
        /// 决定角色在此段道路展开时使用的三维来源
        /// </summary>
        public RoadData source;
        public Vector3 position;
        public Vector3 size;
    }

    /// <summary>
    /// 保存地图配置并计算投影形状与恢复位置
    /// </summary>
    [Serializable]
    public class MapData
    {
        public List<RoadData> roads = new List<RoadData>();
        public Vector3 spawn = new Vector3(-6, 2.5f, -6);
        public float fallHeight = -12;

        public static Vector3 Project(Vector3 point, SpaceAxis axis, float plane)
        {
            point[(int)axis] = plane;
            return point;
        }

        /// <summary>
        /// 保留未压缩坐标并使用来源道路中心深度恢复落脚位置
        /// </summary>
        public static Vector3 Restore(Vector3 foot, RoadData road, SpaceAxis axis)
        {
            foot[(int)axis] = road.position[(int)axis];
            foot.y = road.Top;
            return foot;
        }

        /// <summary>
        /// 按锚点和配置顺序裁剪同面重叠区域并保留唯一来源
        /// </summary>
        public static List<ProjectionData> CreateProjection(IReadOnlyList<RoadData> roads, RoadData anchor, SpaceAxis axis)
        {
            var result = new List<ProjectionData>();
            if (anchor == null) return result;
            AddRoad(result, anchor, anchor, axis);
            for (int i = 0; i < roads.Count; i++)
                if (roads[i] != anchor) AddRoad(result, roads[i], anchor, axis);
            return result;
        }

        private static void AddRoad(List<ProjectionData> result, RoadData road, RoadData anchor, SpaceAxis axis)
        {
            int horizontal = axis == SpaceAxis.X ? 2 : 0;
            int vertical = axis == SpaceAxis.Y ? 2 : 1;
            float plane = axis == SpaceAxis.Y ? anchor.Top : anchor.position[(int)axis];
            var pending = new List<Rect> { new Rect(road.position[horizontal] - road.size[horizontal] / 2,
                road.position[vertical] - road.size[vertical] / 2, road.size[horizontal], road.size[vertical]) };

            for (int i = 0; i < result.Count; i++)
            {
                ProjectionData existing = result[i];
                if (axis != SpaceAxis.Y && Math.Abs(existing.source.Top - road.Top) > 0.001f) continue;
                Rect occupied = new Rect(existing.position[horizontal] - existing.size[horizontal] / 2,
                    existing.position[vertical] - existing.size[vertical] / 2,
                    existing.size[horizontal], existing.size[vertical]);
                var next = new List<Rect>();
                foreach (Rect rect in pending) Subtract(rect, occupied, next);
                pending = next;
            }

            foreach (Rect rect in pending)
            {
                Vector3 position = road.position;
                Vector3 size = road.size;
                position[horizontal] = rect.center.x;
                position[vertical] = rect.center.y;
                size[horizontal] = rect.width;
                size[vertical] = rect.height;
                if (axis == SpaceAxis.Y)
                {
                    size.y = 0.6f;
                    position.y = plane - size.y / 2;
                }
                else
                {
                    size[(int)axis] = 2.4f;
                    position[(int)axis] = plane;
                }
                result.Add(new ProjectionData { source = road, position = position, size = size });
            }
        }

        private static void Subtract(Rect rect, Rect occupied, List<Rect> result)
        {
            float left = Math.Max(rect.xMin, occupied.xMin);
            float right = Math.Min(rect.xMax, occupied.xMax);
            float bottom = Math.Max(rect.yMin, occupied.yMin);
            float top = Math.Min(rect.yMax, occupied.yMax);
            if (right <= left || top <= bottom) { result.Add(rect); return; }
            AddRect(result, rect.xMin, rect.yMin, left - rect.xMin, rect.height);
            AddRect(result, right, rect.yMin, rect.xMax - right, rect.height);
            AddRect(result, left, rect.yMin, right - left, bottom - rect.yMin);
            AddRect(result, left, top, right - left, rect.yMax - top);
        }

        private static void AddRect(List<Rect> result, float x, float y, float width, float height)
        {
            if (width > 0.001f && height > 0.001f) result.Add(new Rect(x, y, width, height));
        }
    }
}
