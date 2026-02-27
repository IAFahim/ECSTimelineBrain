using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    [TrackClipType(typeof(UnParentingClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(GameObject))]
    [DisplayName("DOTS/" + nameof(ParentingTrack))]
    public class ParentingTrack : DOTSTrack
    {
    }
}