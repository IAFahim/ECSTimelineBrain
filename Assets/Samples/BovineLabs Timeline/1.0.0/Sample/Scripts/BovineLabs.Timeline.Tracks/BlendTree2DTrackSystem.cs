using BovineLabs.Core.Extensions;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Tracks.Systems
{[UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    [UpdateBefore(typeof(AnimationProcessSystem))]
    public partial struct BlendTree2DTrackSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlobDatabaseSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var blobDB = SystemAPI.GetSingleton<BlobDatabaseSingleton>();
            var animationBufferLookup = SystemAPI.GetBufferLookup<AnimationToProcessComponent>(false);
            var trackDataLookup = SystemAPI.GetComponentLookup<BlendTree2DTrackData>(true);
            var motionBufferLookup = SystemAPI.GetBufferLookup<BlendTree2DMotionData>(true);
            var clipWeightLookup = SystemAPI.GetComponentLookup<ClipWeight>(true);

            new ProcessBlendClipsJob()
            {
                AnimDB = blobDB.animations,
                AnimationBufferLookup = animationBufferLookup,
                TrackDataLookup = trackDataLookup,
                MotionBufferLookup = motionBufferLookup,
                ClipWeightLookup = clipWeightLookup
            }.ScheduleParallel();
        }

        [BurstCompile][WithAll(typeof(ClipActive))]
        private partial struct ProcessBlendClipsJob : IJobEntity
        {[NativeDisableParallelForRestriction] public BufferLookup<AnimationToProcessComponent> AnimationBufferLookup;
            
            [ReadOnly] public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> AnimDB;
            [ReadOnly] public ComponentLookup<BlendTree2DTrackData> TrackDataLookup;[ReadOnly] public BufferLookup<BlendTree2DMotionData> MotionBufferLookup;
            [ReadOnly] public ComponentLookup<ClipWeight> ClipWeightLookup;

            public void Execute(Entity clipEntity, in TrackBinding binding, in Clip clip, in BlendTree2DClipData clipData, in LocalTime localTime)
            {
                var currentTarget = binding.Value;
                var currentTrack = clip.Track;
                
                if (!AnimationBufferLookup.TryGetBuffer(currentTarget, out var animBuffer)) return;
                if (!MotionBufferLookup.TryGetBuffer(currentTrack, out var motions)) return;
                if (!TrackDataLookup.TryGetComponent(currentTrack, out var trackData)) return;

                float weight = ClipWeightLookup.HasComponent(clipEntity) ? ClipWeightLookup[clipEntity].Value : 1f;
                if (weight <= 0f) return;

                var blendTreeClips = new NativeArray<BlobAssetReference<AnimationClipBlob>>(motions.Length, Allocator.Temp);
                var blendTreePositions = new NativeArray<ScriptedAnimator.BlendTree2DMotionElement>(motions.Length, Allocator.Temp);
                
                float refLength = 1f;
                bool refLooped = true;
                bool gotRef = false;

                for (int i = 0; i < motions.Length; i++)
                {
                    var motionData = motions[i];
                    if (AnimDB.TryGetValue(motionData.AnimationHash, out var clipBlob))
                    {
                        blendTreeClips[i] = clipBlob;
                        if (!gotRef && clipBlob.IsCreated)
                        {
                            refLength = clipBlob.Value.length > 0 ? clipBlob.Value.length : 1f;
                            refLooped = clipBlob.Value.looped;
                            gotRef = true;
                        }
                    }
                    else
                    {
                        blendTreeClips[i] = BlobAssetReference<AnimationClipBlob>.Null;
                    }

                    blendTreePositions[i] = motionData.BlendTree2DMotionElement;
                }

                float absoluteTime = (float)localTime.Value;
                float normalizedTime = refLooped ? math.frac(absoluteTime / refLength) : math.saturate(absoluteTime / refLength);


                ScriptedAnimator.PlayBlendTree2D(
                    ref animBuffer,
                    blendTreeClips,
                    blendTreePositions,
                    clipData.Value, normalizedTime,
                    trackData.BlendTreeType,
                    weight,
                    default
                );

                blendTreeClips.Dispose();
                blendTreePositions.Dispose();
            }
        }
    }
}