using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 管理观察方向与摄像机切换指令
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CameraData data = new CameraData();
        [SerializeField] private UnityEngine.Camera sceneCamera;
        private CameraController controller;

        public bool IsChanging => controller.IsChanging;
        public Vector3 Right => Vector3.ProjectOnPlane(controller.Right, Vector3.up).normalized;
        public Vector3 Forward => Vector3.ProjectOnPlane(controller.Forward, Vector3.up).normalized;

        public void Init()
        {
            controller = new CameraController();
            controller.Init(sceneCamera, data);
        }

        public void ChangeView(ViewDirection direction, bool flat, bool immediate = false) => controller.ChangeView(direction, flat, immediate);
        public void Tick(Vector3 target) => controller.Tick(target);
    }
}
