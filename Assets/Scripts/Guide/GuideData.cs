using System;
using UnityEngine;
using UnityLightFrame;
namespace PlaneShift
{
    /// <summary>
    /// 保存单步说明与练习时开放的输入
    /// </summary>
    [Serializable]
    public class GuideEntry
    {
        public GuideStep step;
        public string title;
        [TextArea] public string body;
        public E_InputEventType[] inputs;
    }
    /// <summary>
    /// 保存引导说明
    /// </summary>
    [CreateAssetMenu(menuName = "PlaneShift/Guide Data")]
    public class GuideData : ScriptableObject
    {
        public GuideEntry[] entries;
        public GuideEntry Get(GuideStep step) => Array.Find(entries, entry => entry.step == step);
    }
}
