using System;
using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 保存机关运动与交互参数
    /// </summary>
    [Serializable]
    public class MechanismData
    {
        public Vector3 moveOffset;
        public float turnAngle;
        [Min(0.1f)] public float duration = 1.5f;
        [Min(0.5f)] public float useDistance = 3;
        public bool carryPlayer;
        /// <summary>
        /// 允许角色在台外召回已升起的升降台
        /// </summary>
        public bool allowCall;

        public Vector3 GetPosition(Vector3 start, bool active) => active ? start + moveOffset : start;
    }
}
