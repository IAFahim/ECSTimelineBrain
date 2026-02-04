using Movements.Movement.Data.Advanced.Targets;
using Movements.Movement.Data.Parameters.Motion;
using Movements.Movement.Data.Tags.MovementTypes;
using Movements.Movement.Data.Transforms.Unassigned;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Movements.Movement.Systems
{
    /// <summary>
    ///     System that handles movement toward target Transforms.
    ///     Architecture:
    ///     1. SyncTargetTransformSystem (non-Burst): Reads enabled TargetTransformComponent and syncs to Updated components
    ///     2. TargetTransformSystem (Burst): Reads Updated components and performs movement calculations using SpeedComponent
    ///     Uses IEnableableComponent on TargetTransformComponent to indicate when the target is instantiated and valid.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(LinearMovementSystem))]
    public partial struct TargetTransformSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LinearMovementTag>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Movement with rotation (Burst) - only processes enabled TargetTransformComponent
            state.Dependency = new MoveToTargetWithRotationJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);

            // Movement without rotation (Burst) - only processes enabled TargetTransformComponent
            state.Dependency = new MoveToTargetJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
        }
    }

    /// <summary>
    ///     Non-Burst system that syncs managed Transform data to ECS components.
    ///     Only processes entities where TargetTransformComponent is ENABLED (IEnableableComponent).
    ///     This reads UnityObjectRef
    ///     <Transform>
    ///         and writes to UpdatedPositionComponent/UpdatedQuaternionComponent.
    ///         IMPORTANT: This system must NOT be Burst-compiled because it accesses UnityObjectRef.Value.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TargetTransformSystem))]
    public partial struct SyncTargetTransformSystem : ISystem
    {
        private EntityQuery _queryWithRotation;
        private EntityQuery _queryWithoutRotation;

        public void OnCreate(ref SystemState state)
        {
            _queryWithRotation = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TargetTransformComponent, UnAssignedPositionComponent, UnAssignedQuaternionComponent>()
                .Build(state.EntityManager);

            _queryWithoutRotation = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TargetTransformComponent, UnAssignedPositionComponent>()
                .WithNone<UnAssignedQuaternionComponent>()
                .Build(state.EntityManager);
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            // Sync entities WITH rotation
            var entities = _queryWithRotation.ToEntityArray(Allocator.Temp);
            var targets = _queryWithRotation.ToComponentDataArray<TargetTransformComponent>(Allocator.Temp);
            var updatedPos = _queryWithRotation.ToComponentDataArray<UnAssignedPositionComponent>(Allocator.Temp);
            var updatedRot = _queryWithRotation.ToComponentDataArray<UnAssignedQuaternionComponent>(Allocator.Temp);

            for (var i = 0; i < entities.Length; i++)
            {
                // Check if TargetTransformComponent is enabled for this entity
                if (!state.EntityManager.IsComponentEnabled<TargetTransformComponent>(entities[i]))
                    continue;

                var transform = targets[i].Value.Value;
                if (transform == null)
                {
                    updatedPos[i] = new UnAssignedPositionComponent
                    {
                        value = new float3(float.MaxValue, float.MaxValue, float.MaxValue)
                    };
                    updatedRot[i] = new UnAssignedQuaternionComponent { value = quaternion.identity };
                    continue;
                }

                updatedPos[i] = new UnAssignedPositionComponent { value = transform.position };
                updatedRot[i] = new UnAssignedQuaternionComponent { value = transform.rotation };
            }

            _queryWithRotation.CopyFromComponentDataArray(updatedPos);
            _queryWithRotation.CopyFromComponentDataArray(updatedRot);

            // Sync entities WITHOUT rotation
            var entities2 = _queryWithoutRotation.ToEntityArray(Allocator.Temp);
            var targets2 = _queryWithoutRotation.ToComponentDataArray<TargetTransformComponent>(Allocator.Temp);
            var updatedPos2 = _queryWithoutRotation.ToComponentDataArray<UnAssignedPositionComponent>(Allocator.Temp);

            for (var i = 0; i < entities2.Length; i++)
            {
                if (!state.EntityManager.IsComponentEnabled<TargetTransformComponent>(entities2[i]))
                    continue;

                var transform = targets2[i].Value.Value;
                if (transform == null)
                {
                    updatedPos2[i] = new UnAssignedPositionComponent
                    {
                        value = new float3(float.MaxValue, float.MaxValue, float.MaxValue)
                    };
                    continue;
                }

                updatedPos2[i] = new UnAssignedPositionComponent { value = transform.position };
            }

            _queryWithoutRotation.CopyFromComponentDataArray(updatedPos2);
        }
    }

    /// <summary>
    ///     Job for entities moving toward a managed Transform WITH rotation.
    ///     Only processes entities where TargetTransformComponent is ENABLED.
    ///     Reads from UpdatedPositionComponent/UpdatedQuaternionComponent that were synced from managed code.
    ///     Uses SpeedComponent for movement speed.
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(LinearMovementTag))]
    public partial struct MoveToTargetWithRotationJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(
            ref LocalTransform transform,
            RefRO<UnAssignedPositionComponent> updatedPos,
            RefRO<UnAssignedQuaternionComponent> updatedRot,
            RefRO<SpeedComponent> speed)
        {
            var targetPos = updatedPos.ValueRO.value;
            var targetRot = updatedRot.ValueRO.value;

            // Check for invalid target (float.MaxValue sentinel value)
            if (targetPos.x >= float.MaxValue || targetPos.y >= float.MaxValue || targetPos.z >= float.MaxValue) return;

            var direction = targetPos - transform.Position;
            var distance = math.length(direction);

            if (distance > 0.001f)
            {
                var moveAmount = math.min(distance, speed.ValueRO.value * DeltaTime);
                transform.Position += math.normalize(direction) * moveAmount;
            }

            transform.Rotation = math.slerp(
                transform.Rotation,
                targetRot,
                speed.ValueRO.value * DeltaTime * 2f
            );
        }
    }

    /// <summary>
    ///     Job for entities moving toward a managed Transform WITHOUT rotation.
    ///     Only processes entities where TargetTransformComponent is ENABLED.
    ///     Reads from UpdatedPositionComponent that was synced from managed code.
    ///     Uses SpeedComponent for movement speed.
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(LinearMovementTag))]
    public partial struct MoveToTargetJob : IJobEntity
    {
        public float DeltaTime;

        private void Execute(
            ref LocalTransform transform,
            RefRO<UnAssignedPositionComponent> updatedPos,
            RefRO<SpeedComponent> speed)
        {
            var targetPos = updatedPos.ValueRO.value;

            // Check for invalid target (float.MaxValue sentinel value)
            if (targetPos.x >= float.MaxValue || targetPos.y >= float.MaxValue || targetPos.z >= float.MaxValue) return;

            var direction = targetPos - transform.Position;
            var distance = math.length(direction);

            if (distance > 0.001f)
            {
                var moveAmount = math.min(distance, speed.ValueRO.value * DeltaTime);
                transform.Position += math.normalize(direction) * moveAmount;
            }
        }
    }
}