using BovineLabs.Timeline.Data;
using Rukhanka;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Authoring
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup), OrderLast = true)]
    [UpdateBefore(typeof(AnimationProcessSystem))]
    public partial struct TimelineAnimationUnificationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlobDatabaseSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var blobDB = SystemAPI.GetSingleton<BlobDatabaseSingleton>();

            var job = new UnifyAnimationsJob
            {
                AnimDB = blobDB.animations,
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct UnifyAnimationsJob : IJobEntity
        {
            [ReadOnly] public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> AnimDB;
            public float DeltaTime;

            public void Execute(
                Entity entity,
                ref BlendGroupTimer timer,
                in BlendGroupFallBackForNoAnimationToProcessComponent fallbackData,
                ref DynamicBuffer<BlendGroupEntry> blendEntries,
                ref DynamicBuffer<AnimationToProcessComponent> atps)
            {
                atps.Clear();
                bool isTimelineActive = blendEntries.Length > 0;

                // 1. UPDATE CROSSFADE TIMER (0.0 = Fallback, 1.0 = Timeline)
                if (isTimelineActive)
                {
                    timer.TimelineWeight = math.saturate(timer.TimelineWeight + fallbackData.BlendInSpeed * DeltaTime);
                }
                else
                {
                    timer.TimelineWeight = math.saturate(timer.TimelineWeight - fallbackData.BlendOutSpeed * DeltaTime);
                }

                // 2. PROCESS FALLBACK (If it is visible at all)
                if (timer.TimelineWeight < 1.0f && fallbackData.ClipHash.IsValid)
                {
                    if (AnimDB.TryGetValue(fallbackData.ClipHash, out var fallbackClip) && fallbackClip.IsCreated)
                    {
                        float duration = math.max(0.001f, fallbackClip.Value.length);
                        timer.FallbackAccumulatedTime += DeltaTime / duration;
                        
                        atps.Add(new AnimationToProcessComponent
                        {
                            animation = fallbackClip,
                            time = math.frac(timer.FallbackAccumulatedTime),
                            weight = 1.0f - timer.TimelineWeight, 
                            blendMode = AnimationBlendingMode.Override,
                            layerIndex = 0,
                            layerWeight = 1.0f,
                            motionId = 0xFFFFFFFF // Special ID to prevent Rukhanka internal event overlaps
                        });
                    }
                }

                // 3. PROCESS TIMELINE ENTRIES
                if (timer.TimelineWeight > 0.0f)
                {
                    for (int i = 0; i < blendEntries.Length; i++)
                    {
                        var entry = blendEntries[i];
                        
                        if (AnimDB.TryGetValue(entry.ClipHash, out var clipBlob) && clipBlob.IsCreated)
                        {
                            atps.Add(new AnimationToProcessComponent
                            {
                                animation = clipBlob,
                                time = entry.NormalizedTime,
                                // We multiply the track's internal weight by the overall Timeline crossfade weight
                                weight = entry.Weight * timer.TimelineWeight,
                                blendMode = entry.BlendMode,
                                layerIndex = entry.LayerIndex,
                                layerWeight = 1.0f, // Normalization happens naturally by Rukhanka because weights sum correctly
                                motionId = (uint)i // Unique ID per frame position
                            });
                        }
                    }
                }

                // 4. CLEAN UP FOR NEXT FRAME
                blendEntries.Clear();
            }
        }
    }
}