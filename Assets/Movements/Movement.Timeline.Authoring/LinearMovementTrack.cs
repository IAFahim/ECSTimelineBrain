using System.ComponentModel;
using BovineLabs.Timeline.Authoring;
using Movements.Movement.Timeline.Data;
using UnityEngine;
using UnityEngine.Timeline;

namespace Movements.Movement.Timeline.Authoring
{
    /// <summary>
    ///     Timeline track for linear movement clips.
    ///     Binds to Transform components and controls position/rotation via Timeline.
    /// </summary>
    [TrackClipType(typeof(LinearMovementClip))]
    [TrackBindingType(typeof(Transform))]
    [TrackColor(0.2f, 0.8f, 0.4f)]
    [DisplayName("DOTS/Movements/Linear Movement")]
    public class LinearMovementTrack : DOTSTrack
    {
        [Header("Timeline Settings")] [Tooltip("Reset position to start when clip deactivates")]
        public bool ResetPositionOnDeactivate;

        [Tooltip("Reset rotation to start when clip deactivates")]
        public bool ResetRotationOnDeactivate;

        /// <summary>
        ///     Bake the track into ECS components.
        ///     Called once per track during Timeline baking.
        /// </summary>
        protected override void Bake(BakingContext context)
        {
            // Add component to store start position (captured when timeline activates)
            context.Baker.AddComponent<LinearMovementStartPosition>(context.TrackEntity);

            // Add reset behavior if enabled
            if (ResetPositionOnDeactivate || ResetRotationOnDeactivate)
                context.Baker.AddComponent(context.TrackEntity, new LinearMovementResetOnDeactivate
                {
                    ResetPosition = ResetPositionOnDeactivate,
                    ResetRotation = ResetRotationOnDeactivate
                });
        }
    }
}