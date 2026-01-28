namespace BovineLabs.Timeline.Authoring
{
    using BovineLabs.Timeline.Tracks.Data;
    using Unity.Entities;
    using UnityEngine.Timeline;

    public class PositionStartClip : DOTSClip, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Blending;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent<PositionMoveToStart>(clipEntity);
            context.Baker.AddComponent<PositionAnimated>(clipEntity);
            // We need a snapshot to grab the position when this clip begins
            context.Baker.AddComponent<PositionClipSnapshot>(clipEntity);
            
            context.Baker.AddTransformUsageFlags(context.Binding!.Target, TransformUsageFlags.Dynamic);

            base.Bake(clipEntity, context);
        }
    }
}