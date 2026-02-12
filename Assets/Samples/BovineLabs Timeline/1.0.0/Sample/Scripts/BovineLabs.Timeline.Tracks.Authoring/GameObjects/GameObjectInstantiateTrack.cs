using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    [TrackClipType(typeof(GameObjectInstantiateClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(GameObject))]
    [DisplayName("DOTS/" + "GameObjectInstantiateTrack")]
    public class GameObjectInstantiateTrack : DOTSTrack
    {
        protected override void Bake(BakingContext context)
        {
        }
    }
}