using System;
using System.ComponentModel;
using BovineLabs.Timeline.Tracks.Data.GameObjects;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    [TrackClipType(typeof(ActivationClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(GameObject))]
    [DisplayName("DOTS/" + nameof(ActivationTrack))]
    public class ActivationTrack : DOTSTrack
    {
        public bool activationResetOnDeactivate = true;

        protected override void Bake(BakingContext context)
        {
            context.Baker.AddComponent<ActivationTrackComponent>(context.TrackEntity);

            if (!activationResetOnDeactivate) return;
            {
                context.Baker.AddComponent<ActivationResetOnDeactivate>(context.TrackEntity);
            }
        }
    }
}