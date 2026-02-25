using BovineLabs.Core;
using PlayerInputs.PlayerInputs.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PlayerInputs.PlayerInputs
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [BurstCompile]
    public partial struct ClearPlayerInputTriggersSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ECSPlayerInputCurrent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new ClearTriggersJob(){
            }.ScheduleParallel();
        }

        [BurstCompile]
        [WithPresent(typeof(ECSPlayerInputActiveThisFrame), typeof(ECSPlayerInputActivePreviousFrame))]
        public partial struct ClearTriggersJob : IJobEntity
        {
            private void Execute(
                ref ECSPlayerInputCurrent current,
                ref ECSPlayerInputPrevious previous,
                EnabledRefRW<ECSPlayerInputActiveThisFrame> active,
                EnabledRefRW<ECSPlayerInputActivePreviousFrame> activePrevious)
            {
                previous.Value = current.Value;
                current.Value.Buttons.ClearMarked();
                activePrevious.ValueRW = active.ValueRO;
                active.ValueRW = false;
            }
        }
    }
}