using BovineLabs.Core.Jobs;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace BovineLabs.Timeline.Tracks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct PhysicsVelocityTrackSystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>();
            new PhysicsVelocityTrackSystemJob
            {
                PhysicsVelocityLookup = velocityLookup
            }.ScheduleParallel();

        }

        [BurstCompile]
        [WithAll(typeof(ClipActive), typeof(TimelineActive))]
        
        public partial struct PhysicsVelocityTrackSystemJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public ComponentLookup<PhysicsVelocity> PhysicsVelocityLookup; 
            public void Execute(in PhysicsVelocityClip physicsVelocityClip, in TrackBinding trackBinding)
            {
                var velocity = PhysicsVelocityLookup.GetRefRW(trackBinding.Value);
                velocity.ValueRW.Linear += physicsVelocityClip.Value.Linear;
                velocity.ValueRW.Angular += physicsVelocityClip.Value.Angular;
            }
        }
        
    }
}