using System;

namespace PlaneShift
{
    public enum SpaceAxis { X, Y, Z }
    public enum SpaceMode { World = 0, Flat = 2, Changing = 3 }
    public enum ViewDirection { North, East, South, West, Top, Bottom }

    /// <summary>
    /// 保存空间模式与本次压缩方向
    /// </summary>
    [Serializable]
    public class SpaceData
    {
        public SpaceMode mode = SpaceMode.World;
        public ViewDirection direction = ViewDirection.North;
        /// <summary>
        /// 确认压缩后固定使用此轴 镜头过渡不改变投影方向
        /// </summary>
        public SpaceAxis axis = SpaceAxis.Z;
    }
}
