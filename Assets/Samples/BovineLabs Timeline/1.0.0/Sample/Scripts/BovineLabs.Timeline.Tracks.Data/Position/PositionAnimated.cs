// <copyright file="PositionAnimated.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data
{
    using BovineLabs.Core.Collections;
    using BovineLabs.Timeline.Data;
    using Unity.Mathematics;
    using Unity.Properties;

    public struct PositionAnimated : IAnimatedComponent<float3>
    {
        [CreateProperty]
        public float3 Value { get; set; }
    }
}
