using System.Collections.Generic;
using UnityEngine;

namespace PlaneShift
{
    public enum GuideStep { Move, ViewSide, ViewTop, Compress, FlatMove, Expand, Interact }

    /// <summary>
    /// 保存首次体验与关卡记录的默认值
    /// </summary>
    [CreateAssetMenu(menuName = "PlaneShift/Record Data")]
    public class RecordData : ScriptableObject
    {
        public bool helpRead;
        public List<GuideStep> steps = new List<GuideStep>();
        public List<int> visited = new List<int>();
        public List<int> completed = new List<int>();
    }
}
