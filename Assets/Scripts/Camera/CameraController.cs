using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 执行固定方向镜头与视图过渡
    /// </summary>
    public class CameraController
    {
        private UnityEngine.Camera camera;
        private Quaternion startRotation;
        private Quaternion targetRotation;
        private float changeStart;
        private CameraData data;
        private bool flat;

        public bool IsChanging => Time.time - changeStart < data.changeTime;
        public Vector3 Right => targetRotation * Vector3.right;
        public Vector3 Forward => targetRotation * Vector3.up;

        public void Init(UnityEngine.Camera sceneCamera, CameraData cameraData)
        {
            data = cameraData;
            camera = sceneCamera;
        }

        public void ChangeView(ViewDirection direction, bool compressed, bool immediate)
        {
            flat = compressed;
            if (direction == ViewDirection.Bottom) direction = ViewDirection.Top;
            Vector3 forward = CameraData.GetForward(direction);
            Vector3 up = Mathf.Abs(forward.y) > 0.9f ? Vector3.forward : Vector3.up;
            targetRotation = Quaternion.LookRotation(forward, up);
            if (!flat && Mathf.Abs(forward.y) < 0.9f)
                targetRotation *= Quaternion.Euler(data.worldPitch, 0, 0);
            startRotation = immediate ? targetRotation : camera.transform.rotation;
            changeStart = immediate ? Time.time - data.changeTime : Time.time;
            camera.orthographic = flat || Mathf.Abs(forward.y) > 0.9f;
            camera.orthographicSize = data.flatSize;
        }

        public void Tick(Vector3 target)
        {
            float t = data.changeTime <= 0 ? 1 : Mathf.Clamp01((Time.time - changeStart) / data.changeTime);
            Quaternion rotation = Quaternion.Slerp(startRotation, targetRotation, Mathf.SmoothStep(0, 1, t));
            camera.transform.SetPositionAndRotation(target - rotation * Vector3.forward * data.distance, rotation);
        }
    }
}
