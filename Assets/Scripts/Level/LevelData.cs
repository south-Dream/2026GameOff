using System;
using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 保存关卡顺序与各关入口配置
    /// </summary>
    [CreateAssetMenu(menuName = "PlaneShift/Level Data")]
    public class LevelData : ScriptableObject
    {
        public LevelEntry[] levels;
    }

    /// <summary>
    /// 保存单关预制体与选关说明
    /// </summary>
    [Serializable]
    public class LevelEntry
    {
        public LevelController prefab;
        public string title;
        [TextArea(2, 4)] public string description;
    }

    /// <summary>
    /// 向共用界面传递当前关卡
    /// </summary>
    public class LevelInfo
    {
        public LevelEntry level;
        public int number;
        public int total;
    }
}
