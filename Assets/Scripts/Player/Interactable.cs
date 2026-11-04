using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 提供交互物的范围检测与执行入口
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [SerializeField] private string interactionName = "交互";
        [SerializeField, Min(0.5f)] private float interactionDistance = 3;
        [SerializeField] private Collider interactionArea;

        public string InteractionName => interactionName;

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
            if (!IsInRange(player)) { reason = "请靠近交互物"; return false; }
            if (!player.Data.grounded) { reason = "请先站稳"; return false; }
            return true;
        }

        public abstract bool TryInteract(PlayerManager player, out string message);
    }
}
