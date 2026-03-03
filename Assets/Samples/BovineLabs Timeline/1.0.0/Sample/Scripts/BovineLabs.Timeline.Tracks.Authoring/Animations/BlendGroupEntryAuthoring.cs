using BovineLabs.Timeline.Data;
using Rukhanka;
using Rukhanka.Hybrid;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Authoring
{

    public class BlendGroupEntryAuthoring : MonoBehaviour
    {
        public AnimationClip fallbackAnimationClip;
        public float blendInDuration; 
        public float blendOutDuration; 
        
        public class BlendGroupEntryBaker : Baker<BlendGroupEntryAuthoring>
        {
            public override void Bake(BlendGroupEntryAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                
                // 1. Resolve the Rig Definition to get the Avatar
                var rigDef = GetComponent<RigDefinitionAuthoring>();
                if (rigDef == null)
                    rigDef = GetComponentInParent<RigDefinitionAuthoring>();

                Avatar avatar = rigDef != null ? rigDef.GetAvatar() : null;
                Hash128 fallbackHash = default;

                // 2. Bake the clip if it's assigned
                if (authoring.fallbackAnimationClip != null)
                {
                    fallbackHash = BakingUtils.ComputeAnimationHash(authoring.fallbackAnimationClip, avatar);

                    // 3. Bake the animation into a Rukhanka Blob
                    var animationBaker = new AnimationClipBaker();
                    var bakedAnimations = animationBaker.BakeAnimations(
                        this, 
                        new[] { authoring.fallbackAnimationClip }, 
                        avatar, 
                        rigDef != null ? rigDef.gameObject : authoring.gameObject
                    );

                    // 4. Register the baked blob into the Database
                    var dbBuffer = AddBuffer<NewBlobAssetDatabaseRecord<AnimationClipBlob>>(entity);
                    if (bakedAnimations.IsCreated && bakedAnimations.Length > 0 && bakedAnimations[0] != BlobAssetReference<AnimationClipBlob>.Null)
                    {
                        dbBuffer.Add(new NewBlobAssetDatabaseRecord<AnimationClipBlob>
                        {
                            hash = fallbackHash,
                            value = bakedAnimations[0]
                        });
                    }

                    if (bakedAnimations.IsCreated)
                        bakedAnimations.Dispose();
                }

                AddComponent<BlendGroupTimer>(entity);
                
                // 5. Save the Hash and Durations to be used by your runtime systems
                AddComponent(entity, new BlendGroupFallBackForNoAnimationToProcessComponent
                {
                    ClipHash = fallbackHash,
                    // BlendInDuration = authoring.blendInDuration,
                    // BlendOutDuration = authoring.blendOutDuration
                });
                
                AddBuffer<BlendGroupEntry>(entity);
            }
        }
    }
}