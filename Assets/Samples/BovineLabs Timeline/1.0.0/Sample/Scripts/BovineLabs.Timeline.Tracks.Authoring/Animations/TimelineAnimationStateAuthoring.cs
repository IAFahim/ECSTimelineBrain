using BovineLabs.Timeline.Data;
using Rukhanka;
using Rukhanka.Hybrid;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Authoring
{
    // Forces this component to sit next to Rukhanka's Rig[RequireComponent(typeof(RigDefinitionAuthoring))]
    public class TimelineAnimationStateAuthoring : MonoBehaviour
    {
        [Tooltip("The animation to play when no timeline clips are active.")]
        public AnimationClip fallbackAnimationClip;

        [Tooltip("Time in seconds to smoothly transition from Fallback to Timeline.")] [Min(0.001f)]
        public float blendInDuration = 0.25f;

        [Tooltip("Time in seconds to smoothly transition from Timeline back to Fallback.")] [Min(0.001f)]
        public float blendOutDuration = 0.25f;

        public class Baker : Baker<TimelineAnimationStateAuthoring>
        {
            public override void Bake(TimelineAnimationStateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var rigDef = GetComponent<RigDefinitionAuthoring>();
                var avatar = rigDef != null ? rigDef.GetAvatar() : null;

                Hash128 fallbackHash = default;

                // 1. Bake the fallback clip into Rukhanka's database
                if (authoring.fallbackAnimationClip != null)
                {
                    fallbackHash = BakingUtils.ComputeAnimationHash(authoring.fallbackAnimationClip, avatar);

                    var animationBaker = new AnimationClipBaker();
                    var bakedAnimations = animationBaker.BakeAnimations(this, new[] { authoring.fallbackAnimationClip },
                        avatar, authoring.gameObject);

                    var dbBuffer = AddBuffer<NewBlobAssetDatabaseRecord<AnimationClipBlob>>(entity);
                    if (bakedAnimations.IsCreated && bakedAnimations.Length > 0 &&
                        bakedAnimations[0] != BlobAssetReference<AnimationClipBlob>.Null)
                    {
                        dbBuffer.Add(new NewBlobAssetDatabaseRecord<AnimationClipBlob>
                        {
                            hash = fallbackHash,
                            value = bakedAnimations[0]
                        });
                    }

                    if (bakedAnimations.IsCreated) bakedAnimations.Dispose();
                }

                // 2. Initialize the crossfade timer (Starts at 0: Fully Fallback)
                AddComponent(entity, new BlendGroupTimer
                {
                    TimelineWeight = 0f,
                    FallbackAccumulatedTime = 0f
                });

                // 3. Save speeds and hash. (Speed = 1/Duration). 
                // We use speed to avoid doing division in our Bursted runtime jobs.
                AddComponent(entity, new BlendGroupFallBackForNoAnimationToProcessComponent
                {
                    ClipHash = fallbackHash,
                    BlendInSpeed = 1f / Mathf.Max(0.001f, authoring.blendInDuration),
                    BlendOutSpeed = 1f / Mathf.Max(0.001f, authoring.blendOutDuration)
                });

                // 4. Provision the Unification Buffer
                AddBuffer<BlendGroupEntry>(entity);
            }
        }
    }
}