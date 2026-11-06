using UnityEngine;
using UnityEngine.UI;

namespace PlaneShift
{
    /// <summary>
    /// 显示关卡节点与当前选择的关卡信息
    /// </summary>
    public class LevelUI : MonoBehaviour
    {
        [SerializeField] private UIMgr uiMgr;
        [SerializeField] private LevelNode[] nodes;
        [SerializeField] private Text title;
        [SerializeField] private Text description;
        [SerializeField] private Text progress;
        private int selected;

        private void OnEnable() => LanguageManager.Changed += ApplyLanguage;
        private void OnDisable() => LanguageManager.Changed -= ApplyLanguage;
        private void ApplyLanguage() => Select(selected);

        public void Show()
        {
            uiMgr.levels.Leave();
            uiMgr.startUI.Hide();
            uiMgr.gameUI.Hide();
            uiMgr.menuRoot.SetActive(true);
            gameObject.SetActive(true);
            Select(Mathf.Clamp(uiMgr.levels.CurrentIndex, 0, uiMgr.levels.Data.levels.Length - 1));
        }

        public void Hide() => gameObject.SetActive(false);

        public void Select(int index)
        {
            selected = index;
            LevelEntry[] levels = uiMgr.levels.Data.levels;
            int completed = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                bool available = i < levels.Length;
                bool done = available && uiMgr.levels.IsComplete(i);
                if (done) completed++;
                nodes[i].SetState(available, i == selected, done);
            }
            title.text = LanguageManager.Get("ui.levelTitle", index + 1, new LanguageManager.Word(levels[index].title));
            description.text = LanguageManager.Get(levels[index].description);
            progress.text = LanguageManager.Get("ui.progress", completed, levels.Length);
        }

        public void Play() => uiMgr.PlayLevel(selected);
    }
}
