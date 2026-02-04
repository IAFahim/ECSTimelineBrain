using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Movements.Movement.Logic
{
    [BurstCompile]
    public static class LinearLogic
    {
        public const float MinDist = 0.001f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Solve(
            float3 start,
            float3 end,
            float currentT,
            float speed,
            float dt,
            out float newT,
            out float3 pos)
        {
            float dist = math.distance(start, end);
            
            // Calculate step. If dist is too small, snap to end (step = 1.0).
            // This avoids division by zero and handles "arrived" logic implicitly.
            float tStep = math.select((speed * dt) / dist, 1.0f, dist < MinDist);

            newT = math.saturate(currentT + tStep);
            pos = math.lerp(start, end, newT);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SolveRotation(
            quaternion start,
            quaternion end,
            float t,
            out quaternion rot)
        {
            rot = math.slerp(start, end, t);
        }
    }
}