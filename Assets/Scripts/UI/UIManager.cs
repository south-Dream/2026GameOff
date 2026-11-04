using UnityEngine;
using UnityEngine.UI;
using UnityLightFrame;

namespace PlaneShift
{
    /// <summary>
    /// 接收玩法事件并显示中文模式与交互提示
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject interactionPanel;
        [SerializeField] private Text interactionText;
        [SerializeField] private Text modeText;
        [SerializeField] private Text noticeText;
        private EventCenter.IDisposable interactionSubscription;
        private EventCenter.IDisposable modeSubscription;
        private EventCenter.IDisposable noticeSubscription;
        private float noticeEnd;

        private void OnEnable()
        {
            interactionPanel.SetActive(false);
            noticeText.text = "";
            interactionSubscription = EventCenter.Instance.SubscribeEvent<InteractionInfo>(E_EventType.InteractionChanged, ShowInteraction);
            modeSubscription = EventCenter.Instance.SubscribeEvent<string>(E_EventType.ModeChanged, ShowMode);
            noticeSubscription = EventCenter.Instance.SubscribeEvent<string>(E_EventType.Notice, ShowNotice);
            EventCenter.Instance.TriggerEvent(E_EventType.RequestUI);
        }

        private void OnDisable()
        {
            interactionSubscription?.Invoke();
            modeSubscription?.Invoke();
            noticeSubscription?.Invoke();
            interactionPanel.SetActive(false);
        }

        private void Update()
        {
            if (Time.unscaledTime >= noticeEnd) noticeText.text = "";
        }

        private void ShowInteraction(InteractionInfo info)
        {
            interactionPanel.SetActive(info != null && info.visible);
            if (info == null || !info.visible) return;
            interactionText.text = info.text;
            interactionText.color = info.available ? Color.white : new Color(0.78f, 0.78f, 0.78f);
        }

        private void ShowMode(string text) => modeText.text = text;

        private void ShowNotice(string text)
        {
            noticeText.text = text;
            noticeEnd = Time.unscaledTime + 3;
        }
    }
}
