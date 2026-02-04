using System.Runtime.CompilerServices;
using BovineLabs.Core;
using BovineLabs.Core.Assertions;
using Movements.Movement.Data.Parameters.Motion;
using Movements.Movement.Data.Parameters.Timing;
using Movements.Movement.Data.Transforms.StartEnd;
using Movements.Movement.Logic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Movements.Movement.Data.Advanced.Facets
{
    /// <summary>
    ///     Base Facet for linear movement - position only.
    ///     Use this when rotation interpolation is not needed.
    /// </summary>
    public partial struct LinearMovementFacet : IFacet
    {
        public readonly Entity Entity;
        public readonly RefRW<LocalTransform> Transform;
        public readonly RefRW<NormalizedProgress> Progress;
        public readonly RefRO<StartPositionComponent> StartPos;
        public readonly RefRO<EndPositionComponent> EndPos;
        public readonly RefRO<SpeedComponent> Speed;
        public readonly RefRO<RangeComponent> Range;
        [Singleton] public readonly TimeData Time;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute()
        {
            Check.Assume(Speed.ValueRO.value >= 0, "Speed must be non-negative");

            // Calculate Effective End Position based on Range
            // EffectiveEnd = Start + (End - Start) * Range
            var rawDir = EndPos.ValueRO.value - StartPos.ValueRO.value;
            var effectiveEnd = StartPos.ValueRO.value + (rawDir * Range.ValueRO.value);

            var dt = Time.DeltaTime;
            var dist = math.distance(StartPos.ValueRO.value, effectiveEnd);

            var tStep = math.select(
                Speed.ValueRO.value * dt / dist,
                1.0f,
                dist < LinearLogic.MinDist
            );

            var newProgress = math.saturate(Progress.ValueRO.value + tStep);
            Progress.ValueRW.value = newProgress;

            // Interpolate to EffectiveEnd
            Transform.ValueRW.Position = math.lerp(
                StartPos.ValueRO.value,
                effectiveEnd,
                newProgress
            );
        }
    }

    /// <summary>
    ///     Facet for linear movement with rotation.
    ///     Use this when both position and rotation interpolation are needed.
    /// </summary>
    public partial struct LinearMovementWithRotationFacet : IFacet
    {
        public readonly Entity Entity;
        public readonly RefRW<LocalTransform> Transform;
        public readonly RefRW<NormalizedProgress> Progress;
        public readonly RefRO<StartPositionComponent> StartPos;
        public readonly RefRO<EndPositionComponent> EndPos;
        public readonly RefRO<SpeedComponent> Speed;
        public readonly RefRO<RangeComponent> Range;
        public readonly RefRO<StartQuaternionComponent> StartRot;
        public readonly RefRO<EndQuaternionComponent> EndRot;
        [Singleton] public readonly TimeData Time;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute()
        {
            Check.Assume(Speed.ValueRO.value >= 0, "Speed must be non-negative");

            // Calculate Effective End Position based on Range
            // EffectiveEnd = Start + (End - Start) * Range
            var rawDir = EndPos.ValueRO.value - StartPos.ValueRO.value;
            var effectiveEnd = StartPos.ValueRO.value + (rawDir * Range.ValueRO.value);

            var dt = Time.DeltaTime;
            var dist = math.distance(StartPos.ValueRO.value, effectiveEnd);

            var tStep = math.select(
                Speed.ValueRO.value * dt / dist,
                1.0f,
                dist < LinearLogic.MinDist
            );

            var newProgress = math.saturate(Progress.ValueRO.value + tStep);
            Progress.ValueRW.value = newProgress;

            // Interpolate Position to EffectiveEnd
            Transform.ValueRW.Position = math.lerp(
                StartPos.ValueRO.value,
                effectiveEnd,
                newProgress
            );

            // Interpolate Rotation (Rotation completes exactly when object reaches effectiveEnd)
            Transform.ValueRW.Rotation = math.slerp(
                StartRot.ValueRO.value,
                EndRot.ValueRO.value,
                newProgress
            );
        }
    }
}