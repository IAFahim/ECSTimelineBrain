using Movements.Movement.Data.Advanced.Facets;
using Movements.Movement.Data.Advanced.Targets;
using Movements.Movement.Data.Parameters.Motion;
using Movements.Movement.Data.Parameters.Timing;
using Movements.Movement.Data.Tags.MovementTypes;
using Movements.Movement.Data.Tags.Targets;
using Movements.Movement.Data.Transforms.StartEnd;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Movements.Movement.Systems
{
    /// <summary>
    ///     System that processes linear movement using IFacet patterns.
    ///     Two separate jobs handle entities with and without rotation components.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct LinearMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Require at least one entity with linear movement
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LinearMovementTag, LocalTransform, NormalizedProgress>()
                .WithAll<StartPositionComponent, EndPositionComponent, SpeedComponent, RangeComponent>()
                .WithNone<TargetEcsLocalTransformTag>();

            state.RequireForUpdate(state.GetEntityQuery(builder));
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeData = new TimeData { DeltaTime = SystemAPI.Time.DeltaTime };

            // Job 1: Movement WITH rotation
            new LinearMovementWithRotationJob
            {
                Time = timeData
            }.ScheduleParallel();

            // Job 2: Movement WITHOUT rotation
            new LinearMovementJob
            {
                Time = timeData
            }.ScheduleParallel();
        }
    }

    /// <summary>
    ///     Job for entities WITH rotation components.
    ///     Constructs LinearMovementWithRotationFacet.
    ///     Excludes entities with enabled TargetTransformComponent (they use a separate tracking job).
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(LinearMovementTag))]
    [WithNone(typeof(TargetEcsLocalTransformTag), typeof(TargetTransformComponent))]
    public partial struct LinearMovementWithRotationJob : IJobEntity
    {
        public TimeData Time;

        private void Execute(
            Entity entity,
            RefRW<LocalTransform> transform,
            RefRW<NormalizedProgress> progress,
            RefRO<StartPositionComponent> startPos,
            RefRO<EndPositionComponent> endPos,
            RefRO<SpeedComponent> speed,
            RefRO<RangeComponent> range,
            RefRO<StartQuaternionComponent> startRot,
            RefRO<EndQuaternionComponent> endRot)
        {
            var facet = new LinearMovementWithRotationFacet(
                entity,
                transform,
                progress,
                startPos,
                endPos,
                speed,
                range,
                startRot,
                endRot,
                Time
            );

            facet.Execute();
        }
    }

    /// <summary>
    ///     Job for entities WITHOUT rotation components.
    ///     Constructs LinearMovementFacet (position only).
    ///     Excludes entities with enabled TargetTransformComponent (they use a separate tracking job).
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(LinearMovementTag))]
    [WithNone(typeof(TargetEcsLocalTransformTag), typeof(TargetTransformComponent))]
    public partial struct LinearMovementJob : IJobEntity
    {
        public TimeData Time;

        private void Execute(
            Entity entity,
            RefRW<LocalTransform> transform,
            RefRW<NormalizedProgress> progress,
            RefRO<StartPositionComponent> startPos,
            RefRO<EndPositionComponent> endPos,
            RefRO<SpeedComponent> speed,
            RefRO<RangeComponent> range)
        {
            var facet = new LinearMovementFacet(
                entity,
                transform,
                progress,
                startPos,
                endPos,
                speed,
                range,
                Time
            );

            facet.Execute();
        }
    }
}