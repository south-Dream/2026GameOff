using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 显示开始界面并关闭其他主界面
    /// </summary>
    public class StartUI : MonoBehaviour
    {
        [SerializeField] private UIMgr uiMgr;

        public void Show()
        {
            uiMgr.levels.Leave();
            uiMgr.levelUI.Hide();
            uiMgr.gameUI.Hide();
            uiMgr.menuRoot.SetActive(true);
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
