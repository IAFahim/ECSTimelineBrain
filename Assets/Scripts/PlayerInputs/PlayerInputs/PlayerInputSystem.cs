using System.Collections.Generic;
using PlayerInputs.PlayerInputs.Data;
using Unity.Entities;

namespace PlayerInputs.PlayerInputs
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class PlayerInputSystem : SystemBase
    {
        private Dictionary<PlayerInputBridge, Entity> _bridgeToEntity;
        private List<PlayerInputBridge> _deadBridges;

        protected override void OnCreate()
        {
            _bridgeToEntity = new Dictionary<PlayerInputBridge, Entity>();
            _deadBridges = new List<PlayerInputBridge>();
        }

        protected override void OnUpdate()
        {
            foreach (var bridge in PlayerInputBridge.Instances)
            {
                if (!_bridgeToEntity.TryGetValue(bridge, out var entity))
                {
                    entity = EntityManager.CreateEntity(
                        typeof(ECSPlayerInputCurrent), 
                        typeof(ECSPlayerInputPrevious),
                        typeof(ECSPlayerInputActiveThisFrame),
                        typeof(ECSPlayerInputActivePreviousFrame), typeof(InputAttack), typeof(InputAttackPrevious),
                        typeof(InputInteract), typeof(InputInteractPrevious),
                        typeof(InputCrouch), typeof(InputCrouchPrevious),
                        typeof(InputJump), typeof(InputJumpPrevious),
                        typeof(InputPrevious), typeof(InputPreviousPrevious),
                        typeof(InputNext), typeof(InputNextPrevious),
                        typeof(InputSprint), typeof(InputSprintPrevious)
                    );
                    
                    _bridgeToEntity[bridge] = entity;

                    EntityManager.SetComponentData(entity, bridge.InputCurrentData);
                    EntityManager.SetComponentData(entity, new ECSPlayerInputPrevious { Value = bridge.InputCurrentData.Value });
                    
                    EntityManager.SetComponentEnabled<ECSPlayerInputActiveThisFrame>(entity, false);
                    EntityManager.SetComponentEnabled<ECSPlayerInputActivePreviousFrame>(entity, false);

                    EntityManager.SetComponentEnabled<InputAttack>(entity, false);
                    EntityManager.SetComponentEnabled<InputAttackPrevious>(entity, false);
                    EntityManager.SetComponentEnabled<InputInteract>(entity, false);
                    EntityManager.SetComponentEnabled<InputInteractPrevious>(entity, false);
                    EntityManager.SetComponentEnabled<InputCrouch>(entity, false);
                    EntityManager.SetComponentEnabled<InputCrouchPrevious>(entity, false);
                    EntityManager.SetComponentEnabled<InputJump>(entity, false);
                    EntityManager.SetComponentEnabled<InputJumpPrevious>(entity, false);
                    EntityManager.SetComponentEnabled<InputPrevious>(entity, false);
                    EntityManager.SetComponentEnabled<InputPreviousPrevious>(entity, false);
                    EntityManager.SetComponentEnabled<InputNext>(entity, false);
                    EntityManager.SetComponentEnabled<InputNextPrevious>(entity, false);
                    EntityManager.SetComponentEnabled<InputSprint>(entity, false);
                    EntityManager.SetComponentEnabled<InputSprintPrevious>(entity, false);
                }

                if (bridge.HasNewData)
                {
                    EntityManager.SetComponentData(entity, bridge.InputCurrentData);
                    EntityManager.SetComponentEnabled<ECSPlayerInputActiveThisFrame>(entity, true);

                    EntityManager.SetComponentEnabled<InputAttack>(entity, bridge.AttackPressed);
                    EntityManager.SetComponentEnabled<InputInteract>(entity, bridge.InteractPressed);
                    EntityManager.SetComponentEnabled<InputCrouch>(entity, bridge.CrouchHeld);
                    EntityManager.SetComponentEnabled<InputJump>(entity, bridge.JumpPressed);
                    EntityManager.SetComponentEnabled<InputPrevious>(entity, bridge.PreviousPressed);
                    EntityManager.SetComponentEnabled<InputNext>(entity, bridge.NextPressed);
                    EntityManager.SetComponentEnabled<InputSprint>(entity, bridge.SprintHeld);

                    bridge.AttackPressed = false;
                    bridge.InteractPressed = false;
                    bridge.JumpPressed = false;
                    bridge.PreviousPressed = false;
                    bridge.NextPressed = false;

                    bridge.HasNewData = false;
                }
            }

            _deadBridges.Clear();
            foreach (var bridge in _bridgeToEntity.Keys)
            {
                if (!PlayerInputBridge.Instances.Contains(bridge))
                {
                    _deadBridges.Add(bridge);
                }
            }

            foreach (var bridge in _deadBridges)
            {
                var entity = _bridgeToEntity[bridge];
                if (SystemAPI.Exists(entity))
                {
                    EntityManager.DestroyEntity(entity);
                }
                _bridgeToEntity.Remove(bridge);
            }
        }
    }
}