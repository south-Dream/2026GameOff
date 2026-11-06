using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityLightFrame
{
    /// <summary>
    /// 按绑定读取输入并控制单项输入是否启用
    /// </summary>
    public sealed class InputManager
    {
        public static InputManager Instance { get; } = new InputManager();
        public bool totalToggle = true;
        public bool keyToggle = true;
        public bool mouseToggle = true;
        public event Func<bool> isPointerOverUI;
        private readonly Dictionary<E_InputEventType, InputInfo> inputMap = new Dictionary<E_InputEventType, InputInfo>();

        private InputManager() { }

        public bool IsTrigger(E_InputEventType type)
        {
            if (!totalToggle || !inputMap.TryGetValue(type, out InputInfo info) || !info.isActive) return false;
            if (info.inputType == InputInfo.E_InputType.Key)
            {
                if (!keyToggle) return false;
                switch (info.triggerType)
                {
                    case InputInfo.E_TriggerType.Down: return Input.GetKeyDown(info.keyCode);
                    case InputInfo.E_TriggerType.Up: return Input.GetKeyUp(info.keyCode);
                    default: return Input.GetKey(info.keyCode);
                }
            }
            if (!mouseToggle || (isPointerOverUI != null && isPointerOverUI())) return false;
            switch (info.triggerType)
            {
                case InputInfo.E_TriggerType.Down: return Input.GetMouseButtonDown(info.mouseId);
                case InputInfo.E_TriggerType.Up: return Input.GetMouseButtonUp(info.mouseId);
                default: return Input.GetMouseButton(info.mouseId);
            }
        }

        public void SetInput(E_InputEventType type, InputInfo.E_TriggerType trigger, KeyCode key)
            => inputMap[type] = new InputInfo(trigger, key);
        public void SetInput(E_InputEventType type, InputInfo.E_TriggerType trigger, int mouse)
            => inputMap[type] = new InputInfo(trigger, mouse);
        public InputInfo GetInput(E_InputEventType type) => inputMap[type];
        public void RemoveInput(E_InputEventType type) => inputMap.Remove(type);
        public void Active(E_InputEventType type) => SetActive(type, true);
        public void Unactive(E_InputEventType type) => SetActive(type, false);

        /// <summary>
        /// 读取物理按住状态用于暂停恢复与等待松键
        /// </summary>
        public bool IsHeld(E_InputEventType type)
        {
            InputInfo info = inputMap[type];
            return info.inputType == InputInfo.E_InputType.Key ? Input.GetKey(info.keyCode) : Input.GetMouseButton(info.mouseId);
        }

        private void SetActive(E_InputEventType type, bool active)
        {
            InputInfo info = inputMap[type];
            info.isActive = active;
            inputMap[type] = info;
        }

        public void Clear()
        {
            inputMap.Clear();
            isPointerOverUI = null;
        }
    }
}
