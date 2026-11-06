using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 保存关卡包引用并初始化玩法与出口
    /// </summary>
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private SpaceManager space;

        public SpaceManager Space => space;
        public bool IsConfigured => space != null && space.IsConfigured;
        public bool CanExit => space.CanExit;

        public void Init(LevelManager manager)
        {
            foreach (LevelExit exit in GetComponentsInChildren<LevelExit>(true)) exit.Init(manager, this);
            space.Init();
        }

        public void SetControl(bool active) => space.SetControl(active);
    }
}
