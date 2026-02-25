using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.IntegerTime;
using Unity.Mathematics;

namespace Rukhanka.Timeline.Systems
{
    /// <summary>
    /// This system evaluates the state of all active Rukhanka Animation Timeline Clips, 
    /// looks up their baked data, and injects them into the target entity's Animation buffer 
    /// for seamless playback and blending.
    /// </summary>
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    [UpdateBefore(typeof(AnimationProcessSystem))]
    public partial struct RukhankaTimelineTrackSystem : ISystem
    {

        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlobDatabaseSingleton>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

            new RukhankaTimelineTrackSystemJob()
            {
                animDB = SystemAPI.GetSingleton<BlobDatabaseSingleton>().animations, ClipData = SystemAPI.GetSingleton<RukhankaAnimationClipData>(), animationTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel();

        }

        [BurstCompile]
        public partial struct RukhankaTimelineTrackSystemJob : IJobEntity
        {
            [ReadOnly]
            public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> animDB;
            public RukhankaAnimationClipData ClipData;
            public float animationTime;

            void Execute(ref DynamicBuffer<AnimationToProcessComponent> atps)
            {
                ScriptedAnimator.ResetAnimationState(ref atps);
                animDB.TryGetValue(ClipData.AnimationHash, out var clip0Blob);
                ScriptedAnimator.PlayAnimation(ref atps, clip0Blob, animationTime);

            }
        }
    }
}