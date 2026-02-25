using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    [TrackClipType(typeof(PhysicsVelocityClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(Animator))]
    [DisplayName("DOTS/Physics Velocity Target")]
    public class AnimationTrack : DOTSTrack
    {
        protected override void Bake(BakingContext context)
        {
        }
    }
}