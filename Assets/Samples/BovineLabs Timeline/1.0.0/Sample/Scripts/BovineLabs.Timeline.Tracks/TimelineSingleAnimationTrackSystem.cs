using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Authoring
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))][UpdateBefore(typeof(TimelineAnimationUnificationSystem))]
    public partial struct TimelineSingleAnimationTrackSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlobDatabaseSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var blobDB = SystemAPI.GetSingleton<BlobDatabaseSingleton>();
            
            // We use a local ECB played back immediately to safely allow parallel clips to append to the same target entity buffer
            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            var job = new ProcessSingleClipsJob
            {
                AnimDB = blobDB.animations,
                TrackDataLookup = SystemAPI.GetComponentLookup<RukhankaSingleTrackData>(true),
                ClipWeightLookup = SystemAPI.GetComponentLookup<ClipWeight>(true),
                ECB = ecb.AsParallelWriter()
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
            
            // Complete and playback immediately so the buffer is ready for the Unification system this frame
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        [WithAll(typeof(ClipActive))]
        private partial struct ProcessSingleClipsJob : IJobEntity
        {
            [ReadOnly] public NativeHashMap<Hash128, BlobAssetReference<AnimationClipBlob>> AnimDB;
            [ReadOnly] public ComponentLookup<RukhankaSingleTrackData> TrackDataLookup;
            [ReadOnly] public ComponentLookup<ClipWeight> ClipWeightLookup;
            
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(Entity clipEntity,[ChunkIndexInQuery] int chunkIndex, in TrackBinding binding, in Clip clip, in LocalTime localTime, in RukhankaSingleClipData clipData)
            {
                var target = binding.Value;
                var track = clip.Track;

                if (!TrackDataLookup.TryGetComponent(track, out var trackData)) return;
                
                float timelineWeight = ClipWeightLookup.HasComponent(clipEntity) ? ClipWeightLookup[clipEntity].Value : 1f;
                if (timelineWeight <= 0f) return;

                if (!AnimDB.TryGetValue(clipData.ClipHash, out var clipBlob) || !clipBlob.IsCreated) return;

                float duration = math.max(0.001f, clipBlob.Value.length);
                float absoluteTime = (float)localTime.Value;
                
                float normalizedTime = clipBlob.Value.looped 
                    ? math.frac(absoluteTime / duration) 
                    : math.saturate(absoluteTime / duration);

                ECB.AppendToBuffer(chunkIndex, target, new BlendGroupEntry
                {
                    LayerIndex = trackData.LayerIndex,
                    ClipHash = clipData.ClipHash,
                    NormalizedTime = normalizedTime,
                    Weight = timelineWeight,
                    AvatarMaskHash = default, // Support can be added here later if needed
                    BlendMode = AnimationBlendingMode.Override
                });
            }
        }
    }
}