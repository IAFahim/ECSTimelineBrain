// <copyright file="PhysicsVelocityClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using BovineLabs.Timeline.Data;
using Unity.Entities;
using Unity.Physics;
using Unity.Properties;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct PhysicsVelocityClip : IComponentData
    {
        public PhysicsVelocity Value;
    }
}