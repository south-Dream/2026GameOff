using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 提供交互物的范围检测与执行入口
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] private string interactionName = "interact.generic";
        [SerializeField, Min(0.5f)] private float interactionDistance = 3;
        [SerializeField] private Collider interactionArea;

        public virtual string InteractionName => interactionName;

        /// <summary>
        /// 同范围内优先选择数值更大的可用交互
        /// </summary>
        public virtual int Priority => 0;

        public float GetDistance(PlayerManager player)
        {
            Vector3 hand = player.Position + Vector3.up * 0.8f;
            Vector3 point = interactionArea != null ? interactionArea.bounds.ClosestPoint(hand) : transform.position;
            return Vector3.Distance(hand, point);
        }

        public bool IsInRange(PlayerManager player) => isActiveAndEnabled && GetDistance(player) <= interactionDistance;

        public virtual bool CanInteract(PlayerManager player, out string reason)
        {
            reason = "";
            if (!IsInRange(player)) { reason = "interact.near"; return false; }
            if (!player.Data.grounded) { reason = "interact.grounded"; return false; }
            return true;
        }

        public abstract bool TryInteract(PlayerManager player, out string message);
    }
}
