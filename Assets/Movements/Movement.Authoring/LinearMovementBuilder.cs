// <copyright file="LinearMovementBuilder.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace Movements.Movement.Authoring
{
    using BovineLabs.Core.EntityCommands;
    using Movements.Movement.Data;
    using Unity.Entities;
    using Unity.Mathematics;

    /// <summary>
    /// Builder for configuring linear movement settings, including position, speed, and optional rotation.
    /// </summary>
    public struct LinearMovementBuilder
    {
        private float3 startPosition;
        private float3 endPosition;
        private float speed;
        private float progress;

        private bool hasRotation;
        private quaternion startRotation;
        private quaternion endRotation;

        /// <summary>
        /// Sets the start and end positions for the linear movement.
        /// </summary>
        /// <param name="start">The starting position.</param>
        /// <param name="end">The ending position.</param>
        public void WithPosition(float3 start, float3 end)
        {
            this.startPosition = start;
            this.endPosition = end;
        }

        /// <summary>
        /// Sets the movement speed in units per second.
        /// </summary>
        /// <param name="value">The speed value.</param>
        public void WithSpeed(float value)
        {
            this.speed = value;
        }

        /// <summary>
        /// Sets the initial normalized progress (0 to 1).
        /// </summary>
        /// <param name="value">The progress value.</param>
        public void WithProgress(float value)
        {
            this.progress = value;
        }

        /// <summary>
        /// Configures the rotation for the linear movement.
        /// </summary>
        /// <param name="start">The starting rotation.</param>
        /// <param name="end">The ending rotation.</param>
        public void WithRotation(quaternion start, quaternion end)
        {
            this.hasRotation = true;
            this.startRotation = start;
            this.endRotation = end;
        }

        /// <summary>
        /// Applies the configured linear movement settings to the specified entity builder.
        /// </summary>
        /// <typeparam name="T">The type of entity command builder.</typeparam>
        /// <param name="builder">The entity builder to apply movement settings to.</param>
        public void ApplyTo<T>(ref T builder)
            where T : struct, IEntityCommands
        {
            builder.AddComponent<LinearMovementTag>();

            builder.AddComponent(new StartPositionComponent { value = this.startPosition });
            builder.AddComponent(new EndPositionComponent { value = this.endPosition });
            builder.AddComponent(new SpeedComponent { value = this.speed });
            builder.AddComponent(new NormalizedProgress { value = this.progress });

            if (this.hasRotation)
            {
                builder.AddComponent(new StartQuaternionComponent { value = this.startRotation });
                builder.AddComponent(new EndQuaternionComponent { value = this.endRotation });
            }
            else
            {
                builder.AddComponent<WithoutRotationTag>();
            }
        }
    }
}