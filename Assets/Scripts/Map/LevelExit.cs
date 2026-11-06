using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 在出口范围内接收交互并请求进入下一关
    /// </summary>
    public class LevelExit : Interactable
    {
        private LevelManager manager;
        private LevelController level;

        public override string InteractionName => manager != null && manager.HasNext ? "exit.next" : "exit.finish";
        public override int Priority => 1;

        public void Init(LevelManager owner, LevelController source)
        {
            manager = owner;
            level = source;
        }

        public override bool CanInteract(PlayerManager player, out string reason)
        {
            if (!base.CanInteract(player, out reason)) return false;
            if (manager == null || level == null || manager.Current != level)
            { reason = "exit.wait"; return false; }
            if (!level.CanExit) { reason = "exit.unfold"; return false; }
            return true;
        }

        public override bool TryInteract(PlayerManager player, out string message)
        {
            if (!CanInteract(player, out message)) return false;
            if (!manager.TryFinishLevel(level)) { message = "exit.unavailable"; return false; }
            message = "";
            return true;
        }
    }
}
