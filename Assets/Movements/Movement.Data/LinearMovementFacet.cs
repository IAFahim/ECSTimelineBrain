using System.Runtime.CompilerServices;
using BovineLabs.Core;
using BovineLabs.Core.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Movements.Movement.Data
{
    /// <summary>
    /// Base Facet for linear movement - position only.
    /// Use this when rotation interpolation is not needed.
    /// </summary>
    public partial struct LinearMovementFacet : IFacet
    {
        public readonly Entity Entity;
        public readonly RefRW<LocalTransform> Transform;
        public readonly RefRW<NormalizedProgress> Progress;
        public readonly RefRO<StartPositionComponent> StartPos;
        public readonly RefRO<EndPositionComponent> EndPos;
        public readonly RefRO<SpeedComponent> Speed;
        [Singleton] public readonly TimeData Time;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute()
        {
            Check.Assume(Speed.ValueRO.value >= 0, "Speed must be non-negative");

            var dt = Time.DeltaTime;
            float dist = math.distance(StartPos.ValueRO.value, EndPos.ValueRO.value);

            float tStep = math.select(
                (Speed.ValueRO.value * dt) / dist,
                1.0f,
                dist < Logic.LinearLogic.MinDist
            );

            float newProgress = math.saturate(Progress.ValueRO.value + tStep);
            Progress.ValueRW.value = newProgress;

            Transform.ValueRW.Position = math.lerp(
                StartPos.ValueRO.value,
                EndPos.ValueRO.value,
                newProgress
            );
        }
    }

    /// <summary>
    /// Facet for linear movement with rotation.
    /// Use this when both position and rotation interpolation are needed.
    /// </summary>
    public partial struct LinearMovementWithRotationFacet : IFacet
    {
        public readonly Entity Entity;
        public readonly RefRW<LocalTransform> Transform;
        public readonly RefRW<NormalizedProgress> Progress;
        public readonly RefRO<StartPositionComponent> StartPos;
        public readonly RefRO<EndPositionComponent> EndPos;
        public readonly RefRO<SpeedComponent> Speed;
        public readonly RefRO<StartQuaternionComponent> StartRot;
        public readonly RefRO<EndQuaternionComponent> EndRot;
        [Singleton] public readonly TimeData Time;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute()
        {
            Check.Assume(Speed.ValueRO.value >= 0, "Speed must be non-negative");

            var dt = Time.DeltaTime;
            float dist = math.distance(StartPos.ValueRO.value, EndPos.ValueRO.value);

            float tStep = math.select(
                (Speed.ValueRO.value * dt) / dist,
                1.0f,
                dist < Logic.LinearLogic.MinDist
            );

            float newProgress = math.saturate(Progress.ValueRO.value + tStep);
            Progress.ValueRW.value = newProgress;

            Transform.ValueRW.Position = math.lerp(
                StartPos.ValueRO.value,
                EndPos.ValueRO.value,
                newProgress
            );

            Transform.ValueRW.Rotation = math.slerp(
                StartRot.ValueRO.value,
                EndRot.ValueRO.value,
                newProgress
            );
        }
    }
}
