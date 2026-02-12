using BovineLabs.Timeline.Data;
using Unity.Properties;

namespace BovineLabs.Timeline.Tracks.Data.GameObjects
{
    public struct ActivationAnimatedComponent : IAnimatedComponent<float>
    {
        [CreateProperty] public float Value { get; set; } // 1 for true, 0 for false
    }
}