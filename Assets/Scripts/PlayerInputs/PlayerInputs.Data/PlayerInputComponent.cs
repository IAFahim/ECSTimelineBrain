using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PlayerInputs.PlayerInputs.Data
{
    public struct ECSPlayerInputActiveThisFrame : IComponentData, IEnableableComponent
    {
    }

    public struct ECSPlayerInputActivePreviousFrame : IComponentData, IEnableableComponent
    {
    }

    // --- Enableable Action Components ---
    public struct InputAttack : IComponentData, IEnableableComponent
    {
    }

    public struct InputAttackPrevious : IComponentData, IEnableableComponent
    {
    }

    public struct InputInteract : IComponentData, IEnableableComponent
    {
    }

    public struct InputInteractPrevious : IComponentData, IEnableableComponent
    {
    }

    public struct InputCrouch : IComponentData, IEnableableComponent
    {
    }

    public struct InputCrouchPrevious : IComponentData, IEnableableComponent
    {
    }

    public struct InputJump : IComponentData, IEnableableComponent
    {
    }

    public struct InputJumpPrevious : IComponentData, IEnableableComponent
    {
    }

    public struct InputPrevious : IComponentData, IEnableableComponent
    {
    }

    public struct InputPreviousPrevious : IComponentData, IEnableableComponent
    {
    }

    public struct InputNext : IComponentData, IEnableableComponent
    {
    }

    public struct InputNextPrevious : IComponentData, IEnableableComponent
    {
    }

    public struct InputSprint : IComponentData, IEnableableComponent
    {
    }

    public struct InputSprintPrevious : IComponentData, IEnableableComponent
    {
    }

    // --- State Data ---
    [Serializable]
    public struct ECSPlayerInputCurrent : IComponentData
    {
        public PlayerInputData Value;
    }

    [Serializable]
    public struct ECSPlayerInputPrevious : IComponentData
    {
        public PlayerInputData Value;
    }

    public struct ECSPlayerInputID : IComponentData
    {
        public byte ID;
    }

    [Serializable]
    public struct PlayerInputData
    {
        public float2 Move;
        public float2 Look;

        public FixedString128Bytes ToStringFixedString128Bytes()
        {
            var fs = new FixedString128Bytes();

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

            return fs;
        }
    }
}