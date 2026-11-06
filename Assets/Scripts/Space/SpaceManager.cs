using UnityEngine;
using UnityLightFrame;

namespace PlaneShift
{
    /// <summary>
    /// 管理空间切换并协调地图玩家和摄像机
    /// </summary>
    [DisallowMultipleComponent]
    public class SpaceManager : MonoBehaviour
    {
        [SerializeField] private SpaceData data = new SpaceData();
        [SerializeField] private PlayerManager player;
        [SerializeField] private CameraManager view;
        [SerializeField] private MapManager map;
        /// <summary>
        /// 记录压缩前的落脚道路 用于判断展开是否换路
        /// </summary>
        private RoadData anchor;
        /// <summary>
        /// 保留压缩前坐标用于恢复原深度
        /// </summary>
        private Vector3 savedPosition;
        private int horizontalView;
        private SpaceMode nextMode;
        private bool initialized;
        private bool inputEnabled;
        private EventCenter.IDisposable refreshSubscription;

        public int ExpansionCount { get; private set; }
        public SpaceData Data => data;
        public PlayerManager Player => player;
        public CameraManager View => view;
        public MapManager Map => map;
        public bool IsConfigured => player != null && view != null && map != null;
        public bool CanExit => initialized && inputEnabled && data.mode == SpaceMode.World && !map.IsMoving && !view.IsChanging;

        /// <summary>
        /// 在关卡包实例化完成后初始化玩法 默认等待流程开放输入
        /// </summary>
        public void Init()
        {
            if (initialized) return;
            map.Init();
            player.Init(map);
            view.Init();
            view.ChangeView(data.direction, false, true);
            initialized = true;
            player.SetControl(false);
            view.Tick(player.Position + Vector3.up * 0.7f);
        }

        public void SetControl(bool active)
        {
            inputEnabled = active;
            if (!initialized) return;
            ApplyControl();
            if (!active) player.ClearInteraction();
        }

        private void OnEnable() => refreshSubscription = EventCenter.Instance.SubscribeEvent(E_EventType.RequestUI, PublishMode);
        private void OnDisable() => refreshSubscription?.Invoke();
        private void Start() => PublishMode();

        private void Update()
        {
            if (!initialized || !inputEnabled) return;
            if (InputManager.Instance.IsTrigger(E_InputEventType.Reset)) { ResetPlayer(); return; }
            if (map.IsMoving)
            {
                player.ClearInteraction();
                map.TickMechanism(player);
                if (!map.IsMoving) { Notice("notice.arrived"); ApplyControl(); }
                return;
            }
            if (data.mode == SpaceMode.Changing)
            {
                player.ClearInteraction();
                if (!view.IsChanging)
                {
                    data.mode = nextMode;
                    ApplyControl();
                }
                return;
            }
            if (data.mode != SpaceMode.Flat) ReadDirection();
            if (data.mode == SpaceMode.World && InputManager.Instance.IsTrigger(E_InputEventType.CompressDown)) Compress();
            else if (data.mode == SpaceMode.Flat && InputManager.Instance.IsTrigger(E_InputEventType.CompressDown)) TryExpand();
            player.UpdateInteraction(data.mode == SpaceMode.World);
            if (!inputEnabled || !isActiveAndEnabled || map.IsMoving) return;
            player.Tick(view.Right, view.Forward);
            if (player.Position.y < map.Data.fallHeight) { ResetPlayer(); return; }
        }

        private void LateUpdate()
        {
            if (!initialized) return;
            view.Tick(player.Position + Vector3.up * 0.7f);
        }

        private void ApplyControl()
        {
            player.SetControl(inputEnabled && data.mode != SpaceMode.Changing && !map.IsMoving);
            PublishMode();
        }

        private void PublishMode()
        {
            if (!initialized) return;
            string text = data.mode == SpaceMode.Flat || (data.mode == SpaceMode.Changing && nextMode == SpaceMode.Flat)
                ? "mode.flat"
                : "mode.world";
            EventCenter.Instance.TriggerEvent(E_EventType.ModeChanged, text);
        }

        private void ReadDirection()
        {
            ViewDirection selected = data.direction;
            if (InputManager.Instance.IsTrigger(E_InputEventType.ViewLeft)) { horizontalView = (horizontalView + 3) % 4; selected = (ViewDirection)horizontalView; }
            if (InputManager.Instance.IsTrigger(E_InputEventType.ViewRight)) { horizontalView = (horizontalView + 1) % 4; selected = (ViewDirection)horizontalView; }
            if (InputManager.Instance.IsTrigger(E_InputEventType.ViewTop)) selected = ViewDirection.Top;
            if (InputManager.Instance.IsTrigger(E_InputEventType.ViewBottom)) selected = ViewDirection.Bottom;
            if (selected == data.direction) return;
            data.direction = selected;
            view.ChangeView(selected, false);
            if (data.mode == SpaceMode.World) ChangeMode(SpaceMode.World);
        }

        /// <summary>
        /// 按当前朝向直接切换地图碰撞与角色平面
        /// </summary>
        private void Compress()
        {
            if (!inputEnabled || map.IsMoving || data.mode != SpaceMode.World) return;
            player.RefreshGround();
            if (!player.Data.grounded) { Notice("notice.ground"); return; }
            anchor = player.Data.road;
            savedPosition = player.Position;
            data.axis = CameraData.GetAxis(data.direction);
            player.SetControl(false);
            player.ClearInteraction();
            map.CreateProjection(anchor, data.axis, CameraData.GetForward(data.direction));
            map.Compress();
            player.SetFlat(data.axis, map.Plane, MapData.Project(savedPosition, data.axis, map.Plane));
            view.ChangeView(data.direction, true);
            ChangeMode(SpaceMode.Flat);
        }

        /// <summary>
        /// 原道路沿用压缩前深度 换路后按当前道路恢复
        /// </summary>
        public bool CanExpand => data.mode == SpaceMode.Flat && player.Data.grounded && map.TryRestore(player.Position, player.Data.road,
            player.Data.road == anchor ? (float?)savedPosition[(int)data.axis] : null,
            player.Data.radius, player.Data.height, out _);

        private void TryExpand()
        {
            if (data.mode != SpaceMode.Flat) return;
            player.RefreshGround();
            if (!player.Data.grounded || !map.TryRestore(player.Position, player.Data.road,
                player.Data.road == anchor ? (float?)savedPosition[(int)data.axis] : null,
                player.Data.radius, player.Data.height, out Vector3 target))
            {
                Notice("notice.blocked");
                return;
            }
            map.Expand();
            player.SetWorld(target);
            view.ChangeView(data.direction, false);
            ChangeMode(SpaceMode.World);
            ExpansionCount++;
            Notice("notice.unfolded");
        }

        private void ChangeMode(SpaceMode target)
        {
            nextMode = target;
            data.mode = SpaceMode.Changing;
            player.SetControl(false);
            player.ClearInteraction();
            PublishMode();
        }

        public void ResetPlayer()
        {
            if (!initialized) return;
            player.ClearInteraction();
            map.Expand();
            map.ResetMechanisms();
            player.SetWorld(map.Data.spawn);
            view.ChangeView(data.direction, false, true);
            data.mode = SpaceMode.World;
            ApplyControl();
            Notice("notice.restart");
        }

        private void Notice(string text) => EventCenter.Instance.TriggerEvent(E_EventType.Notice, text);
    }
}
