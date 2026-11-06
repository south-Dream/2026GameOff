using UnityEngine;
using UnityEngine.UI;
using UnityLightFrame;

namespace PlaneShift
{
    /// <summary>
    /// 显示共用玩法界面与关卡提示
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private GameObject interactionPanel;
        [SerializeField] private Text interactionText;
        [SerializeField] private Text modeText;
        [SerializeField] private Text noticeText;
        [SerializeField] private Text levelText;
        [SerializeField] private UIMgr uiMgr;
        [SerializeField] private GameObject completePanel;
        private EventCenter.IDisposable interactionSubscription;
        private EventCenter.IDisposable modeSubscription;
        private EventCenter.IDisposable noticeSubscription;
        private EventCenter.IDisposable levelSubscription;
        private EventCenter.IDisposable unloadSubscription;
        private EventCenter.IDisposable completeSubscription;
        private float noticeEnd;
        private InteractionInfo currentInteraction;
        private string modeId;
        private string noticeId;
        private LevelInfo currentLevel;
        private void ApplyLanguage()
        {
            if (currentInteraction != null && currentInteraction.visible) ShowInteraction(currentInteraction);
            if (!string.IsNullOrEmpty(modeId)) modeText.text = LanguageManager.Get(modeId);
            if (!string.IsNullOrEmpty(noticeId) && Time.unscaledTime < noticeEnd) noticeText.text = LanguageManager.Get(noticeId);
            if (currentLevel != null) levelText.text = LanguageManager.Get("ui.levelTitle", currentLevel.number, new LanguageManager.Word(currentLevel.level.title));
        }

        private void OnEnable()
        {
            ClearLevel();
            LanguageManager.Changed += ApplyLanguage;
            interactionSubscription = EventCenter.Instance.SubscribeEvent<InteractionInfo>(E_EventType.InteractionChanged, ShowInteraction);
            modeSubscription = EventCenter.Instance.SubscribeEvent<string>(E_EventType.ModeChanged, ShowMode);
            noticeSubscription = EventCenter.Instance.SubscribeEvent<string>(E_EventType.Notice, ShowNotice);
            levelSubscription = EventCenter.Instance.SubscribeEvent<LevelInfo>(E_EventType.LevelLoaded, ShowLevel);
            unloadSubscription = EventCenter.Instance.SubscribeEvent(E_EventType.LevelUnloading, ClearLevel);
            completeSubscription = EventCenter.Instance.SubscribeEvent(E_EventType.LevelCompleted, ShowComplete);
            EventCenter.Instance.TriggerEvent(E_EventType.RequestUI);
        }

        private void OnDisable()
        {
            LanguageManager.Changed -= ApplyLanguage;
            interactionSubscription?.Invoke();
            modeSubscription?.Invoke();
            noticeSubscription?.Invoke();
            levelSubscription?.Invoke();
            unloadSubscription?.Invoke();
            completeSubscription?.Invoke();
            ClearLevel();
        }

        private void Update()
        {
            if (Time.unscaledTime >= noticeEnd) noticeText.text = "";
        }

        private void ShowInteraction(InteractionInfo info)
        {
            currentInteraction = info;
            interactionPanel.SetActive(info != null && info.visible);
            if (info == null || !info.visible) return;
            interactionText.text = info.available ? LanguageManager.Get("ui.interaction", new LanguageManager.Word(info.text)) : LanguageManager.Get(info.text);
            interactionText.color = info.available ? Color.white : new Color(0.78f, 0.78f, 0.78f);
        }

        private void ShowMode(string text) { modeId = text; modeText.text = LanguageManager.Get(text); }

        private void ShowNotice(string text)
        {
            noticeId = text;
            noticeText.text = LanguageManager.Get(text);
            noticeEnd = Time.unscaledTime + 3;
        }

        private void ShowLevel(LevelInfo info)
        {
            currentLevel = info;
            levelText.text = LanguageManager.Get("ui.levelTitle", info.number, new LanguageManager.Word(info.level.title));
            completePanel.SetActive(false);
        }

        public void Restart()
        {
            if (completePanel.activeSelf) EventCenter.Instance.TriggerEvent(E_EventType.RestartRequested);
        }

        private void ShowComplete()
        {
            currentInteraction = null;
            noticeId = null;
            interactionPanel.SetActive(false);
            noticeText.text = "";
            completePanel.SetActive(true);
        }

        private void ClearLevel()
        {
            currentInteraction = null; modeId = null; noticeId = null; currentLevel = null;
            interactionPanel.SetActive(false);
            noticeText.text = "";
            modeText.text = "";
            levelText.text = "";
            completePanel.SetActive(false);
        }

        public void Show()
        {
            uiMgr.startUI.Hide();
            uiMgr.levelUI.Hide();
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
