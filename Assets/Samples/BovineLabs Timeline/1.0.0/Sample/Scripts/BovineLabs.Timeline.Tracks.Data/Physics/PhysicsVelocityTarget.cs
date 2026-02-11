// <copyright file="PhysicsVelocityTarget.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data
{
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Physics;

    public struct PhysicsVelocityTarget : IComponentData
    {
        public Entity Target;
        public float3 LinearOffset;
        public float3 AngularOffset;
    }
}