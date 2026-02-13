using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Tracks.Data.GameObjects
{
    using Unity.Entities;

    public struct ActivationTrackComponent : IComponentData
    {
        public PostPlaybackState PostPlaybackState;
    }

    public struct OriginalWasDisabledTag : IComponentData
    {
        
    }
}