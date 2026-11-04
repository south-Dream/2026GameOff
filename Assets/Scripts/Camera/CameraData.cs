using System;
using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 保存镜头参数与方向配置
    /// </summary>
    [Serializable]
    public class CameraData
    {
        public float distance = 22;
        public float worldPitch = 38;
        public float flatSize = 13;
        public float changeTime = 0.3f;

        public static SpaceAxis GetAxis(ViewDirection direction)
        {
            if (direction == ViewDirection.Top || direction == ViewDirection.Bottom) return SpaceAxis.Y;
            if (direction == ViewDirection.East || direction == ViewDirection.West) return SpaceAxis.X;
            return SpaceAxis.Z;
        }

        public static Vector3 GetForward(ViewDirection direction)
        {
            switch (direction)
            {
                case ViewDirection.East: return Vector3.left;
                case ViewDirection.South: return Vector3.back;
                case ViewDirection.West: return Vector3.right;
                case ViewDirection.Top: return Vector3.down;
                case ViewDirection.Bottom: return Vector3.up;
                default: return Vector3.forward;
            }
        }
    }
}
