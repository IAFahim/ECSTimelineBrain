// <copyright file="PositionClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using BovineLabs.Timeline.Tracks.Data;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class PositionClip : DOTSClip, ITimelineClipAsset
    {
        public PositionType Type;
        public Vector3 Position;
        public GameObject Target;

        public OffsetType OffsetType = OffsetType.Local;
        public Vector3 Offset;

        public ClipCaps clipCaps => ClipCaps.Blending;

        /// <inheritdoc />
        public override void Bake(Entity clipEntity, BakingContext context)
        {
            // This value is used for PositionType.World, everything else will override it before use
            context.Baker.AddComponent(clipEntity, new PositionAnimated { Value = Position });
            context.Baker.AddTransformUsageFlags(context.Binding!.Target, TransformUsageFlags.Dynamic);

            switch (Type)
            {
                case PositionType.World:
                    break;
                case PositionType.Offset:
                    context.Baker.AddComponent(clipEntity, new PositionOffset { Type = OffsetType, Offset = Offset });
                    break;
                case PositionType.Target:
                    var target = context.Baker.GetEntity(Target, TransformUsageFlags.Dynamic);
                    context.Baker.AddComponent(clipEntity,
                        new PositionTarget { Target = target, Type = OffsetType, Offset = Offset });
                    break;
            }

            base.Bake(clipEntity, context);
        }
    }

    public enum PositionType : byte
    {
        World,
        Offset,
        Target
    }
}