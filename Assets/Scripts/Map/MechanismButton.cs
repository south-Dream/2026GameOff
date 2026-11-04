using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 将按钮交互交给地图中的目标机关
    /// </summary>
    public class MechanismButton : Interactable
    {
        [SerializeField] private MapMechanism mechanism;
        private MapManager map;

        public void Init(MapManager manager) => map = manager;

        public override bool CanInteract(PlayerManager player, out string reason)
        {
            if (!base.CanInteract(player, out reason)) return false;
            if (map == null) { reason = "机关尚未准备好"; return false; }
            return map.CanUse(mechanism, player, out reason);
        }

        public override bool TryInteract(PlayerManager player, out string message)
        {
            if (!CanInteract(player, out message)) return false;
            return map.TryUse(mechanism, player, out message);
        }
    }
}
