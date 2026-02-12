using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    [TrackClipType(typeof(CompanionLinkInstantiateClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(GameObject))]
    [DisplayName("DOTS/" + "GameObjectInstantiateTrack")]
    public class CompanionLinkInstantiateTrack : DOTSTrack
    {
        protected override void Bake(BakingContext context)
        {
        }
    }
}