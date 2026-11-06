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
        [SerializeField] private Collider road;
        [SerializeField] private Collider[] roads;
        [SerializeField] private Collider button;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip startSound;
        /// <summary>
        /// 机关运行期间循环播放的声音
        /// </summary>
        [SerializeField] private AudioClip moveSound;
        [SerializeField] private AudioClip stopSound;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 buttonPosition;
        private Vector3 fromPosition;
        private Quaternion fromRotation;
        private float elapsed;
        private bool active;
        private int turn;
        private float fromAngle;
        private float angle;

        public MechanismData Data => data;
        public Collider Road => Roads[0];
        public Collider[] Roads
        {
            get
            {
                if (roads == null || roads.Length == 0) roads = new[] { road };
                return roads;
            }
        }
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
            fromAngle = angle;
            if (data.turnAngle != 0) turn++;
            fromPosition = movingRoot.position;
            fromRotation = movingRoot.rotation;
            elapsed = 0;
            IsMoving = true;
            audioSource.clip = moveSound;
            audioSource.loop = true;
            audioSource.Play();
            audioSource.PlayOneShot(startSound);
        }

        /// <summary>
        /// 推进机关运动并返回本帧承载位移
        /// </summary>
        public Vector3 Tick(float deltaTime)
        {
            bool wasMoving = IsMoving;
            Vector3 previous = movingRoot.position;
            elapsed += deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.1f, data.duration));
            float smooth = Mathf.SmoothStep(0, 1, t);
            movingRoot.position = Vector3.Lerp(fromPosition, data.GetPosition(startPosition, active), smooth);
            if (data.turnAngle != 0)
            {
                angle = Mathf.Lerp(fromAngle, turn * 90f * Mathf.Sign(data.turnAngle), smooth);
                movingRoot.rotation = startRotation * Quaternion.Euler(0, angle, 0);
            }
            else movingRoot.rotation = fromRotation;
            if (data.carryPlayer) button.transform.position = buttonPosition + movingRoot.position - startPosition;
            IsMoving = t < 1;
            if (wasMoving && !IsMoving)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(stopSound);
            }
            return movingRoot.position - previous;
        }

        /// <summary>
        /// 暂停运转声并保留播放位置
        /// </summary>
        public void SetSoundPaused(bool paused)
        {
            if (!IsMoving) return;
            if (paused) audioSource.Pause();
            else audioSource.UnPause();
        }

        private void OnDisable() => audioSource.Stop();

        public void ResetState()
        {
            audioSource.Stop();
            active = false;
            turn = 0;
            angle = 0;
            fromAngle = 0;
            IsMoving = false;
            movingRoot.SetPositionAndRotation(startPosition, startRotation);
            button.transform.position = buttonPosition;
        }
    }
}
