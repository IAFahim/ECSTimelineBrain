using PlayerInputs.PlayerInputs.Data;
using Unity.Entities;
using UnityEngine;

namespace PlayerInputs.PlayerInputs.Authoring
{
    public class PlayerInputAuthoring : MonoBehaviour
    {
        public PlayerInputData data;
        public InputInitialState initialState;

        public class PlayerInputBaker : Baker<PlayerInputAuthoring>
        {
            public override void Bake(PlayerInputAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new ECSPlayerInputCurrent
                {
                    Value = authoring.data
                });

                AddComponent<ECSPlayerInputPrevious>(entity);

                Setup<InputAttack, InputAttackPrevious>(
                    entity, 
                    authoring.initialState.HasFlagFast(InputInitialState.Attack)
                );

                Setup<InputInteract, InputInteractPrevious>(
                    entity, 
                    authoring.initialState.HasFlagFast(InputInitialState.Interact)
                );

                Setup<InputCrouch, InputCrouchPrevious>(
                    entity, 
                    authoring.initialState.HasFlagFast(InputInitialState.Crouch)
                );

                Setup<InputJump, InputJumpPrevious>(
                    entity, 
                    authoring.initialState.HasFlagFast(InputInitialState.Jump)
                );

                Setup<InputPrevious, InputPreviousPrevious>(
                    entity, 
                    authoring.initialState.HasFlagFast(InputInitialState.Previous)
                );

                Setup<InputNext, InputNextPrevious>(
                    entity, 
                    authoring.initialState.HasFlagFast(InputInitialState.Next)
                );

                Setup<InputSprint, InputSprintPrevious>(
                    entity, 
                    authoring.initialState.HasFlagFast(InputInitialState.Sprint)
                );
            }

            private void Setup<TCurrent, TPrevious>(Entity entity, bool isActive)
                where TCurrent : unmanaged, IComponentData, IEnableableComponent
                where TPrevious : unmanaged, IComponentData, IEnableableComponent
            {
                AddComponent<TCurrent>(entity);
                SetComponentEnabled<TCurrent>(entity, isActive);

                AddComponent<TPrevious>(entity);
                SetComponentEnabled<TPrevious>(entity, false);
            }
        }
    }
}