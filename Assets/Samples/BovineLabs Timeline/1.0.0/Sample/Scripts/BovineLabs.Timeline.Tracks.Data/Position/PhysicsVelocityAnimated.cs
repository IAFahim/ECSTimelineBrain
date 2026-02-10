// <copyright file="PhysicsVelocityAnimated.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data
{
    using BovineLabs.Core.Collections;
    using BovineLabs.Timeline.Data;
    using Unity.Physics;
    using Unity.Properties;

    public struct PhysicsVelocityAnimated : IAnimatedComponent<PhysicsVelocity>
    {
        [CreateProperty]
        public PhysicsVelocity Value { get; set; }
    }
}
