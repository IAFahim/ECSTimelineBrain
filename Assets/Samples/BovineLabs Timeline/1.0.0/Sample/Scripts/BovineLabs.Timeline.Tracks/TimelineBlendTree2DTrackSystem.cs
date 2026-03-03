using BovineLabs.Core.Jobs;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Authoring
{[UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    [UpdateBefore(typeof(TimelineAnimationUnificationSystem))]
    public partial struct TimelineBlendTree2DTrackSystem : ISystem
    {
        private TrackBlendImpl<float2, BlendTree2DDirectionClipData> _blendImpl;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlobDatabaseSingleton>();
            _blendImpl.OnCreate(ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { _blendImpl.OnDestroy(ref state); }

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

            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            state.Dependency = new DecomposeAndAppendBlendTreeJob
            {
                BlendData = blendData,
                TargetToTrack = targetToTrackMap.AsReadOnly(),
                TargetToTime = targetToTimeMap.AsReadOnly(),
                AnimDB = blobDB.animations,
                TrackDataLookup = SystemAPI.GetComponentLookup<BlendAnimationTree2DTrackData>(true),
                MotionBufferLookup = SystemAPI.GetBufferLookup<BlendTree2DMotionData>(true),
                PlaybackStateLookup = SystemAPI.GetComponentLookup<BlendTreePlaybackState>(false),
                FallbackOverrideLookup = SystemAPI.GetComponentLookup<TrackFallbackOverride>(true),
                GlobalDeltaTime = SystemAPI.Time.DeltaTime,
                ECB = ecb.AsParallelWriter()
            }.ScheduleParallel(blendData, 64, state.Dependency);

            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            targetToTrackMap.Dispose();
            targetToTimeMap.Dispose();
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
        private struct DecomposeAndAppendBlendTreeJob : IJobParallelHashMapDefer
        {[ReadOnly] public NativeParallelHashMap<Entity, MixData<float2>>.ReadOnly BlendData;
            [ReadOnly] public NativeParallelHashMap<Entity, Entity>.ReadOnly TargetToTrack;
            [ReadOnly] public NativeParallelHashMap<Entity, float>.ReadOnly TargetToTime;
            [ReadOnly] public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> AnimDB;
            [ReadOnly] public ComponentLookup<BlendAnimationTree2DTrackData> TrackDataLookup;
            [ReadOnly] public BufferLookup<BlendTree2DMotionData> MotionBufferLookup;
            [ReadOnly] public ComponentLookup<TrackFallbackOverride> FallbackOverrideLookup;[NativeDisableParallelForRestriction] public ComponentLookup<BlendTreePlaybackState> PlaybackStateLookup;
            
            public EntityCommandBuffer.ParallelWriter ECB;
            public float GlobalDeltaTime;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                this.Read(BlendData, entryIndex, out var targetEntity, out var mixData);

                float2 blendedDirection = JobHelpers.Blend<float2, Float2Mixer>(ref mixData, float2.zero);
                float totalTimelineWeight = math.min(1f, mixData.Weights.x + mixData.Weights.y + mixData.Weights.z + mixData.Weights.w);
                
                if (totalTimelineWeight <= 0f || !TargetToTrack.TryGetValue(targetEntity, out var trackEntity) || 
                    !TargetToTime.TryGetValue(targetEntity, out var absoluteTime) || 
                    !MotionBufferLookup.TryGetBuffer(trackEntity, out var motions) ||
                    !TrackDataLookup.TryGetComponent(trackEntity, out var trackData)) return;

                // --- NEW: Apply Track Fallback Override to Target ---
                if (FallbackOverrideLookup.TryGetComponent(trackEntity, out var fallbackOverride))
                {
                    ECB.SetComponent(entryIndex, targetEntity, new BlendGroupFallBackForNoAnimationToProcessComponent
                    {
                        ClipHash = fallbackOverride.FallbackClipHash,
                        BlendInSpeed = fallbackOverride.BlendInSpeed,
                        BlendOutSpeed = fallbackOverride.BlendOutSpeed
                    });
                }

                var blendTreeClips = new NativeArray<BlobAssetReference<AnimationClipBlob>>(motions.Length, Allocator.Temp);
                var blendTreePositions = new NativeArray<ScriptedAnimator.BlendTree2DMotionElement>(motions.Length, Allocator.Temp);

                for (int i = 0; i < motions.Length; i++)
                {
                    var motionData = motions[i];
                    if (AnimDB.TryGetValue(motionData.AnimationHash, out var clipBlob))
                        blendTreeClips[i] = clipBlob;
                    else
                        blendTreeClips[i] = BlobAssetReference<AnimationClipBlob>.Null;

                    blendTreePositions[i] = motionData.BlendTree2DMotionElement;
                }

                var internalWeights = trackData.BlendTreeType switch
                {
                    MotionBlob.Type.BlendTree2DSimpleDirectional => ScriptedAnimator.ComputeBlendTree2DSimpleDirectional(blendTreePositions.AsReadOnly(), blendedDirection),
                    MotionBlob.Type.BlendTree2DFreeformCartesian => ScriptedAnimator.ComputeBlendTree2DFreeformCartesian(blendTreePositions.AsReadOnly(), blendedDirection),
                    MotionBlob.Type.BlendTree2DFreeformDirectional => ScriptedAnimator.ComputeBlendTree2DFreeformDirectional(blendTreePositions.AsReadOnly(), blendedDirection),
                    _ => default
                };

                float weightedDuration = 0f;
                float totalBlendWeight = 0f;

                for (int i = 0; i < internalWeights.Length; i++)
                {
                    var mw = internalWeights[i];
                    var clipBlob = blendTreeClips[mw.motionIndex];
                    if (clipBlob.IsCreated)
                    {
                        weightedDuration += clipBlob.Value.length * mw.weight;
                        totalBlendWeight += mw.weight;
                    }
                }

                if (totalBlendWeight > 0f) weightedDuration /= totalBlendWeight;
                if (weightedDuration <= 0.001f) weightedDuration = 1f;

                float normalizedTime = 0f;

                if (PlaybackStateLookup.TryGetComponent(trackEntity, out var playbackState))
                {
                    if (!playbackState.IsInitialized)
                    {
                        float initialTime = absoluteTime / weightedDuration;
                        playbackState.AccumulatedTime = initialTime;
                        playbackState.PreviousAbsoluteTime = absoluteTime;
                        playbackState.IsInitialized = true;
                        normalizedTime = math.frac(initialTime);
                    }
                    else
                    {
                        float delta = absoluteTime - playbackState.PreviousAbsoluteTime;
                        if (math.abs(delta) > 1.0f) delta = GlobalDeltaTime;
                        playbackState.AccumulatedTime += (delta / weightedDuration);
                        playbackState.PreviousAbsoluteTime = absoluteTime;
                        normalizedTime = math.frac(playbackState.AccumulatedTime);
                    }
                    PlaybackStateLookup[trackEntity] = playbackState; 
                }

                for (int i = 0; i < internalWeights.Length; i++)
                {
                    var mw = internalWeights[i];
                    var clipBlob = blendTreeClips[mw.motionIndex];
                    
                    if (clipBlob.IsCreated && mw.weight > 0f)
                    {
                        ECB.AppendToBuffer(entryIndex, targetEntity, new BlendGroupEntry
                        {
                            LayerIndex = trackData.LayerIndex,
                            ClipHash = clipBlob.Value.hash,
                            NormalizedTime = normalizedTime,
                            Weight = mw.weight * totalTimelineWeight, 
                            AvatarMaskHash = default,
                            BlendMode = AnimationBlendingMode.Override
                        });
                    }
                }

                internalWeights.Dispose();
                blendTreeClips.Dispose();
                blendTreePositions.Dispose();
            }
        }
    }
}