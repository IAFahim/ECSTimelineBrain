using Unity.Entities;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Tracks.Data.GameObjects
{
    public struct ActivationTrackComponent : IComponentData
    {
        public ActivationTrack.PostPlaybackState PostPlaybackState;
    }

    public struct OriginalWasDisabledTag : IComponentData
    {
    }
}