using UnityEngine;
using UnityEngine.UI;
namespace PlaneShift
{
    /// <summary>
    /// 显示阅读与练习说明
    /// </summary>
    public class GuideUI : MonoBehaviour
    {
        public GuideManager guide;
        public Text title;
        public Text body;
        public GameObject continueButton;
        public GameObject shade;
        public GameObject readingPanel;
        public GameObject practicePanel;
        public Text practiceTitle;
        public Text practiceBody;
        private GuideEntry current;
        private void OnEnable() { LanguageManager.Changed += Apply; Apply(); }
        private void OnDisable() => LanguageManager.Changed -= Apply;
        private void Apply()
        {
            if (current == null) return;
            practiceTitle.text = title.text = LanguageManager.Get(current.title);
            practiceBody.text = body.text = LanguageManager.Get(current.body);
        }
        public void Show(GuideEntry entry, bool reading)
        {
            readingPanel.SetActive(reading);
            practicePanel.SetActive(!reading);
            current = entry;
            Apply();
            continueButton.SetActive(reading);
            shade.SetActive(reading);
            gameObject.SetActive(true);
        }
        public void Hide() => gameObject.SetActive(false);
        public void Continue() => guide.BeginPractice();
    }
}
