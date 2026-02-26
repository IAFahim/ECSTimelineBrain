using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace PlayerInputs.PlayerInputs.Data
{
    public partial struct ECSPlayerInputActiveThisFrame : IComponentData, IEnableableComponent {}
    public partial struct ECSPlayerInputActivePreviousFrame : IComponentData, IEnableableComponent {}
    
    // --- Enableable Action Components ---
    public partial struct InputAttack : IComponentData, IEnableableComponent {}
    public partial struct InputAttackPrevious : IComponentData, IEnableableComponent {}

    public partial struct InputInteract : IComponentData, IEnableableComponent {}
    public partial struct InputInteractPrevious : IComponentData, IEnableableComponent {}

    public partial struct InputCrouch : IComponentData, IEnableableComponent {}
    public partial struct InputCrouchPrevious : IComponentData, IEnableableComponent {}

    public partial struct InputJump : IComponentData, IEnableableComponent {}
    public partial struct InputJumpPrevious : IComponentData, IEnableableComponent {}

    public partial struct InputPrevious : IComponentData, IEnableableComponent {}
    public partial struct InputPreviousPrevious : IComponentData, IEnableableComponent {}

    public partial struct InputNext : IComponentData, IEnableableComponent {}
    public partial struct InputNextPrevious : IComponentData, IEnableableComponent {}

    public partial struct InputSprint : IComponentData, IEnableableComponent {}
    public partial struct InputSprintPrevious : IComponentData, IEnableableComponent {}

    // --- State Data ---
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

            return fs;
        }
    }
}