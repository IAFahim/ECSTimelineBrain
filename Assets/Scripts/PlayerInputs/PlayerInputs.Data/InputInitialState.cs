using System;
using System.Runtime.CompilerServices;

namespace PlayerInputs.PlayerInputs.Data
{
    [Flags]
    public enum InputInitialState : byte
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

    public static class InputInitialStateImpl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagFast(this InputInitialState initialState, InputInitialState flag)
        {
            return (initialState & flag) != 0;
        }
    }
}