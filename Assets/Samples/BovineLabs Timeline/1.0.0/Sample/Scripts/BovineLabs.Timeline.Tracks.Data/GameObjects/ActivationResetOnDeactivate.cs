// <copyright file="ActivationResetOnDeactivate.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Tracks.Data.GameObjects
{
    using Unity.Entities;

    /// <summary>
    /// Component used to store the initial state of an entity's disabled status
    /// so it can be restored when the timeline finishes.
    /// </summary>
    public struct ActivationResetOnDeactivate : IComponentData
    {
        /// <summary>
        /// True if the entity was disabled (had the Disabled component) before the timeline started.
        /// </summary>
        public bool WasDisabled;
    }
    
}