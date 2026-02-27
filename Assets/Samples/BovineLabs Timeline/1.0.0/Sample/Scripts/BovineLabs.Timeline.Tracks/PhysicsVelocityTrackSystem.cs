using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace BovineLabs.Timeline.Tracks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct PhysicsVelocityTrackSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>();
            var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            new PhysicsVelocityTrackSystemJob
            {
                PhysicsVelocityLookup = velocityLookup,
                LocalTransformLookup = transformLookup,
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel();
        }

        [BurstCompile]
        [WithAny(typeof(ClipActive), typeof(ClipActivePrevious))]
        public partial struct PhysicsVelocityTrackSystemJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public ComponentLookup<PhysicsVelocity> PhysicsVelocityLookup;
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            [ReadOnly] public float DeltaTime;

            public void Execute(in PhysicsVelocityComponent physicsVelocityComponent, in TrackBinding trackBinding)
            {
                if (!PhysicsVelocityLookup.HasComponent(trackBinding.Value)) return;

                var velocity = PhysicsVelocityLookup.GetRefRW(trackBinding.Value);

                float3 linear = physicsVelocityComponent.PhysicsVelocity.Linear;
                float3 angular = physicsVelocityComponent.PhysicsVelocity.Angular;

                if (physicsVelocityComponent.IsLocalSpace && LocalTransformLookup.HasComponent(trackBinding.Value))
                {
                    var rot = LocalTransformLookup[trackBinding.Value].Rotation;
                    linear = math.rotate(rot, linear);
                    angular = math.rotate(rot, angular);
                }

                velocity.ValueRW.Linear += linear * DeltaTime;
                velocity.ValueRW.Angular += angular * DeltaTime;
            }
        }
    }
}