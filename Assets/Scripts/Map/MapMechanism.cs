using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 按地图指令切换机关位置与朝向
    /// </summary>
    public class MapMechanism : MonoBehaviour
    {
        [SerializeField] private MechanismData data = new MechanismData();
        [SerializeField] private Transform movingRoot;
        [SerializeField] private BoxCollider road;
        [SerializeField] private Collider button;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 buttonPosition;
        private Vector3 fromPosition;
        private Quaternion fromRotation;
        private float elapsed;
        private bool active;

        public MechanismData Data => data;
        public BoxCollider Road => road;
        public Collider Button => button;
        public bool IsMoving { get; private set; }

        public void Init()
        {
            startPosition = movingRoot.position;
            startRotation = movingRoot.rotation;
            buttonPosition = button.transform.position;
        }

        public void StartMove()
        {
            active = !active;
            fromPosition = movingRoot.position;
            fromRotation = movingRoot.rotation;
            elapsed = 0;
            IsMoving = true;
        }

        /// <summary>
        /// 推进机关运动并返回本帧承载位移
        /// </summary>
        public Vector3 Tick(float deltaTime)
        {
            Vector3 previous = movingRoot.position;
            elapsed += deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.1f, data.duration));
            float smooth = Mathf.SmoothStep(0, 1, t);
            movingRoot.position = Vector3.Lerp(fromPosition, data.GetPosition(startPosition, active), smooth);
            movingRoot.rotation = Quaternion.Slerp(fromRotation,
                startRotation * Quaternion.Euler(0, active ? data.turnAngle : 0, 0), smooth);
            if (data.carryPlayer) button.transform.position = buttonPosition + movingRoot.position - startPosition;
            IsMoving = t < 1;
            return movingRoot.position - previous;
        }

        public void ResetState()
        {
            active = false;
            IsMoving = false;
            movingRoot.SetPositionAndRotation(startPosition, startRotation);
            button.transform.position = buttonPosition;
        }
    }
}
