using UnityEngine;
using UnityLightFrame;

namespace PlaneShift
{
    /// <summary>
    /// 管理玩家操作模式与当前道路归属
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private PlayerData data = new PlayerData();
        [SerializeField] private CharacterController body;
        [SerializeField] private Transform model;
        private PlayerController controller;
        private MapManager map;
        private SpaceAxis? axis;
        private float plane;
        private bool canMove;
        private Interactable[] interactables;
        private Interactable interactionTarget;
        private InteractionInfo interactionInfo;
        private EventCenter.IDisposable refreshSubscription;

        public Interactable Target => interactionTarget;
        public Interactable LockedTarget { get; set; }
        public event System.Action<MechanismButton> MechanismAccepted;
        public PlayerData Data => data;
        public Vector3 Position => controller.Position;

        public void Init(MapManager mapManager)
        {
            map = mapManager;
            interactables = map.GetComponentsInChildren<Interactable>(true);
            controller = new PlayerController();
            controller.Init(body, model, data, map.Data.spawn);
            Physics.SyncTransforms();
            RefreshGround();
        }

        public void SetControl(bool active) => canMove = active;

        private void OnEnable() => refreshSubscription = EventCenter.Instance.SubscribeEvent(E_EventType.RequestUI, PublishInteraction);
        private void OnDisable()
        {
            refreshSubscription?.Invoke();
            ClearInteraction();
        }

        /// <summary>
        /// 选择范围内优先级最高的可用目标并通知界面
        /// </summary>
        public void UpdateInteraction(bool allowed)
        {
            if (!allowed) { ClearInteraction(); return; }
            RefreshGround();
            if (LockedTarget != null) interactionTarget = LockedTarget;
            if (LockedTarget == null)
            {
                interactionTarget = null;
                float nearest = float.MaxValue;
                int priority = int.MinValue;
                foreach (Interactable target in interactables)
                {
                    if (target == null || !target.IsInRange(this)) continue;
                    float distance = target.GetDistance(this);
                    int order = target.CanInteract(this, out _) ? target.Priority : int.MinValue;
                    if (order < priority || (order == priority && distance >= nearest)) continue;
                    nearest = distance;
                    priority = order;
                    interactionTarget = target;
                }
            }
            if (interactionTarget == null) { ClearInteraction(); return; }
            bool available = interactionTarget.CanInteract(this, out string reason);
            string text = available ? interactionTarget.InteractionName : reason;
            if (interactionInfo == null || !interactionInfo.visible || interactionInfo.available != available || interactionInfo.text != text)
            {
                interactionInfo = new InteractionInfo { visible = true, available = available, text = text };
                PublishInteraction();
            }
            if (available && interactionTarget is MechanismButton ready)
                EventCenter.Instance.TriggerEvent(E_EventType.MechanismReady, ready);
            if (InputManager.Instance.IsTrigger(E_InputEventType.Interact)) TryInteract();
        }

        public bool TryInteract()
        {
            if (interactionTarget == null || !canMove || axis.HasValue) return false;
            RefreshGround();
            if (!interactionTarget.TryInteract(this, out string message))
            {
                EventCenter.Instance.TriggerEvent(E_EventType.Notice, message);
                return false;
            }
            if (interactionTarget is MechanismButton button) MechanismAccepted?.Invoke(button);
            ClearInteraction();
            if (!string.IsNullOrEmpty(message)) EventCenter.Instance.TriggerEvent(E_EventType.Notice, message);
            return true;
        }

        public void ClearInteraction()
        {
            interactionTarget = null;
            if (interactionInfo == null || !interactionInfo.visible) return;
            interactionInfo = new InteractionInfo();
            PublishInteraction();
        }

        private void PublishInteraction() => EventCenter.Instance.TriggerEvent(E_EventType.InteractionChanged, interactionInfo);

        /// <summary>
        /// 跟随平台位移并保持脚底间距
        /// </summary>
        public void Carry(Vector3 delta) => controller.Teleport(Position + delta - Vector3.up * 0.025f);

        public void SetWorld(Vector3 position)
        {
            axis = null;
            controller.Teleport(position);
            RefreshGround();
        }

        public void SetFlat(SpaceAxis selectedAxis, float targetPlane, Vector3 position)
        {
            axis = selectedAxis;
            plane = targetPlane;
            controller.Teleport(position);
            RefreshGround();
        }

        public void Tick(Vector3 right, Vector3 forward)
        {
            if (!canMove) return;
            float horizontal = (InputManager.Instance.IsTrigger(E_InputEventType.Right) ? 1 : 0) - (InputManager.Instance.IsTrigger(E_InputEventType.Left) ? 1 : 0);
            float vertical = (InputManager.Instance.IsTrigger(E_InputEventType.Forward) ? 1 : 0) - (InputManager.Instance.IsTrigger(E_InputEventType.Back) ? 1 : 0);
            Vector3 direction = right * horizontal;
            if (!axis.HasValue || axis == SpaceAxis.Y) direction += forward * vertical;
            bool topView = axis == SpaceAxis.Y;
            Vector3 previous = Position;
            controller.Move(direction, data, topView, axis, plane);
            RefreshGround();
            if (topView && !data.grounded)
            {
                controller.Teleport(previous - Vector3.up * 0.025f);
                RefreshGround();
            }
        }

        public void RefreshGround()
        {
            data.grounded = map.TryGetGround(Position, 0.16f, out RoadData road, out Vector3 point);
            data.road = data.grounded ? road : null;
            data.groundPoint = point;
        }

    }
}
