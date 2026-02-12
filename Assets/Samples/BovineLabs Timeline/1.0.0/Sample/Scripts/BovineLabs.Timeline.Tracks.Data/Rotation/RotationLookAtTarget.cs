// <copyright file="LookAtTarget.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using Unity.Entities;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct RotationLookAtTarget : IComponentData
    {
        public Entity Target;
    }
}