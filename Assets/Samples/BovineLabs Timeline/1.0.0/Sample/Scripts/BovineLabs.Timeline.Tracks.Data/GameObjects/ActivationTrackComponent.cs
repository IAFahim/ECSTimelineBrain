using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Tracks.Data.GameObjects
{
    using Unity.Entities;

    public struct ActivationTrackComponent : IComponentData
    {
        public PostPlaybackState PostPlaybackState;
        
        // Stored state for the 'Revert' logic. 
        // True if the entity was Disabled when timeline started.
        public bool OriginalWasDisabled; 
    }
}