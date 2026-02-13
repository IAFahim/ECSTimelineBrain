using System;
using System.ComponentModel;
using BovineLabs.Reaction.Authoring.Core;
using BovineLabs.Reaction.Data.Core;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    [TrackClipType(typeof(TargetClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(TargetsAuthoring))]
    [DisplayName("DOTS/" + nameof(TargetTrack))]
    public class TargetTrack : DOTSTrack
    {

        [Tooltip("What should Target be set to on Instantiation.")]
        public Target Target = Target.Target;
        
        protected override void Bake(BakingContext context)
        {
            
        }
    }
}