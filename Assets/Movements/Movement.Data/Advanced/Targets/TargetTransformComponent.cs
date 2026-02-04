using System;
using Unity.Entities;
using UnityEngine;

namespace Movements.Movement.Data.Advanced.Targets
{
    /// <summary>
    ///     Component that holds a reference to a target Transform.
    ///     Uses IEnableableComponent to indicate when the target has been instantiated and is valid.
    ///     Usage:
    ///     - During baking: Add with UnityObjectRef
    ///     <Transform>
    ///         pointing to a prefab or scene reference
    ///         - At runtime: Enable this component when the target is instantiated and ready
    ///         - Systems: Only processes entities where this component is ENABLED
    /// </summary>
    [Serializable]
    public struct TargetTransformComponent : IComponentData, IEnableableComponent
    {
        public UnityObjectRef<Transform> Value;
    }
}