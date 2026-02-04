// <copyright file="LinearExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using Movements.Movement.Logic;

namespace Movements.Movement.Data
{
    using Unity.Mathematics;

    /// <summary>
    /// Extensions for linear movement data.
    /// </summary>
    public static class LinearExtensions
    {
        /// <summary>
        /// Tries to update the linear movement progress.
        /// </summary>
        /// <param name="progress">The normalized progress.</param>
        /// <param name="start">The start position.</param>
        /// <param name="end">The end position.</param>
        /// <param name="speed">The speed.</param>
        /// <param name="deltaTime">The delta time.</param>
        /// <param name="newPosition">The calculated new position.</param>
        /// <returns>True if the progress was updated successfully.</returns>
        public static bool TryUpdateLinear(
            ref this NormalizedProgress progress,
            float3 start,
            float3 end,
            float speed,
            float deltaTime,
            out float3 newPosition)
        {
            if (progress.value >= 1.0f)
            {
                newPosition = end;
                return false;
            }

            LinearLogic.Solve(
                start,
                end,
                progress.value,
                speed,
                deltaTime,
                out var nextT,
                out newPosition);

            progress.value = nextT;
            return true;
        }

        /// <summary>
        /// Tries to update the linear rotation.
        /// </summary>
        /// <param name="start">The start rotation.</param>
        /// <param name="end">The end rotation.</param>
        /// <param name="t">The normalized time.</param>
        /// <param name="rotation">The calculated rotation.</param>
        /// <returns>True if the rotation was calculated.</returns>
        public static bool TryGetRotation(
            quaternion start,
            quaternion end,
            float t,
            out quaternion rotation)
        {
            LinearLogic.SolveRotation(start, end, t, out rotation);
            return true;
        }
    }
}