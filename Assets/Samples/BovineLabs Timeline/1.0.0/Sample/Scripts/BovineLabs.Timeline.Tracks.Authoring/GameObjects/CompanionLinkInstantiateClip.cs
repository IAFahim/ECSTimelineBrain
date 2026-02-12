using System;
using BovineLabs.Timeline.Data;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class CompanionLinkInstantiateClip : DOTSClip, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Looping;
        public override double duration => 1;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            var entity = context.Baker.GetEntity(context.Director.GetGenericBinding(context.Track) as GameObject, TransformUsageFlags.None);
            context.Baker.AddComponent(clipEntity, new CompanionLinkInstantiateComponent
            {
                prefab = entity
            });
            
            base.Bake(clipEntity, context);
        }
    }
}