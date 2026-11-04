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
        private RoadData anchor;
        private Vector3 savedPosition;
        private int horizontalView;
        private SpaceMode nextMode;
        private bool completed;
        /// <summary>
        /// 保存松开按键后的确认请求并等待镜头过渡完成
        /// </summary>
        private bool pendingConfirm;
        private EventCenter.IDisposable refreshSubscription;

        private void Awake()
        {
            map.Init();
            player.Init(map);
            view.Init();
            view.ChangeView(data.direction, false, true);
            player.SetControl(true);
        }

        private void OnEnable() => refreshSubscription = EventCenter.Instance.SubscribeEvent(E_EventType.RequestUI, PublishMode);
        private void OnDisable() => refreshSubscription?.Invoke();
        private void Start() => PublishMode();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) { ResetPlayer(); return; }
            if (map.IsMoving)
            {
                player.ClearInteraction();
                map.TickMechanism(player);
                if (!map.IsMoving) { Notice("机关已到位"); ApplyControl(); }
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
            if (data.mode != SpaceMode.Flat && !pendingConfirm) ReadDirection();
            if (data.mode == SpaceMode.World && Input.GetKeyDown(KeyCode.LeftShift)) BeginPreview();
            else if (data.mode == SpaceMode.Preview)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) CancelPreview();
                else if (pendingConfirm)
                {
                    if (!view.IsChanging) Confirm();
                }
                else if (Input.GetKeyUp(KeyCode.LeftShift)) Confirm();
            }
            else if (data.mode == SpaceMode.Flat && Input.GetKeyDown(KeyCode.LeftShift)) TryExpand();
            player.UpdateInteraction(data.mode == SpaceMode.World);
            if (map.IsMoving) return;
            player.Tick(view.Right, view.Forward);
            if (player.Position.y < map.Data.fallHeight) { ResetPlayer(); return; }
            if (!completed && data.mode == SpaceMode.World && map.IsAtGoal(player.Position))
            {
                completed = true;
                Notice("已抵达终点，按 R 重新开始");
            }
        }

        private void LateUpdate()
        {
            bool projected = data.mode == SpaceMode.Preview || data.mode == SpaceMode.Flat ||
                (data.mode == SpaceMode.Changing && nextMode == SpaceMode.Flat);
            view.Tick(projected ? map.ProjectionCenter : player.Position + Vector3.up * 0.7f);
        }

        private void ApplyControl()
        {
            player.SetControl(data.mode != SpaceMode.Preview &&
                data.mode != SpaceMode.Changing && !map.IsMoving);
            PublishMode();
        }

        private void PublishMode()
        {
            string text = data.mode == SpaceMode.Flat || (data.mode == SpaceMode.Changing && nextMode == SpaceMode.Flat)
                ? "WASD 移动  ·  Shift 展开空间  ·  R 返回起点"
                : "WASD 移动  ·  左右方向键转向  ·  上下方向键切换视角  ·  Shift 压缩  ·  F 交互";
            EventCenter.Instance.TriggerEvent(E_EventType.ModeChanged, text);
        }

        private void ReadDirection()
        {
            ViewDirection selected = data.direction;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) { horizontalView = (horizontalView + 3) % 4; selected = (ViewDirection)horizontalView; }
            if (Input.GetKeyDown(KeyCode.RightArrow)) { horizontalView = (horizontalView + 1) % 4; selected = (ViewDirection)horizontalView; }
            if (Input.GetKeyDown(KeyCode.UpArrow)) selected = ViewDirection.Top;
            if (Input.GetKeyDown(KeyCode.DownArrow)) selected = ViewDirection.Bottom;
            if (selected == data.direction) return;
            data.direction = selected;
            if (data.mode == SpaceMode.Preview)
            {
                data.axis = CameraData.GetAxis(selected);
                map.Preview(anchor, data.axis);
            }
            view.ChangeView(selected, data.mode == SpaceMode.Preview);
            if (data.mode == SpaceMode.World) ChangeMode(SpaceMode.World);
        }

        private void BeginPreview()
        {
            if (map.IsMoving || data.mode != SpaceMode.World) return;
            player.RefreshGround();
            if (!player.Data.grounded) { Notice("请站在道路上再压缩"); return; }
            anchor = player.Data.road;
            savedPosition = player.Position;
            data.axis = CameraData.GetAxis(data.direction);
            player.SetControl(false);
            player.ClearInteraction();
            map.Preview(anchor, data.axis);
            view.ChangeView(data.direction, true);
            data.mode = SpaceMode.Preview;
            Notice("松开 Shift 确认压缩，Esc 取消");
        }

        private void Confirm()
        {
            if (data.mode != SpaceMode.Preview) return;
            if (view.IsChanging) { pendingConfirm = true; return; }
            pendingConfirm = false;
            map.Compress();
            player.SetFlat(data.axis, map.Plane, MapData.Project(savedPosition, data.axis, map.Plane));
            view.ChangeView(data.direction, true);
            ChangeMode(SpaceMode.Flat);
        }

        private void CancelPreview()
        {
            pendingConfirm = false;
            map.CancelPreview();
            view.ChangeView(data.direction, false);
            ChangeMode(SpaceMode.World);
        }

        /// <summary>
        /// 检查来源与恢复空间 成功后展开并恢复行走
        /// </summary>
        private void TryExpand()
        {
            if (data.mode != SpaceMode.Flat) return;
            player.RefreshGround();
            if (!player.Data.grounded || !map.TryRestore(player.Position, player.Data.road,
                player.Data.radius, player.Data.height, out Vector3 target))
            {
                Notice("当前位置无法展开，请移至道路安全区域");
                return;
            }
            map.Expand();
            player.SetWorld(target);
            view.ChangeView(data.direction, false);
            ChangeMode(SpaceMode.World);
            Notice("空间已展开");
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
            pendingConfirm = false;
            completed = false;
            player.ClearInteraction();
            map.Expand();
            map.ResetMechanisms();
            player.SetWorld(map.Data.spawn);
            view.ChangeView(data.direction, false, true);
            data.mode = SpaceMode.World;
            ApplyControl();
            Notice("已返回起点");
        }

        private void Notice(string text) => EventCenter.Instance.TriggerEvent(E_EventType.Notice, text);
    }
}
