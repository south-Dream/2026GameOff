using UnityEngine;

namespace PlaneShift
{
    /// <summary>
    /// 执行角色方向移动与碰撞位置切换
    /// </summary>
    public class PlayerController
    {
        private CharacterController body;
        private Transform visual;
        private float fallSpeed;

        public Vector3 Position => body.transform.position;
        public Collider Collider => body;

        public void Init(CharacterController character, Transform model, PlayerData data, Vector3 spawn)
        {
            body = character;
            visual = model;
            body.height = data.height;
            body.radius = data.radius;
            body.center = Vector3.up * data.height / 2;
            body.skinWidth = 0.025f;
            body.stepOffset = 0.15f;
            body.minMoveDistance = 0;
            Teleport(spawn);
        }

        public void Move(Vector3 direction, PlayerData data, bool topView, SpaceAxis? axis, float plane)
        {
            if (direction.sqrMagnitude > 1) direction.Normalize();
            if (direction.sqrMagnitude > 0.001f)
                visual.rotation = Quaternion.LookRotation(direction, Vector3.up);
            fallSpeed = topView ? 0 : body.isGrounded ? -2 : fallSpeed - data.gravity * Time.deltaTime;
            body.Move((direction * data.moveSpeed + Vector3.up * fallSpeed) * Time.deltaTime);
            if (axis.HasValue)
            {
                Vector3 position = Position;
                position[(int)axis.Value] = plane;
                if ((position - Position).sqrMagnitude > 0.000001f) Teleport(position);
            }
        }

        /// <summary>
        /// 暂停碰撞后设置脚底位置并清除下落速度
        /// </summary>
        public void Teleport(Vector3 foot)
        {
            body.enabled = false;
            body.transform.position = foot + Vector3.up * 0.025f;
            fallSpeed = 0;
            body.enabled = true;
        }

    }
}
