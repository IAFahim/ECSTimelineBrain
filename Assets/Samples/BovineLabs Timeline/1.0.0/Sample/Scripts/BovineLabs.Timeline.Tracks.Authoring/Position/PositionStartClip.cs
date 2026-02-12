// <copyright file="PositionStartClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using BovineLabs.Timeline.Tracks.Data;
using Unity.Entities;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class PositionStartClip : DOTSClip, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Blending;

        /// <inheritdoc />
        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent<PositionMoveToStart>(clipEntity);
            context.Baker.AddComponent<PositionAnimated>(clipEntity);
            context.Baker.AddTransformUsageFlags(context.Binding!.Target, TransformUsageFlags.Dynamic);

            base.Bake(clipEntity, context);
        }
    }
}