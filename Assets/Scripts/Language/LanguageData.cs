using System;
using UnityEngine;
namespace PlaneShift
{
    /// <summary>
    /// 保存导入的语言词条
    /// </summary>
    [CreateAssetMenu(menuName = "PlaneShift/Language Data")]
    public class LanguageData : ScriptableObject
    {
        /// <summary>
        /// 按语言列顺序保存同一词条的译文
        /// </summary>
        [Serializable]
        public class Entry
        {
            public string id;
            public string[] values;
        }
        public string[] languages;
        public Entry[] entries;
    }
}
