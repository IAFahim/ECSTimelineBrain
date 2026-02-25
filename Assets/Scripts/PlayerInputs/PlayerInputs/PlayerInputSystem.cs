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
                        typeof(ECSPlayerInputActivePreviousFrame)
                    );
                    
                    _bridgeToEntity[bridge] = entity;

                    EntityManager.SetComponentData(entity, bridge.InputCurrentData);
                    EntityManager.SetComponentData(entity, new ECSPlayerInputPrevious { Value = bridge.InputCurrentData.Value });
                    
                    EntityManager.SetComponentEnabled<ECSPlayerInputActiveThisFrame>(entity, false);
                    EntityManager.SetComponentEnabled<ECSPlayerInputActivePreviousFrame>(entity, false);
                }

                if (bridge.HasNewData)
                {
                    EntityManager.SetComponentData(entity, bridge.InputCurrentData);
                    EntityManager.SetComponentEnabled<ECSPlayerInputActiveThisFrame>(entity, true);
                    
                    bridge.HasNewData = false;

                    bridge.InputCurrentData.Value.Buttons.ClearMarked();
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