using BovineLabs.Core;
using PlayerInputs.PlayerInputs.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace PlayerInputs.PlayerInputs
{[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
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
            state.Dependency = new ClearDataJob().ScheduleParallel(state.Dependency);
            state.Dependency = new ClearCombatJob().ScheduleParallel(state.Dependency);
            state.Dependency = new ClearMovementJob().ScheduleParallel(state.Dependency);
            state.Dependency = new ClearMiscJob().ScheduleParallel(state.Dependency);
        }

        [BurstCompile][WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
        public partial struct ClearDataJob : IJobEntity
        {
            private void Execute(
                in ECSPlayerInputCurrent current,
                ref ECSPlayerInputPrevious previous,
                EnabledRefRW<ECSPlayerInputActiveThisFrame> active,
                EnabledRefRW<ECSPlayerInputActivePreviousFrame> activePrevious)
            {
                previous.Value = current.Value;

                activePrevious.ValueRW = active.ValueRO;
                active.ValueRW = false;
            }
        }

        [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)][WithAll(typeof(ECSPlayerInputCurrent))]
        public partial struct ClearCombatJob : IJobEntity
        {
            private void Execute(
                EnabledRefRW<InputAttack> attack,
                EnabledRefRW<InputAttackPrevious> attackPrevious,
                EnabledRefRW<InputInteract> interact,
                EnabledRefRW<InputInteractPrevious> interactPrevious)
            {
                attackPrevious.ValueRW = attack.ValueRO;
                attack.ValueRW = false;

                interactPrevious.ValueRW = interact.ValueRO;
                interact.ValueRW = false;
            }
        }

        [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)][WithAll(typeof(ECSPlayerInputCurrent))]
        public partial struct ClearMovementJob : IJobEntity
        {
            private void Execute(
                EnabledRefRW<InputJump> jump,
                EnabledRefRW<InputJumpPrevious> jumpPrevious,
                EnabledRefRW<InputCrouch> crouch,
                EnabledRefRW<InputCrouchPrevious> crouchPrevious,
                EnabledRefRW<InputSprint> sprint,
                EnabledRefRW<InputSprintPrevious> sprintPrevious)
            {
                jumpPrevious.ValueRW = jump.ValueRO;
                jump.ValueRW = false;

                crouchPrevious.ValueRW = crouch.ValueRO;

                sprintPrevious.ValueRW = sprint.ValueRO;
            }
        }

        [BurstCompile][WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
        [WithAll(typeof(ECSPlayerInputCurrent))]
        public partial struct ClearMiscJob : IJobEntity
        {
            private void Execute(
                EnabledRefRW<InputPrevious> inputPrev,
                EnabledRefRW<InputPreviousPrevious> inputPrevPrev,
                EnabledRefRW<InputNext> next,
                EnabledRefRW<InputNextPrevious> nextPrevious)
            {
                inputPrevPrev.ValueRW = inputPrev.ValueRO;
                inputPrev.ValueRW = false;

                nextPrevious.ValueRW = next.ValueRO;
                next.ValueRW = false;
            }
        }
    }
}