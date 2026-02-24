using BovineLabs.Core;

namespace PlayerInputs.PlayerInputs.Data
{
    using System;
    using Unity.Entities;
    using Unity.Mathematics;

    [Flags]
    public enum ECSPlayerButton : byte
    {
        None = 0,
        Attack = 1 << 0,
        Interact = 1 << 1,
        Crouch = 1 << 2,
        Jump = 1 << 3,
        Previous = 1 << 4,
        Next = 1 << 5,
        Sprint = 1 << 6
    }

    public static class ECSPlayerButtonExt
    {
        public static void ClearMarked(this ref ECSPlayerButton ecsPlayerButton) => ecsPlayerButton &= ~MakeForClear();
        public static ECSPlayerButton MakeForClear()
        {
            return
                ECSPlayerButton.Attack |
                ECSPlayerButton.Jump |
                ECSPlayerButton.Interact |
                ECSPlayerButton.Previous |
                ECSPlayerButton.Next;
        }
    }

    public partial struct ECSPlayerInputUpdateThisFrame: IComponentData, IEnableableComponent{}
    
    public partial struct ECSPlayerInput : IComponentData
    {
        public byte ID;
        public float2 Move;
        public float2 Look;

        public ECSPlayerButton Buttons;

        public bool IsAttacking => (Buttons & ECSPlayerButton.Attack) != 0;
        public bool IsInteracting => (Buttons & ECSPlayerButton.Interact) != 0;
        public bool IsCrouching => (Buttons & ECSPlayerButton.Crouch) != 0;
        public bool IsJumping => (Buttons & ECSPlayerButton.Jump) != 0;
        public bool IsPrevious => (Buttons & ECSPlayerButton.Previous) != 0;
        public bool IsNext => (Buttons & ECSPlayerButton.Next) != 0;
        public bool IsSprinting => (Buttons & ECSPlayerButton.Sprint) != 0;
        
        public void SetButton(ECSPlayerButton button, bool isPressed)
        {
            if (isPressed) Buttons |= button;
            else Buttons &= ~button;
        }
    }
}