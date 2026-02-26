using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka;
using Rukhanka.Hybrid;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Timeline.Authoring
{
    // A must for animation as without if no animation is playing it would freeze into last position
    public class IdleAnimationClipAuthoring : MonoBehaviour
    {
        public AnimationClip AnimationClip;

        public class IdleAnimationClipBaker : Baker<IdleAnimationClipAuthoring>
        {
            public override void Bake(IdleAnimationClipAuthoring authoring)
            {
                if (authoring.AnimationClip == null)
                    return;

                var rigDef = authoring.GetComponent<RigDefinitionAuthoring>();
                if (rigDef == null)
                    return;

                var avatar = rigDef.GetAvatar();

                var animationBaker = new AnimationClipBaker();
                var bakedAnimations = animationBaker.BakeAnimations(
                    this,
                    new[] { authoring.AnimationClip },
                    avatar,
                    authoring.gameObject
                );

                var e = CreateAdditionalEntity(TransformUsageFlags.None, false, authoring.name + "_IdleAnimationAsset");
                var buffer = AddBuffer<NewBlobAssetDatabaseRecord<AnimationClipBlob>>(e);
                buffer.AddValidAnimations(bakedAnimations);

                if (bakedAnimations.IsCreated) bakedAnimations.Dispose();

                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new IdleAnimationClip
                {
                    AnimationHash = authoring.AnimationClip.ComputeHashOrDefault(avatar),
                });
            }
        }
    }
}