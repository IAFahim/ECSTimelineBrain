using BovineLabs.Timeline.Tracks.Data.GameObjects;
using Unity.Entities;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class ActivationClip : DOTSClip, ITimelineClipAsset
    {
        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.Looping;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new ActivationAnimatedComponent { Value = false });
            base.Bake(clipEntity, context);
        }
    }
}