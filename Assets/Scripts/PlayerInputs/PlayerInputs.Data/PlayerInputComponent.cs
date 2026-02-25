using BovineLabs.Core;
using Unity.Collections;

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
        Sprint = 1 << 6,
        All = Attack | Interact | Crouch | Jump | Previous | Next | Sprint
    }

    public static class ECSPlayerButtonExt
    {
        public static void ClearMarked(this ref ECSPlayerButton ecsPlayerButton) => ecsPlayerButton &= ~MakeForClear();
        public static ECSPlayerButton MakeForClear() => ECSPlayerButton.All;
    }

    public partial struct ECSPlayerInputActiveThisFrame: IComponentData, IEnableableComponent{}
    public partial struct ECSPlayerInputActivePreviousFrame: IComponentData, IEnableableComponent{}
    
    public partial struct ECSPlayerInputCurrent : IComponentData
    {
        public PlayerInputData Value;
    }
    
    public partial struct ECSPlayerInputPrevious : IComponentData
    {
        public PlayerInputData Value;
    }
    
    public partial struct PlayerInputData
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
        
        public FixedString128Bytes ToStringFixedString128Bytes()
        {
            var fs = new FixedString128Bytes();

            fs.Append((FixedString32Bytes)"ID:");
            fs.Append(ID);

            if (math.any(Move != float2.zero))
            {
                fs.Append((FixedString32Bytes)" M:(");
                fs.Append(Move.x);
                fs.Append((FixedString32Bytes)",");
                fs.Append(Move.y);
                fs.Append((FixedString32Bytes)")");
            }

            if (math.any(Look != float2.zero))
            {
                fs.Append((FixedString32Bytes)" L:(");
                fs.Append(Look.x);
                fs.Append((FixedString32Bytes)",");
                fs.Append(Look.y);
                fs.Append((FixedString32Bytes)")");
            }

            if (Buttons != ECSPlayerButton.None)
            {
                fs.Append((FixedString32Bytes)" [");

                if (IsAttacking)   fs.Append((FixedString32Bytes)"A");
                if (IsInteracting) fs.Append((FixedString32Bytes)"I");
                if (IsCrouching)   fs.Append((FixedString32Bytes)"C");
                if (IsJumping)     fs.Append((FixedString32Bytes)"J");
                if (IsSprinting)   fs.Append((FixedString32Bytes)"S");
                if (IsPrevious)    fs.Append((FixedString32Bytes)"<");
                if (IsNext)        fs.Append((FixedString32Bytes)">");

                fs.Append((FixedString32Bytes)"]");
            }

            return fs;
        }
    }
}