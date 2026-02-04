using BovineLabs.Core.Collections;
using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Properties;

namespace Movements.Movement.Timeline.Data
{
    /// <summary>
    /// Animated component for linear movement position.
    /// Implements IAnimatedComponent<float3> for Timeline blending.
    /// Timeline automatically blends between clips and writes the blended value to LocalTransform.
    /// </summary>
    public struct LinearMovementAnimated : IAnimatedComponent<float3>
    {
        /// <summary>
        /// The target position to animate to (lerped from start position).
        /// Timeline blends this value across multiple overlapping clips.
        /// </summary>
        [CreateProperty]
        public float3 Value { get; set; }
    }

    /// <summary>
    /// Animated component for linear movement rotation.
    /// Implements IAnimatedComponent<quaternion> for Timeline blending.
    /// Optional - only added when rotation is enabled on the clip.
    /// </summary>
    public struct LinearMovementRotationAnimated : IAnimatedComponent<quaternion>
    {
        /// <summary>
        /// The target rotation to animate to (slerped from start rotation).
        /// Timeline blends this value across multiple overlapping clips.
        /// </summary>
        [CreateProperty]
        public quaternion Value { get; set; }
    }

    /// <summary>
    /// Stores the captured start position and rotation for timeline-driven movement.
    /// Populated when timeline activates.
    /// </summary>
    public struct LinearMovementStartPosition : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
        public bool IsValid;
    }

    /// <summary>
    /// Tag component to reset position when clip deactivates.
    /// When enabled, the object returns to its start position when timeline ends.
    /// </summary>
    public struct LinearMovementResetOnDeactivate : IComponentData
    {
        /// <summary>
        /// Whether to reset position on deactivation.
        /// </summary>
        public bool ResetPosition;

        /// <summary>
        /// Whether to reset rotation on deactivation.
        /// </summary>
        public bool ResetRotation;
    }
}
