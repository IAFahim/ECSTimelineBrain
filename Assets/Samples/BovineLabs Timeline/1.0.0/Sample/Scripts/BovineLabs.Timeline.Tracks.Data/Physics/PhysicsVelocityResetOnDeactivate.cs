// <copyright file="PhysicsVelocityResetOnDeactivate.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data
{
    using Unity.Entities;
    using Unity.Physics;

    public struct PhysicsVelocityResetOnDeactivate : IComponentData
    {
        public PhysicsVelocity Value;
    }
}
