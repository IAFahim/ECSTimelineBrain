// <copyright file="RotationAnimated.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using BovineLabs.Timeline.Data;
using Unity.Mathematics;
using Unity.Properties;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct RotationAnimated : IAnimatedComponent<quaternion>
    {
        [CreateProperty] public quaternion Value { get; set; }
    }
}