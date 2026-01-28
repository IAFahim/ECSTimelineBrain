namespace BovineLabs.Timeline.Authoring
{
    using BovineLabs.Timeline.Tracks.Data;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.Timeline;

    public class PositionClip : DOTSClip, ITimelineClipAsset
    {
        public PositionType Type;
        public Vector3 Position;
        public GameObject Target;

        public OffsetType OffsetType = OffsetType.Local;
        public Vector3 Offset;

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            // The value we will animate TO
            context.Baker.AddComponent(clipEntity, new PositionAnimated { Value = this.Position });
            
            // Ensure the object being moved allows dynamic transform changes
            context.Baker.AddTransformUsageFlags(context.Binding!.Target, TransformUsageFlags.Dynamic);

            switch (this.Type)
            {
                case PositionType.World:
                    // Value is already set to 'this.Position' above.
                    break;
                    
                case PositionType.Offset:
                    context.Baker.AddComponent(clipEntity, new PositionOffset { Type = this.OffsetType, Offset = this.Offset });
                    // We need to snapshot the position when the clip starts to apply the offset relative to that start
                    context.Baker.AddComponent<PositionClipSnapshot>(clipEntity);
                    break;
                    
                case PositionType.Target:
                    var target = context.Baker.GetEntity(this.Target, TransformUsageFlags.Dynamic);
                    context.Baker.AddComponent(clipEntity, new PositionTarget { Target = target, Type = this.OffsetType, Offset = this.Offset });
                    break;
            }

            base.Bake(clipEntity, context);
        }
    }

    public enum PositionType : byte
    {
        World,
        Offset,
        Target,
    }
}