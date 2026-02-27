using System;
using System.ComponentModel;
using BovineLabs.Timeline.Tracks.Data.GameObjects;
using Samples.BovineLabs_Timeline._1._0._0.Sample.Scripts.BovineLabs.Timeline.Tracks.Data.Parenting;
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