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

        [HideInInspector] public ECSPlayerInputCurrent InputCurrentData;
        [HideInInspector] public bool HasNewData;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instances.Clear();
        }

        private void OnEnable()
        {
            Instances.Add(this);
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        private void Start()
        {
            var unityPlayerInput = GetComponent<PlayerInput>();
            InputCurrentData = new ECSPlayerInputCurrent
            {
                Value = new PlayerInputData()
                {
                    ID = (byte)unityPlayerInput.playerIndex
                }
            };
            HasNewData = true;
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
                InputCurrentData.Value.SetButton(ECSPlayerButton.Attack, true);
                HasNewData = true;
            }
        }

        [UsedImplicitly]
        public void OnInteract(InputValue value)
        {
            if (value.isPressed)
            {
                InputCurrentData.Value.SetButton(ECSPlayerButton.Interact, true);
                HasNewData = true;
            }
        }

        [UsedImplicitly]
        public void OnCrouch(InputValue value)
        {
            InputCurrentData.Value.SetButton(ECSPlayerButton.Crouch, value.isPressed);
            HasNewData = true;
        }

        [UsedImplicitly]
        public void OnJump(InputValue value)
        {
            if (value.isPressed)
            {
                InputCurrentData.Value.SetButton(ECSPlayerButton.Jump, true);
                HasNewData = true;
            }
        }

        [UsedImplicitly]
        public void OnPrevious(InputValue value)
        {
            if (value.isPressed)
            {
                InputCurrentData.Value.SetButton(ECSPlayerButton.Previous, true);
                HasNewData = true;
            }
        }

        [UsedImplicitly]
        public void OnNext(InputValue value)
        {
            if (value.isPressed)
            {
                InputCurrentData.Value.SetButton(ECSPlayerButton.Next, true);
                HasNewData = true;
            }
        }

        [UsedImplicitly]
        public void OnSprint(InputValue value)
        {
            InputCurrentData.Value.SetButton(ECSPlayerButton.Sprint, value.isPressed);
            HasNewData = true;
        }
    }
}