using BovineLabs.Core.EntityCommands;
using Movements.Movement.Data.Parameters.Motion;
using Movements.Movement.Data.Parameters.Timing;
using Movements.Movement.Data.Tags.MovementTypes;
using Movements.Movement.Data.Transforms.StartEnd;
using Unity.Mathematics;

namespace Movements.Movement.Authoring
{
    /// <summary>
    ///     Builder for configuring linear movement settings, including position, speed, and optional rotation.
    /// </summary>
    public struct LinearMovementBuilder
    {
        private float3 startPosition;
        private float3 endPosition;
        private float speed;
        private float progress;
        private float range;

        private bool hasRotation;
        private quaternion startRotation;
        private quaternion endRotation;

        /// <summary>
        ///     Sets the start and end positions for the linear movement.
        /// </summary>
        /// <param name="start">The starting position.</param>
        /// <param name="end">The ending position.</param>
        public void WithPositions(float3 start, float3 end)
        {
            startPosition = start;
            endPosition = end;
        }

        public void WithStartPosition(float3 start)
        {
            startPosition = start;
        }

        public void WithEndPosition(float3 end)
        {
            endPosition = end;
        }

        /// <summary>
        ///     Sets the movement speed in units per second.
        /// </summary>
        /// <param name="value">The speed value.</param>
        public void WithSpeed(float value)
        {
            speed = value;
        }

        /// <summary>
        ///     Sets the initial normalized progress (0 to 1).
        /// </summary>
        /// <param name="value">The progress value.</param>
        public void WithProgress(float value)
        {
            progress = value;
        }

        /// <summary>
        ///     Sets the range scaler for movement distance.
        /// </summary>
        /// <param name="value">The range scaler (1 = full distance, 0.5 = half, 2 = double).</param>
        public void WithRange(float value)
        {
            range = value;
        }

        /// <summary>
        ///     Configures the rotation for the linear movement.
        /// </summary>
        /// <param name="start">The starting rotation.</param>
        /// <param name="end">The ending rotation.</param>
        public void WithRotation(quaternion start, quaternion end)
        {
            hasRotation = true;
            startRotation = start;
            endRotation = end;
        }

        /// <summary>
        ///     Applies the configured linear movement settings to the specified entity builder.
        ///     With the IFacet pattern, rotation components are optional - no need for WithoutRotationTag.
        /// </summary>
        /// <typeparam name="T">The type of entity command builder.</typeparam>
        /// <param name="builder">The entity builder to apply movement settings to.</param>
        public void ApplyTo<T>(ref T builder)
            where T : struct, IEntityCommands
        {
            builder.AddComponent<LinearMovementTag>();

            builder.AddComponent(new StartPositionComponent { value = startPosition });
            builder.AddComponent(new EndPositionComponent { value = endPosition });
            builder.AddComponent(new SpeedComponent { value = speed });
            builder.AddComponent(new NormalizedProgress { value = progress });
            builder.AddComponent(new RangeComponent { value = range });

            if (hasRotation)
            {
                builder.AddComponent(new StartQuaternionComponent { value = startRotation });
                builder.AddComponent(new EndQuaternionComponent { value = endRotation });
            }
        }
    }
}