using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Movements.Movement.Data;
using Movements.Movement.Logic;

namespace Movements.Movement.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct LinearMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            // Job 1: Move AND Rotate
            // Runs on entities that have LinearMovementTag AND Rotation components (implicitly) 
            // and do NOT have WithoutRotationTag.
            new LinearMoveAndRotateJob 
            { 
                DeltaTime = dt 
            }.ScheduleParallel();

            // Job 2: Move ONLY
            // Runs on entities that have LinearMovementTag AND WithoutRotationTag.
            new LinearMoveOnlyJob 
            { 
                DeltaTime = dt 
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(LinearMovementTag))]
    [WithNone(typeof(WithoutRotationTag), typeof(TargetEcsLocalTransformTag))]
    public partial struct LinearMoveAndRotateJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(
            ref LocalTransform transform,
            ref NormalizedProgress progress,
            RefRO<StartPositionComponent> start,
            RefRO<EndPositionComponent> end,
            RefRO<StartQuaternionComponent> startRot,
            RefRO<EndQuaternionComponent> endRot,
            RefRO<SpeedComponent> speed)
        {
            // 1. Logic: Position & Time
            LinearLogic.Solve(
                start.ValueRO.value,
                end.ValueRO.value,
                progress.value,
                speed.ValueRO.value,
                DeltaTime,
                out float newT,
                out float3 newPos
            );

            // 2. Logic: Rotation
            LinearLogic.SolveRotation(
                startRot.ValueRO.value,
                endRot.ValueRO.value,
                newT,
                out quaternion newRot
            );

            // 3. Apply
            progress.value = newT;
            transform.Position = newPos;
            transform.Rotation = newRot;
        }
    }

    [BurstCompile]
    [WithAll(typeof(LinearMovementTag), typeof(WithoutRotationTag))]
    [WithNone(typeof(TargetEcsLocalTransformTag))]
    public partial struct LinearMoveOnlyJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(
            ref LocalTransform transform,
            ref NormalizedProgress progress,
            RefRO<StartPositionComponent> start,
            RefRO<EndPositionComponent> end,
            RefRO<SpeedComponent> speed)
        {
            // 1. Logic: Position & Time
            LinearLogic.Solve(
                start.ValueRO.value,
                end.ValueRO.value,
                progress.value,
                speed.ValueRO.value,
                DeltaTime,
                out float newT,
                out float3 newPos
            );

            // 2. Apply (No Rotation)
            progress.value = newT;
            transform.Position = newPos;
        }
    }
}