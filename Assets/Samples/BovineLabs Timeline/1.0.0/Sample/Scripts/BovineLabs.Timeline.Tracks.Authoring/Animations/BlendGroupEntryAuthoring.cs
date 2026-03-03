using BovineLabs.Timeline.Data;
using Unity.Entities;
using UnityEngine;

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
                AddComponent<BlendGroupTimer>(entity);
                AddComponent(entity, new BlendGroupFallBackForNoAnimationToProcessComponent
                {
                    // TODO:
                });
                
                AddBuffer<BlendGroupEntry>(entity);
            }
        }
    }
}