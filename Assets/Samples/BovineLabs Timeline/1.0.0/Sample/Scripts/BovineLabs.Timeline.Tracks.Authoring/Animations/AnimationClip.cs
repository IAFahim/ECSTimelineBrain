using Unity.Entities;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class AnimationClip : DOTSClip, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Blending;
        public AnimationClip animationClip;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            base.Bake(clipEntity, context);
        }
    }
}