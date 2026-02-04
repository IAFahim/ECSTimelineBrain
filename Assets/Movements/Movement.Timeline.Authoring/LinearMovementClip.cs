using BovineLabs.Core.Authoring;
using BovineLabs.Timeline.Authoring;
using Movements.Movement.Timeline.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Timeline;

namespace Movements.Movement.Timeline.Authoring
{
    /// <summary>
    /// Timeline clip asset for linear movement animation.
    /// Adds IAnimatedComponent data that Timeline blends and writes to LocalTransform.
    /// </summary>
    public class LinearMovementClip : DOTSClip, ITimelineClipAsset
    {
        [Header("Position Settings")]
        [Tooltip("Target position to animate to")]
        public float3 EndPosition = new(5f, 0f, 0f);

        [Header("Rotation Settings")]
        [Tooltip("Enable rotation animation")]
        public bool HasRotation;

        [Tooltip("End rotation Euler angles (in degrees)")]
        public Vector3 EndRotationEuler = new Vector3(0f, 180f, 0f);

        /// <summary>
        /// Enable Timeline blending for this clip type.
        /// </summary>
        public ClipCaps clipCaps => ClipCaps.Blending;

        /// <summary>
        /// Bake the clip into ECS components.
        /// This is called during Timeline baking/conversion.
        /// </summary>
        public override void Bake(Entity clipEntity, BakingContext context)
        {
            // Add the animated position component
            // Timeline will blend this value across overlapping clips
            context.Baker.AddComponent(clipEntity, new LinearMovementAnimated
            {
                Value = EndPosition
            });

            // Add rotation animation if enabled (convert Euler to quaternion)
            if (HasRotation)
            {
                context.Baker.AddComponent(clipEntity, new LinearMovementRotationAnimated
                {
                    Value = quaternion.Euler(math.radians(EndRotationEuler))
                });
            }

            // Ensure the target entity can have its transform modified
            context.Baker.AddTransformUsageFlags(context.Binding!.Target, TransformUsageFlags.Dynamic);

            // Call base to setup timing, active state, blending, extrapolation
            base.Bake(clipEntity, context);
        }
    }
}
