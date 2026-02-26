using PlayerInputs.PlayerInputs.Data;
using Unity.Entities;
using UnityEngine;

namespace PlayerInputs.PlayerInputs.Authoring
{
    public class PlayerInputAuthoring : MonoBehaviour
    {
        public PlayerInputData data;
        
        public class PlayerInputBaker : Baker<PlayerInputAuthoring>
        {
            public override void Bake(PlayerInputAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new ECSPlayerInputCurrent()
                {
                    Value = authoring.data
                });
                AddComponent(entity, new ECSPlayerInputPrevious());

                AddComponent<InputAttack>(entity);
                SetComponentEnabled<InputAttack>(entity, false);
                AddComponent<InputAttackPrevious>(entity);
                SetComponentEnabled<InputAttackPrevious>(entity, false);

                AddComponent<InputInteract>(entity);
                SetComponentEnabled<InputInteract>(entity, false);
                AddComponent<InputInteractPrevious>(entity);
                SetComponentEnabled<InputInteractPrevious>(entity, false);

                AddComponent<InputCrouch>(entity);
                SetComponentEnabled<InputCrouch>(entity, false);
                AddComponent<InputCrouchPrevious>(entity);
                SetComponentEnabled<InputCrouchPrevious>(entity, false);

                AddComponent<InputJump>(entity);
                SetComponentEnabled<InputJump>(entity, false);
                AddComponent<InputJumpPrevious>(entity);
                SetComponentEnabled<InputJumpPrevious>(entity, false);

                AddComponent<InputPrevious>(entity);
                SetComponentEnabled<InputPrevious>(entity, false);
                AddComponent<InputPreviousPrevious>(entity);
                SetComponentEnabled<InputPreviousPrevious>(entity, false);

                AddComponent<InputNext>(entity);
                SetComponentEnabled<InputNext>(entity, false);
                AddComponent<InputNextPrevious>(entity);
                SetComponentEnabled<InputNextPrevious>(entity, false);

                AddComponent<InputSprint>(entity);
                SetComponentEnabled<InputSprint>(entity, false);
                AddComponent<InputSprintPrevious>(entity);
                SetComponentEnabled<InputSprintPrevious>(entity, false);
            }
        }
    }
}