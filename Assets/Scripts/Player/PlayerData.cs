using System;
using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 保存角色移动参数与当前落脚道路
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public float moveSpeed = 4;
        public float gravity = 22;
        public float radius = 0.3f;
        public float height = 1.6f;
        /// <summary>
        /// 保存当前脚下的来源道路供空间展开使用
        /// </summary>
        [NonSerialized] public RoadData road;
        [NonSerialized] public Vector3 groundPoint;
        [NonSerialized] public bool grounded;
    }
}
