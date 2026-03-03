using BovineLabs.Timeline.Data;
using Rukhanka;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Authoring
{[UpdateInGroup(typeof(TimelineComponentAnimationGroup), OrderLast = true)]
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
                ref DynamicBuffer<PreviousBlendGroupEntry> previousBlendEntries,
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

                // 2. MANAGE PREVIOUS ENTRIES FOR SMOOTH FADEOUTS
                if (isTimelineActive)
                {
                    // Timeline is playing. Store these entries so we know what to fade out from later.
                    previousBlendEntries.Clear();
                    for (int i = 0; i < blendEntries.Length; i++)
                    {
                        var entry = blendEntries[i];
                        previousBlendEntries.Add(new PreviousBlendGroupEntry
                        {
                            LayerIndex = entry.LayerIndex,
                            ClipHash = entry.ClipHash,
                            NormalizedTime = entry.NormalizedTime,
                            Weight = entry.Weight,
                            AvatarMaskHash = entry.AvatarMaskHash,
                            BlendMode = entry.BlendMode
                        });
                    }
                }
                else if (timer.TimelineWeight > 0.0f)
                {
                    // Timeline just stopped! We are fading down to Idle.
                    // Keep the previous animations alive and advance their time manually so they don't freeze mid-fade.
                    for (int i = 0; i < previousBlendEntries.Length; i++)
                    {
                        var prev = previousBlendEntries[i];
                        if (AnimDB.TryGetValue(prev.ClipHash, out var clipBlob) && clipBlob.IsCreated)
                        {
                            float duration = math.max(0.001f, clipBlob.Value.length);
                            
                            // Advance the animation
                            prev.NormalizedTime += DeltaTime / duration;
                            prev.NormalizedTime = math.frac(prev.NormalizedTime); 
                            
                            previousBlendEntries[i] = prev; // Save state for next frame
                            
                            // Temporarily inject it back into the blend pool so it gets rendered this frame
                            blendEntries.Add(new BlendGroupEntry
                            {
                                LayerIndex = prev.LayerIndex,
                                ClipHash = prev.ClipHash,
                                NormalizedTime = prev.NormalizedTime,
                                Weight = prev.Weight,
                                AvatarMaskHash = prev.AvatarMaskHash,
                                BlendMode = prev.BlendMode
                            });
                        }
                    }
                }

                // 3. PROCESS FALLBACK (Play if it has any visibility > 0)
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
                            motionId = 0xFFFFFFFF // Special ID to prevent Rukhanka event overlaps
                        });
                    }
                }

                // 4. PROCESS TIMELINE ENTRIES (Apply the master fade weight)
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
                                weight = entry.Weight * timer.TimelineWeight, // Apply master fadeout weight
                                avatarMask = BlobAssetReference<AvatarMaskBlob>.Null, // Update if masks are added later
                                blendMode = entry.BlendMode,
                                layerIndex = entry.LayerIndex,
                                layerWeight = 1.0f, // Layers resolve naturally in Rukhanka if weights sum properly
                                motionId = (uint)i 
                            });
                        }
                    }
                }

                // 5. CLEAN UP 
                // We clear this so the track systems can fill it fresh next frame!
                blendEntries.Clear();
            }
        }
    }
}