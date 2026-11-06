using UnityEngine;
using UnityLightFrame;

namespace PlaneShift
{
    /// <summary>
    /// 直接关联场景界面与关卡入口
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class UIMgr : MonoBehaviour
    {
        public LanguageData languageData;
        public HelpUI helpUI;
        public GuideUI guideUI;
        public RecordManager record;
        public GuideManager guide;
        public StartUI startUI;
        public LevelUI levelUI;
        public GameUI gameUI;
        public SettingsUI settingsUI;
        public LevelManager levels;
        public GameObject menuRoot;
        private EventCenter.IDisposable loadedSubscription;
        private EventCenter.IDisposable unloadSubscription;

        private void Awake()
        {
            LanguageManager.Init(languageData, PlayerPrefs.GetString("Language", "zh-CN"));
            record.Init();
            levels.record = record;
            BindInput();
            guide.Init();
        }

        /// <summary>
        /// 登记默认按键 重新启用时恢复输入映射
        /// </summary>
        private void BindInput()
        {
            KeyCode[] keys = { KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftShift, KeyCode.R, KeyCode.F };
            var actions = (E_InputEventType[])System.Enum.GetValues(typeof(E_InputEventType));
            for (int i = 0; i < keys.Length; i++)
                InputManager.Instance.SetInput(actions[i], i < 4 ? InputInfo.E_TriggerType.Press : InputInfo.E_TriggerType.Down, keys[i]);
        }

        private void OnEnable()
        {
            BindInput();
            guide.Apply();
            loadedSubscription = EventCenter.Instance.SubscribeEvent<LevelInfo>(E_EventType.LevelLoaded, LevelLoaded);
            unloadSubscription = EventCenter.Instance.SubscribeEvent(E_EventType.LevelUnloading, LevelUnloading);
        }

        private void OnDisable()
        {
            loadedSubscription?.Invoke();
            unloadSubscription?.Invoke();
        }

        private void Start()
        {
            settingsUI.Init();
            startUI.Show();
            if (!record.Data.helpRead) helpUI.Show();
        }

        public void PlayLevel(int index)
        {
            if (levels.LoadLevel(index)) gameUI.Show();
        }

        private void LevelLoaded(LevelInfo info) => menuRoot.SetActive(false);
        private void LevelUnloading() => menuRoot.SetActive(true);

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
