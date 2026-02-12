using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    [TrackClipType(typeof(CompanionActivationClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(GameObject))]
    [DisplayName("DOTS/" + nameof(CompanionActivationTrack))]
    public class CompanionActivationTrack : DOTSTrack
    {
        protected override void Bake(BakingContext context)
        {
        }
    }
}