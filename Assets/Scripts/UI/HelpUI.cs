using UnityEngine;
namespace PlaneShift
{
    /// <summary>
    /// 叠加显示玩法说明
    /// </summary>
    public class HelpUI : MonoBehaviour
    {
        public UIMgr ui;
        public void Show() { gameObject.SetActive(true); ui.guide.SetHelp(true); }
        public void Hide()
        {
            if (!gameObject.activeSelf) return;
            gameObject.SetActive(false);
            ui.record.ReadHelp();
            ui.guide.SetHelp(false);
        }
    }
}
