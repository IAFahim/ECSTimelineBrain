using BovineLabs.Timeline.Tracks.Data.Instantiates;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class InstantiateClip : DOTSClip, ITimelineClipAsset
    {
        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.None;
        public GameObject prefab;
        public bool parent;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new InstantiateComponent
            {
                Prefab = context.Baker.GetEntity(prefab, TransformUsageFlags.None),
                Parent = parent
            });
            base.Bake(clipEntity, context);
        }
    }
}