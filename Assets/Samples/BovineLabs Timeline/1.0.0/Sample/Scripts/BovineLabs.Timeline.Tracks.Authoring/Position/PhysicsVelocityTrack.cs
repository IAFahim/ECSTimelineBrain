using System;
using System.ComponentModel;
using BovineLabs.Timeline.Tracks.Data;
using Unity.Physics.Authoring;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    [TrackClipType(typeof(PhysicsVelocityClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(PhysicsBodyAuthoring))]
    [DisplayName("DOTS/Physics Velocity Target")]
    public class PhysicsVelocityTrack : DOTSTrack
    {
        protected override void Bake(BakingContext context)
        {
            context.Baker.AddComponent(
                context.TrackEntity,
                new PhysicsVelocityOffset()
            );
        }
    }
}