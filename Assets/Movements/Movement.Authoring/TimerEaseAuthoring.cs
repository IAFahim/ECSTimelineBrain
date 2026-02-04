using AV.Eases.Runtime;
using BovineLabs.Core.Authoring;
using Movements.Movement.Data.Parameters.Easing;
using Unity.Entities;
using UnityEngine;

namespace Movements.Movement.Authoring
{
    /// <summary>
    /// Authoring component for time-based easing on movement progression.
    /// Applies easing to the normalized time (0-1) of movement.
    /// </summary>
    [DisallowMultipleComponent]
    public class TimerEaseAuthoring : MonoBehaviour
    {
        [Tooltip("Easing configuration for movement time progression")] [SerializeField]
        private EEase ease;

        private EaseConfig EaseConfig => new() { Value = (byte)ease };
        
        private class Baker : Baker<TimerEaseAuthoring>
        {
            public override void Bake(TimerEaseAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TimerEaseComponent
                {
                    value = authoring.EaseConfig
                });
            }
        }
    }
}