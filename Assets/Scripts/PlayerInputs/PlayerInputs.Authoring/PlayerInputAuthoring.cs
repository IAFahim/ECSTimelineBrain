using PlayerInputs.PlayerInputs.Data;
using Unity.Entities;
using UnityEngine;

namespace PlayerInputs.PlayerInputs.Authoring
{
    public class PlayerInputAuthoring : MonoBehaviour
    {
        public PlayerInputData data;
        public PlayerInputBaker.InputInitialState initialState;

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

                Setup<InputAttack, InputAttackPrevious>(entity, authoring.initialState.attack);
                Setup<InputInteract, InputInteractPrevious>(entity, authoring.initialState.interact);
                Setup<InputCrouch, InputCrouchPrevious>(entity, authoring.initialState.crouch);
                Setup<InputJump, InputJumpPrevious>(entity, authoring.initialState.jump);
                Setup<InputPrevious, InputPreviousPrevious>(entity, authoring.initialState.previous);
                Setup<InputNext, InputNextPrevious>(entity, authoring.initialState.next);
                Setup<InputSprint, InputSprintPrevious>(entity, authoring.initialState.sprint);
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
            
            [System.Serializable]
            public struct InputInitialState
            {
                public bool attack;
                public bool interact;
                public bool crouch;
                public bool jump;
                public bool previous;
                public bool next;
                public bool sprint;
            }
        }
    }
}