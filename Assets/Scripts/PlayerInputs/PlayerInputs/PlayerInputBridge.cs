using System.Collections.Generic;
using JetBrains.Annotations;
using PlayerInputs.PlayerInputs.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerInputs.PlayerInputs
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputBridge : MonoBehaviour
    {
        public static readonly HashSet<PlayerInputBridge> Instances = new();
        public bool HasNewData;

        public bool AttackPressed;
        public bool InteractPressed;
        public bool CrouchHeld;
        public bool JumpPressed;
        public bool PreviousPressed;
        public bool NextPressed;
        public bool SprintHeld;

        public byte id;
        public ECSPlayerInputCurrent InputCurrentData;

        private void Start()
        {
            var unityPlayerInput = GetComponent<PlayerInput>();
            id = (byte)unityPlayerInput.playerIndex;
            InputCurrentData = new ECSPlayerInputCurrent
            {
                Value = new PlayerInputData
                {
                    Move = default,
                    Look = default
                }
            };
            HasNewData = true;
        }

        private void OnEnable()
        {
            Instances.Add(this);
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instances.Clear();
        }

        [UsedImplicitly]
        public void OnMove(InputValue value)
        {
            InputCurrentData.Value.Move = value.Get<Vector2>();
            HasNewData = true;
        }

        [UsedImplicitly]
        public void OnLook(InputValue value)
        {
            InputCurrentData.Value.Look = value.Get<Vector2>();
            HasNewData = true;
        }

        [UsedImplicitly]
        public void OnAttack(InputValue value)
        {
            if (value.isPressed)
            {
                AttackPressed = true;
                HasNewData = true;
            }
        }

        [UsedImplicitly]
        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                InteractPressed = true;
                HasNewData = true;
            }
        }

        [UsedImplicitly]
        public void OnCrouch(InputValue value)
        {
            CrouchHeld = value.isPressed;
            HasNewData = true;
        }

        [UsedImplicitly]
        public void OnJump(InputValue value)
        {
            if (value.isPressed)
            {
                JumpPressed = true;
                HasNewData = true;
            }
        }

        [UsedImplicitly]
        public void OnPrevious(InputValue value)
        {
            if (value.isPressed)
            {
                PreviousPressed = true;
                HasNewData = true;
            }
        }

        [UsedImplicitly]
        public void OnNext(InputValue value)
        {
            if (value.isPressed)
            {
                NextPressed = true;
                HasNewData = true;
            }
        }

        [UsedImplicitly]
        public void OnSprint(InputValue value)
        {
            SprintHeld = value.isPressed;
            HasNewData = true;
        }
    }
}