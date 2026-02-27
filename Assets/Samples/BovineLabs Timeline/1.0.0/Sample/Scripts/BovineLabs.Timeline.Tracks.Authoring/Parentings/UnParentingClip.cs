using BovineLabs.Timeline.Tracks.Data.GameObjects;
using Samples.BovineLabs_Timeline._1._0._0.Sample.Scripts.BovineLabs.Timeline.Tracks.Data.Parenting;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class UnParentingClip : DOTSClip, ITimelineClipAsset
    {
        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.Looping;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            var parent = (context.Director.GetGenericBinding(context.Track) as GameObject).transform.parent;
            context.Baker.AddComponent<UnParentComponent>(clipEntity, new UnParentComponent()
            {
                LastParent = context.Baker.GetEntity(parent, TransformUsageFlags.None)
            });
            base.Bake(clipEntity, context);
        }
    }
}