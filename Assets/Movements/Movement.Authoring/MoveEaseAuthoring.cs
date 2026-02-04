using AV.Eases.Runtime;
using BovineLabs.Core.Authoring;
using Movements.Movement.Data.Parameters.Easing;
using Unity.Entities;
using UnityEngine;

namespace Movements.Movement.Authoring
{
    /// <summary>
    /// Authoring component for movement-based easing.
    /// Applies easing to the position/rotation interpolation during movement.
    /// </summary>
    [DisallowMultipleComponent]
    public class MoveEaseAuthoring : MonoBehaviour
    {
        [Tooltip("Easing configuration for movement interpolation")]
        [SerializeField]
        private EEase ease;

        public EaseConfig EaseConfig => new(){Value = (byte)ease};

        /// <summary>
        /// Baker that converts authoring data to ECS components.
        /// </summary>
        private class Baker : Baker<MoveEaseAuthoring>
        {
            public override void Bake(MoveEaseAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MoveEaseComponent
                {
                    value = authoring.EaseConfig
                });
            }
        }
    }
}
