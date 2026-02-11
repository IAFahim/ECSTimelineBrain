// <copyright file="PhysicsVelocityOffset.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data
{
    using Unity.Entities;
    using Unity.Physics;

    public struct PhysicsVelocityOffset : IComponentData
    {
        public PhysicsVelocity Value;
    }
}
