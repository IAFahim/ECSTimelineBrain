// <copyright file="PhysicsVelocityMixer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks
{
    using BovineLabs.Timeline;
    using Unity.Burst;
    using Unity.Mathematics;
    using Unity.Physics;

    [BurstCompile]

    public readonly struct PhysicsVelocityMixer : IMixer<PhysicsVelocity>
    {
        [BurstCompile]
        public PhysicsVelocity Lerp(in PhysicsVelocity a, in PhysicsVelocity b, in float s)
        {
            return new PhysicsVelocity
            {
                Linear = math.lerp(a.Linear, b.Linear, s),
                Angular = math.lerp(a.Angular, b.Angular, s)
            };
        }

        [BurstCompile]
        public PhysicsVelocity Add(in PhysicsVelocity a, in PhysicsVelocity b)
        {
            return new PhysicsVelocity
            {
                Linear = a.Linear + b.Linear,
                Angular = a.Angular + b.Angular
            };
        }
    }
}
