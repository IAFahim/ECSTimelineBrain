using BovineLabs.Core.Authoring.EntityCommands;
using Movements.Movement.Data.Builders;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Movements.Movement.Authoring
{
    /// <summary>
    ///     Authoring component for linear movement configuration in the Unity Editor.
    ///     Supports using current GameObject transform as start position/rotation.
    /// </summary>
    [DisallowMultipleComponent]
    public class LinearMovementAuthoring : MonoBehaviour
    {
        [Header("Position Settings")]
        [Tooltip("Use the current GameObject position as the start position")]
        [SerializeField]
        private bool currentAsStartPosition = true;

        [Tooltip("Start position in world space (only used if 'Current As Start Position' is false)")] [SerializeField]
        private float3 startPosition;

        [Tooltip("End position in world space (only used if Target Transform is not set)")] [SerializeField]
        private float3 endPosition = new(5f, 0f, 0f);
        
        [Header("Rotation Settings")] [Tooltip("Enable rotation interpolation")] [SerializeField]
        private bool hasRotation;

        [Tooltip("Use the current GameObject rotation as the start rotation")] [SerializeField]
        private bool currentAsStartRotation = true;

        [Tooltip("Start rotation Euler angles (only used if 'Current As Start Rotation' is false)")] [SerializeField]
        private Vector3 startRotationEuler;

        [Tooltip("End rotation Euler angles")] [SerializeField]
        private Vector3 endRotationEuler = new(0f, 180f, 0f);

        [Header("Movement Settings")] 
        
        [Tooltip("Movement speed in units per second")] [SerializeField] [Min(0f)]
        private float speed = 2f;
        
        [Tooltip("Scales the movement vector. 1 = reach end position, 0.5 = half way, 2 = overshoot.")] [SerializeField]
        private float range = 1f;

        [Tooltip("Initial progress (0-1)")] [SerializeField] [Range(0f, 1f)]
        private float progress;

        private float3 StartPosition => currentAsStartPosition ? transform.position : startPosition;
        private float3 EndPosition => endPosition;
        private float Speed => speed;
        private float Progress => progress;
        private float Range => range;
        private bool HasRotation => hasRotation;

        private quaternion StartRotation => currentAsStartRotation ? transform.rotation
            : quaternion.Euler(math.radians(startRotationEuler));

        private quaternion EndRotation => quaternion.Euler(math.radians(endRotationEuler));
        
        private class Baker : Baker<LinearMovementAuthoring>
        {
            public override void Bake(LinearMovementAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var commands = new BakerCommands(this, entity);

                var builder = default(MovementBuilder);
                builder.WithPositions(authoring.StartPosition, authoring.EndPosition);
                builder.WithSpeed(authoring.Speed);
                builder.WithProgress(authoring.Progress);
                builder.WithRange(authoring.Range);

                if (authoring.HasRotation) builder.WithRotation(authoring.StartRotation, authoring.EndRotation);

                builder.ApplyTo(ref commands);
            }
        }
    }
}