using System;
using UnityEngine;
using UnityLightFrame;
namespace PlaneShift
{
    /// <summary>
    /// 根据实际操作结果推进引导
    /// </summary>
    public class GuideManager : MonoBehaviour
    {
        public LevelManager levels;
        public RecordManager record;
        public GuideData data;
        public GuideUI ui;
        private SpaceManager space;
        private GuideStep? step;
        private bool reading;
        private bool help;
        private bool release;
        private int moved;
        private int expansions;
        private Vector3 previous;
        private Vector3 origin;
        private ViewDirection direction;
        private MapMechanism pending;
        private MechanismButton target;
        private EventCenter.IDisposable readySubscription;
        private void OnEnable() => readySubscription = EventCenter.Instance.SubscribeEvent<MechanismButton>(E_EventType.MechanismReady, OfferMechanism);
        public void Init() { ui.Hide(); Apply(); }
        /// <summary>
        /// 设置玩法说明暂停
        /// </summary>
        public void SetHelp(bool value) { help = value; levels.SetHelpPaused(value); Apply(); }
        /// <summary>
        /// 进入当前步骤练习
        /// </summary>
        public void BeginPractice() { if (!reading || !step.HasValue) return; reading = false; release = true; ui.Show(data.Get(step.Value), false); Apply(); }
        /// <summary>
        /// 恢复当前步骤输入权限
        /// </summary>
        public void Apply()
        {
            var input = InputManager.Instance;
            input.totalToggle = !help && !levels.Paused;
            foreach (E_InputEventType key in Enum.GetValues(typeof(E_InputEventType)))
            {
                bool allow = !release && (!step.HasValue || (!reading && Allowed(key)));
                if (allow) input.Active(key); else input.Unactive(key);
            }
        }
        private bool Allowed(E_InputEventType key)
        {
            return Array.IndexOf(data.Get(step.Value).inputs, key) >= 0;
        }
        private void Show(GuideStep next)
        {
            step = next; reading = true; pending = null; expansions = space.ExpansionCount;
            previous = space.Player.Position; origin = previous; direction = space.Data.direction; moved = 0;
            ui.Show(data.Get(next), true); Apply();
        }
        private void Finish()
        {
            record.Finish(step.Value); space.Player.LockedTarget = null; step = null; ui.Hide(); release = true; Apply();
        }
        private void Accepted(MechanismButton button) { if (target == button) pending = button.Mechanism; }
        /// <summary>
        /// 在交互读取按键前显示首次说明并锁定目标
        /// </summary>
        private void OfferMechanism(MechanismButton button)
        {
            if (step.HasValue || !levels.CanGuide || help || release) return;
            Bind(levels.Current.Space);
            if (space.Data.mode != SpaceMode.World || space.View.IsChanging || NextStep(record, SpaceMode.World).HasValue) return;
            if (record.HasStep(GuideStep.Interact)) return;
            target = button;
            space.Player.LockedTarget = button;
            Show(GuideStep.Interact);
        }
        private void Bind(SpaceManager current)
        {
            if (current == space) return;
            if (space != null) space.Player.MechanismAccepted -= Accepted;
            space = current; step = null; pending = null; target = null; ui.Hide();
            if (space != null) space.Player.MechanismAccepted += Accepted;
            Apply();
        }
        private void Update()
        {
            SpaceManager current = levels.Current != null ? levels.Current.Space : null;
            Bind(current);
            if (space == null || levels.Paused || help) return;
            if (release)
            {
                foreach (E_InputEventType key in Enum.GetValues(typeof(E_InputEventType)))
                    if (InputManager.Instance.IsHeld(key)) return;
                release = false; Apply();
            }
            if (!step.HasValue)
            {
                if (!levels.CanGuide || space.Map.IsMoving || !space.Player.Data.grounded || space.View.IsChanging) return;
                GuideStep? next = NextStep(record, space.Data.mode);
                if (next.HasValue) { Show(next.Value); return; }
                if (space.Data.mode != SpaceMode.World) return;
                return;
            }
            if (reading) return;
            if ((step == GuideStep.FlatMove || (step == GuideStep.Expand && space.ExpansionCount == expansions)) && space.Data.mode == SpaceMode.World)
            {
                Show(GuideStep.Compress);
                return;
            }
            bool settled = !space.View.IsChanging && space.Data.mode != SpaceMode.Changing;
            switch (step.Value)
            {
                case GuideStep.Move:
                    Vector3 delta = space.Player.Position - previous;
                    Vector3 right = space.View.Right; right.y = 0; right.Normalize();
                    Vector3 forward = space.View.Forward; forward.y = 0; forward.Normalize();
                    Vector3[] directions = { -right, right, forward, -forward };
                    delta.y = 0;
                    for (int i = 0; i < 4; i++)
                        if (InputManager.Instance.IsTrigger((E_InputEventType)i) && Vector3.Dot(delta, directions[i]) > 0.0001f) moved |= 1 << i;
                    if (moved == 15) Finish(); break;
                case GuideStep.ViewSide: if (settled && space.Data.direction != direction) Finish(); break;
                case GuideStep.ViewTop: if (settled && (space.Data.direction == ViewDirection.Top || space.Data.direction == ViewDirection.Bottom)) Finish(); break;
                case GuideStep.Compress: if (settled && space.Data.mode == SpaceMode.Flat) Finish(); break;
                case GuideStep.FlatMove: if ((space.Player.Position - origin).sqrMagnitude >= 1f && space.Player.Data.grounded && space.CanExpand) Finish(); break;
                case GuideStep.Expand: if (settled && space.Data.mode == SpaceMode.World && space.ExpansionCount > expansions) Finish(); break;
                default: if (pending != null && !pending.IsMoving) Finish(); break;
            }
            previous = space.Player.Position;
        }
        /// <summary>
        /// 按空间模式选择下一步骤
        /// </summary>
        public static GuideStep? NextStep(RecordManager record, SpaceMode mode)
        {
            for (int i = 0; i <= 5; i++)
            {
                GuideStep next = (GuideStep)i;
                if (record.HasStep(next)) continue;
                if (next == GuideStep.FlatMove || next == GuideStep.Expand)
                    return mode == SpaceMode.Flat ? next : mode == SpaceMode.World ? GuideStep.Compress : (GuideStep?)null;
                return mode == SpaceMode.World ? next : (GuideStep?)null;
            }
            return null;
        }
        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            readySubscription?.Invoke();
            if (space != null) { space.Player.MechanismAccepted -= Accepted; space.Player.LockedTarget = null; }
            space = null;
            step = null; release = false; ui.Hide(); Apply();
        }
    }
}
