// <copyright file="RotationResetOnDeactivate.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using Unity.Entities;
using Unity.Mathematics;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct RotationResetOnDeactivate : IComponentData
    {
        public quaternion Value;
    }
}