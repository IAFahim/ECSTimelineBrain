// <copyright file="PhysicsVelocityAnimated.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using BovineLabs.Timeline.Data;
using Unity.Physics;
using Unity.Properties;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct PhysicsVelocityAnimated : IAnimatedComponent<PhysicsVelocity>
    {
        [CreateProperty] public PhysicsVelocity Value { get; set; }
    }
}