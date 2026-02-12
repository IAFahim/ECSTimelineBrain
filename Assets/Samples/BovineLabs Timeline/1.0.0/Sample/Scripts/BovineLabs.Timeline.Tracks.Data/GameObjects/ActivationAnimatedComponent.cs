using BovineLabs.Timeline.Data;
using Unity.Properties;

namespace BovineLabs.Timeline.Tracks.Data.GameObjects
{
    public struct ActivationAnimatedComponent : IAnimatedComponent<bool>
    {
        [CreateProperty] public bool Value { get; set; } 
    }
}