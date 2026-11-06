using UnityEngine;
using UnityEngine.UI;

namespace PlaneShift
{
    /// <summary>
    /// 显示单个关卡节点并转交选择
    /// </summary>
    public class LevelNode : MonoBehaviour
    {
        [SerializeField] private LevelUI owner;
        [SerializeField] private int index;
        [SerializeField] private Button button;
        [SerializeField] private Image face;
        [SerializeField] private GameObject selected;
        [SerializeField] private GameObject completed;
        [SerializeField] private Text status;
        [SerializeField] private Text number;

        private string statusId;
        private void OnEnable() => LanguageManager.Changed += ApplyLanguage;
        private void OnDisable() => LanguageManager.Changed -= ApplyLanguage;
        private void ApplyLanguage() { if (statusId != null) status.text = LanguageManager.Get(statusId); }

        public void Select() => owner.Select(index);

        public void SetState(bool available, bool isSelected, bool isComplete)
        {
            button.interactable = available;
            face.color = !available ? new Color(0.79f, 0.82f, 0.76f) :
                isSelected ? new Color(0.20f, 0.53f, 0.52f) : new Color(0.96f, 0.92f, 0.79f);
            selected.SetActive(available && isSelected);
            completed.SetActive(isComplete);
            statusId = !available ? "ui.pending" : isComplete ? "ui.completed" : "ui.available";
            ApplyLanguage();
            number.color = isSelected ? new Color(0.97f, 0.94f, 0.84f) : new Color(0.12f, 0.27f, 0.28f);
        }
    }
}
