using BovineLabs.Core.Jobs;
using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Tracks.Systems
{[UpdateInGroup(typeof(TimelineComponentAnimationGroup))][UpdateBefore(typeof(AnimationProcessSystem))]
    public partial struct TimelineAnimationBlendTree2DTrackSystem : ISystem
    {
        private TrackBlendImpl<float2, BlendTree2DDirectionClipData> _blendImpl;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlobDatabaseSingleton>();
            _blendImpl.OnCreate(ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _blendImpl.OnDestroy(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var blobDB = SystemAPI.GetSingleton<BlobDatabaseSingleton>();

            var blendData = _blendImpl.Update(ref state);

            var targetToTrackMap = new NativeParallelHashMap<Entity, Entity>(64, Allocator.TempJob);
            var targetToTimeMap = new NativeParallelHashMap<Entity, float>(64, Allocator.TempJob);

            state.Dependency = new GatherTrackInfoJob
            {
                TargetToTrack = targetToTrackMap.AsParallelWriter(),
                TargetToTime = targetToTimeMap.AsParallelWriter()
            }.ScheduleParallel(state.Dependency);

            var animationBufferLookup = SystemAPI.GetBufferLookup<AnimationToProcessComponent>(false);
            var trackDataLookup = SystemAPI.GetComponentLookup<BlendAnimationTree2DTrackData>(true);
            var motionBufferLookup = SystemAPI.GetBufferLookup<BlendTree2DMotionData>(true);
            
            // Grab our newly baked playback state
            var playbackStateLookup = SystemAPI.GetComponentLookup<BlendTreePlaybackState>(false);

            state.Dependency = new ApplyBlendedBlendTreeJob
            {
                BlendData = blendData,
                TargetToTrack = targetToTrackMap.AsReadOnly(),
                TargetToTime = targetToTimeMap.AsReadOnly(),
                AnimClipBlobHashMap = blobDB.animations,
                AnimationToProcessComponentLookup = animationBufferLookup,
                BlendTrack2DDataComponentLookup = trackDataLookup,
                BlendTree2DMotionDataBufferLookup = motionBufferLookup,
                PlaybackStateLookup = playbackStateLookup,
                GlobalDeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(blendData, 64, state.Dependency);

            targetToTrackMap.Dispose(state.Dependency);
            targetToTimeMap.Dispose(state.Dependency);
        }

        [BurstCompile][WithAll(typeof(ClipActive))]
        private partial struct GatherTrackInfoJob : IJobEntity
        {
            public NativeParallelHashMap<Entity, Entity>.ParallelWriter TargetToTrack;
            public NativeParallelHashMap<Entity, float>.ParallelWriter TargetToTime;

            public void Execute(in TrackBinding binding, in Clip clip, in LocalTime localTime)
            {
                TargetToTrack.TryAdd(binding.Value, clip.Track);
                TargetToTime.TryAdd(binding.Value, (float)localTime.Value);
            }
        }

        [BurstCompile]
        private struct ApplyBlendedBlendTreeJob : IJobParallelHashMapDefer
        {
            [ReadOnly] public NativeParallelHashMap<Entity, MixData<float2>>.ReadOnly BlendData;
            [ReadOnly] public NativeParallelHashMap<Entity, Entity>.ReadOnly TargetToTrack;
            [ReadOnly] public NativeParallelHashMap<Entity, float>.ReadOnly TargetToTime;
            [ReadOnly] public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> AnimClipBlobHashMap;
            [ReadOnly] public ComponentLookup<BlendAnimationTree2DTrackData> BlendTrack2DDataComponentLookup;
            [ReadOnly] public BufferLookup<BlendTree2DMotionData> BlendTree2DMotionDataBufferLookup;

            [NativeDisableParallelForRestriction] public BufferLookup<AnimationToProcessComponent> AnimationToProcessComponentLookup;
            
            // Notice we use the trackEntity now, which maps perfectly to the specific timeline track!
            [NativeDisableParallelForRestriction] public ComponentLookup<BlendTreePlaybackState> PlaybackStateLookup;
            
            public float GlobalDeltaTime;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                this.Read(BlendData, entryIndex, out var targetEntity, out var mixData);

                float2 blendedDirection = JobHelpers.Blend<float2, Float2Mixer>(ref mixData, float2.zero);
                float totalTimelineWeight = math.min(1f, mixData.Weights.x + mixData.Weights.y + mixData.Weights.z + mixData.Weights.w);
                
                if (totalTimelineWeight <= 0f || !TargetToTrack.TryGetValue(targetEntity, out var trackEntity) || 
                    !TargetToTime.TryGetValue(targetEntity, out var absoluteTime) || 
                    !AnimationToProcessComponentLookup.TryGetBuffer(targetEntity, out var animationToProcess) ||
                    !BlendTree2DMotionDataBufferLookup.TryGetBuffer(trackEntity, out var motions) ||
                    !BlendTrack2DDataComponentLookup.TryGetComponent(trackEntity, out var trackData)) return;

                animationToProcess.Clear();
                int startIndex = animationToProcess.Length;

                var blendTreeClips = new NativeArray<BlobAssetReference<AnimationClipBlob>>(motions.Length, Allocator.Temp);
                var blendTreePositions = new NativeArray<ScriptedAnimator.BlendTree2DMotionElement>(motions.Length, Allocator.Temp);

                for (int i = 0; i < motions.Length; i++)
                {
                    var motionData = motions[i];
                    if (AnimClipBlobHashMap.TryGetValue(motionData.AnimationHash, out var clipBlob))
                        blendTreeClips[i] = clipBlob;
                    else
                        blendTreeClips[i] = BlobAssetReference<AnimationClipBlob>.Null;

                    blendTreePositions[i] = motionData.BlendTree2DMotionElement;
                }

                // --- 1. DUMMY EVALUATION FOR DURATIONS ---
                ScriptedAnimator.PlayBlendTree2D(
                    ref animationToProcess, blendTreeClips, blendTreePositions, blendedDirection, 
                    0f, trackData.BlendTreeType, 1f, default
                );

                float weightedDuration = 0f;
                float totalBlendWeight = 0f;

                for (int i = startIndex; i < animationToProcess.Length; i++)
                {
                    var anim = animationToProcess[i];
                    if (anim.animation.IsCreated)
                    {
                        weightedDuration += anim.animation.Value.length * anim.weight;
                        totalBlendWeight += anim.weight;
                    }
                }

                if (totalBlendWeight > 0f) weightedDuration /= totalBlendWeight;
                if (weightedDuration <= 0.001f) weightedDuration = 1f;

                // --- 2. STATEFUL NORMALIZED TIME FROM TRACK ENTITY ---
                float normalizedTime = 0f;

                if (PlaybackStateLookup.TryGetComponent(trackEntity, out var playbackState))
                {
                    if (!playbackState.IsInitialized)
                    {
                        // First frame initialization
                        float initialTime = absoluteTime / weightedDuration;
                        playbackState.AccumulatedTime = initialTime;
                        playbackState.PreviousAbsoluteTime = absoluteTime;
                        playbackState.IsInitialized = true;
                        
                        normalizedTime = math.frac(initialTime);
                    }
                    else
                    {
                        float delta = absoluteTime - playbackState.PreviousAbsoluteTime;

                        // If delta is huge, user scrubbed the timeline or it looped. 
                        if (math.abs(delta) > 1.0f) delta = GlobalDeltaTime;

                        playbackState.AccumulatedTime += (delta / weightedDuration);
                        playbackState.PreviousAbsoluteTime = absoluteTime;

                        normalizedTime = math.frac(playbackState.AccumulatedTime);
                    }
                    
                    // Save back to the track entity
                    PlaybackStateLookup[trackEntity] = playbackState; 
                }

                // --- 3. APPLY CORRECT TIME & OVERALL TRACK WEIGHT ---
                for (int i = startIndex; i < animationToProcess.Length; i++)
                {
                    var anim = animationToProcess[i];
                    anim.time = normalizedTime;
                    anim.layerWeight = totalTimelineWeight; 
                    animationToProcess[i] = anim;
                }

                blendTreeClips.Dispose();
                blendTreePositions.Dispose();
            }
        }
    }
}