using BovineLabs.Reaction.Data.Core;
using Samples.BovineLabs_Timeline._1._0._0.Sample.Scripts.BovineLabs.Timeline.Tracks.Data.Targets;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class TargetClip : DOTSClip, ITimelineClipAsset
    {
        public override double duration => 1;
        public ClipCaps clipCaps => ClipCaps.Looping;
        
        [Tooltip("What should Target be set to on Instantiation.")]
        public Target target = Target.Target;
        
        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new TargetAnimationComponent { Value = target });
            base.Bake(clipEntity, context);
        }
    }
}