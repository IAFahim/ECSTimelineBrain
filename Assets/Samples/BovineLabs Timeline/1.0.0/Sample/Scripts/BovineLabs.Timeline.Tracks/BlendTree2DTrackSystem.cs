using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Tracks.Systems
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
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
                AnimClipBlobHashMap = blobDB.animations,
                AnimationToProcessComponentLookup = animationBufferLookup,
                BlendTrack2DDataComponentLookup = trackDataLookup,
                BlendTree2DMotionDataBufferLookup = motionBufferLookup,
                ClipWeightComponentLookup = clipWeightLookup
            }.ScheduleParallel();
        }


        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct ProcessBlendClipsJob : IJobEntity
        {
            [NativeDisableParallelForRestriction]
            public BufferLookup<AnimationToProcessComponent> AnimationToProcessComponentLookup;

            [ReadOnly] public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> AnimClipBlobHashMap;
            [ReadOnly] public ComponentLookup<BlendTree2DTrackData> BlendTrack2DDataComponentLookup;
            [ReadOnly] public BufferLookup<BlendTree2DMotionData> BlendTree2DMotionDataBufferLookup;
            [ReadOnly] public ComponentLookup<ClipWeight> ClipWeightComponentLookup;

            private void Execute(Entity clipEntity, in TrackBinding binding, in Clip clip,
                in BlendTree2DClipData clipData, in LocalTime localTime)
            {
                var currentTarget = binding.Value;
                var currentTrack = clip.Track;

                if (!AnimationToProcessComponentLookup.TryGetBuffer(currentTarget,
                        out DynamicBuffer<AnimationToProcessComponent> animationToProcess)) return;

                // Note: If you have multiple timeline tracks writing to the same entity, 
                // you shouldn't clear here. We save the startIndex so we only modify clips WE added.
                animationToProcess.Clear();
                int startIndex = animationToProcess.Length;

                if (!BlendTree2DMotionDataBufferLookup.TryGetBuffer(currentTrack, out var motions)) return;
                if (!BlendTrack2DDataComponentLookup.TryGetComponent(currentTrack, out var trackData)) return;

                float timelineTrackWeight =
                    ClipWeightComponentLookup.HasComponent(clipEntity) ? ClipWeightComponentLookup[clipEntity].Value : 1f;
                if (timelineTrackWeight <= 0f) return;

                var blendTreeClips =
                    new NativeArray<BlobAssetReference<AnimationClipBlob>>(motions.Length, Allocator.Temp);
                var blendTreePositions =
                    new NativeArray<ScriptedAnimator.BlendTree2DMotionElement>(motions.Length, Allocator.Temp);

                for (int i = 0; i < motions.Length; i++)
                {
                    var motionData = motions[i];
                    if (AnimClipBlobHashMap.TryGetValue(motionData.AnimationHash, out var clipBlob))
                        blendTreeClips[i] = clipBlob;
                    else
                        blendTreeClips[i] = BlobAssetReference<AnimationClipBlob>.Null;

                    blendTreePositions[i] = motionData.BlendTree2DMotionElement;
                }

                
                ScriptedAnimator.PlayBlendTree2D(
                    ref animationToProcess,
                    blendTreeClips,
                    blendTreePositions,
                    clipData.Value,
                    0f, // DUMMY TIME! We will overwrite this below.
                    trackData.BlendTreeType,
                    1f,
                    default
                );

                // 2. Read the buffer to calculate the true weighted duration of the blend tree
                float weightedDuration = 0f;
                float totalBlendWeight = 0f;

                for (int i = startIndex; i < animationToProcess.Length; i++)
                {
                    var anim = animationToProcess[i];
                    if (anim.animation.IsCreated)
                    {
                        // anim.weight here is the exact directional blend ratio Rukhanka calculated!
                        weightedDuration += anim.animation.Value.length * anim.weight;
                        totalBlendWeight += anim.weight;
                    }
                }

                // Avoid DivideByZero
                if (totalBlendWeight > 0f) weightedDuration /= totalBlendWeight;
                if (weightedDuration <= 0.001f) weightedDuration = 1f;

                // 3. Calculate Actual Normalized Time
                float absoluteTime = (float)localTime.Value;
                float normalizedTime = 0f;
                normalizedTime = math.frac(absoluteTime / weightedDuration);


                for (int i = startIndex; i < animationToProcess.Length; i++)
                {
                    var anim = animationToProcess[i];

                    anim.time = normalizedTime;
                    anim.layerWeight = timelineTrackWeight;

                    
                    animationToProcess[i] = anim;
                }

                blendTreeClips.Dispose();
                blendTreePositions.Dispose();
            }
        }
    }
}