using BovineLabs.Timeline.Authoring;
using Movements.Movement.Timeline.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Timeline;

namespace Movements.Movement.Timeline.Authoring
{
    /// <summary>
    ///     Timeline clip asset for linear movement animation.
    ///     Adds IAnimatedComponent data that Timeline blends and writes to LocalTransform.
    ///     Supports targeting another Transform for the end position.
    /// </summary>
    public class LinearMovementClip : DOTSClip, ITimelineClipAsset
    {
        [Header("Position Settings")] [Tooltip("Use a target Transform for the end position")]
        public bool UseTargetTransform;

        [Tooltip("Target Transform (if enabled, overrides End Position)")]
        public ExposedReference<Transform> TargetTransform;

        [Tooltip("Target position to animate to (only used if Target Transform is not set)")]
        public float3 EndPosition = new(5f, 0f, 0f);

        [Header("Rotation Settings")] [Tooltip("Enable rotation animation")]
        public bool HasRotation;

        [Tooltip("End rotation Euler angles (in degrees)")]
        public Vector3 EndRotationEuler = new(0f, 180f, 0f);

        /// <summary>
        ///     Enable Timeline blending for this clip type.
        /// </summary>
        public ClipCaps clipCaps => ClipCaps.Blending;

        /// <summary>
        ///     Bake the clip into ECS components.
        ///     This is called during Timeline baking/conversion.
        /// </summary>
        public override void Bake(Entity clipEntity, BakingContext context)
        {
            // Handle target Transform if enabled
            Transform targetTransform = null;
            if (UseTargetTransform && context.Director != null)
                targetTransform = context.Director.GetReferenceValue(TargetTransform.exposedName, out _) as Transform;

            // If using target Transform, add the component for tracking
            if (targetTransform != null)
            {
                // Get the entity for the target Transform
                var targetEntity = context.Baker.GetEntity(targetTransform, TransformUsageFlags.Dynamic);

                // Use Timeline-specific component with Entity reference (Burst-compatible)
                context.Baker.AddComponent(clipEntity, new TimelineTargetTransform
                {
                    Target = targetEntity,
                    IsValid = true
                });

                // The animated position will be updated by the target resolution job
                // Set initial value to current target position
                context.Baker.AddComponent(clipEntity, new LinearMovementAnimated
                {
                    Value = targetTransform.position
                });
            }
            else
            {
                // Use static end position
                context.Baker.AddComponent(clipEntity, new LinearMovementAnimated
                {
                    Value = EndPosition
                });
            }

            // Add rotation animation if enabled (convert Euler to quaternion)
            if (HasRotation)
                context.Baker.AddComponent(clipEntity, new LinearMovementRotationAnimated
                {
                    Value = quaternion.Euler(math.radians(EndRotationEuler))
                });

            // Ensure the target entity can have its transform modified
            context.Baker.AddTransformUsageFlags(context.Binding!.Target, TransformUsageFlags.Dynamic);

            // Call base to setup timing, active state, blending, extrapolation
            base.Bake(clipEntity, context);
        }
    }
}