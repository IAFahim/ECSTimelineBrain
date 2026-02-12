using System;
using System.ComponentModel;
using BovineLabs.Timeline.Tracks.Data.GameObjects;
using UnityEngine;
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

        [Tooltip("Select the state of the bound object when the Timeline stops.")]
        public PostPlaybackState postPlaybackState = PostPlaybackState.LeaveAsIs;

        protected override void Bake(BakingContext context)
        {
            // We bake the configuration into the component
            context.Baker.AddComponent(context.TrackEntity, new ActivationTrackComponent
            {
                PostPlaybackState = postPlaybackState
            });
        }
    }
}