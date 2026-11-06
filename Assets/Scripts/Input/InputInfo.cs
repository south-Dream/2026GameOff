using UnityEngine;

namespace UnityLightFrame
{
    /// <summary>
    /// 保存输入绑定及启用状态
    /// </summary>
    public struct InputInfo
    {
        public enum E_InputType { Key, Mouse }
        public enum E_TriggerType { Down, Press, Up }
        public E_InputType inputType;
        public E_TriggerType triggerType;
        public KeyCode keyCode;
        public int mouseId;
        public bool isActive;

        public InputInfo(E_TriggerType trigger, KeyCode key, bool active = true)
        {
            inputType = E_InputType.Key;
            triggerType = trigger;
            keyCode = key;
            mouseId = -1;
            isActive = active;
        }

        public InputInfo(E_TriggerType trigger, int mouse, bool active = true)
        {
            inputType = E_InputType.Mouse;
            triggerType = trigger;
            keyCode = KeyCode.None;
            mouseId = mouse;
            isActive = active;
        }
    }
}
