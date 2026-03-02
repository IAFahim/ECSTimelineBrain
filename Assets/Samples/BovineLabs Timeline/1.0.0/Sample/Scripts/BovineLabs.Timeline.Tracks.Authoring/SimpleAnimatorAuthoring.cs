using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka;
using Rukhanka.Hybrid;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Authoring
{
    [RequireComponent(typeof(RigDefinitionAuthoring))]
    public class SimpleAnimatorAuthoring : MonoBehaviour
    {
        public AnimationClip DefaultClip;
        public float DefaultSpeed = 1f;
        public float DefaultTransitionDuration = 0.25f;

        public class Baker : Baker<SimpleAnimatorAuthoring>
        {
            public override void Bake(SimpleAnimatorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                
                Hash128 defaultClipHash = default;

                if (authoring.DefaultClip != null)
                {
                    var rigDef = GetComponent<RigDefinitionAuthoring>();
                    var avatar = rigDef.GetAvatar();

                    // Bake the animation
                    var animationBaker = new AnimationClipBaker();
                    var bakedAnimations = animationBaker.BakeAnimations(this, new[] { authoring.DefaultClip }, avatar, authoring.gameObject);

                    var e = CreateAdditionalEntity(TransformUsageFlags.None, false, authoring.name + "_SimpleAnimAsset");
                    var dbBuffer = AddBuffer<NewBlobAssetDatabaseRecord<AnimationClipBlob>>(e);
                    dbBuffer.AddValidAnimations(bakedAnimations);

                    defaultClipHash = authoring.DefaultClip.ComputeHashOrDefault(avatar);
                    if (bakedAnimations.IsCreated) bakedAnimations.Dispose();
                }

                AddComponent(entity, new SimpleAnimatorComponent
                {
                    DefaultTransitionDuration = authoring.DefaultTransitionDuration,
                    CurrentClip = defaultClipHash,
                    CurrentSpeed = authoring.DefaultSpeed,
                    CurrentTime = 0f,
                    CurrentMotionId = 1 // Start at 1
                });
            }
        }
    }
}