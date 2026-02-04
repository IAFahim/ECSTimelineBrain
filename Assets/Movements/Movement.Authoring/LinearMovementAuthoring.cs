namespace Movements.Movement.Authoring
{
    using System;
    using BovineLabs.Core.Authoring;
    using BovineLabs.Core.Authoring.EntityCommands;
    using Movements.Movement.Data;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    /// <summary>
    /// Authoring component for linear movement configuration in the Unity Editor.
    /// Supports using current GameObject transform as start position/rotation.
    /// </summary>
    [DisallowMultipleComponent]
    public class LinearMovementAuthoring : MonoBehaviour
    {
        [Header("Position Settings")]
        [Tooltip("Use the current GameObject position as the start position")]
        [SerializeField]
        private bool currentAsStartPosition = true;

        [Tooltip("Start position in world space (only used if 'Current As Start Position' is false)")]
        [SerializeField]
        private float3 startPosition;

        [Tooltip("End position in world space")]
        [SerializeField]
        private float3 endPosition = new(5f, 0f, 0f);

        [Header("Movement Settings")]
        [Tooltip("Movement speed in units per second")]
        [SerializeField]
        [Min(0f)]
        private float speed = 2f;

        [Tooltip("Initial progress (0-1)")]
        [SerializeField]
        [Range(0f, 1f)]
        private float progress;

        [Header("Rotation Settings")]
        [Tooltip("Enable rotation interpolation")]
        [SerializeField]
        private bool hasRotation;

        [Tooltip("Use the current GameObject rotation as the start rotation")]
        [SerializeField]
        private bool currentAsStartRotation = true;

        [Tooltip("Start rotation (only used if 'Current As Start Rotation' is false)")]
        [SerializeField]
        private quaternion startRotation = quaternion.identity;

        [Tooltip("End rotation")]
        [SerializeField]
        private quaternion endRotation = quaternion.identity;

        [Header("Visualization")]
        [Tooltip("Show movement path in Scene view")]
        [SerializeField]
        private bool showGizmos = true;

        [Tooltip("Color of the path visualization")]
        [SerializeField]
        private Color pathColor = Color.cyan;

        // Public properties for Builder pattern access
        public float3 StartPosition => currentAsStartPosition ? transform.position : startPosition;
        public float3 EndPosition => endPosition;
        public float Speed => speed;
        public float Progress => progress;
        public bool HasRotation => hasRotation;
        public quaternion StartRotation => currentAsStartRotation ? transform.rotation : startRotation;
        public quaternion EndRotation => endRotation;

        /// <summary>
        /// Baker that converts authoring data to ECS components.
        /// Uses BakerCommands to implement IEntityCommands pattern.
        /// </summary>
        private class Baker : Baker<LinearMovementAuthoring>
        {
            public override void Bake(LinearMovementAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Create IEntityCommands wrapper for the Baker
                var commands = new BakerCommands(this, entity);

                // Create builder with authoring data
                var builder = default(LinearMovementBuilder);
                builder.WithPositions(authoring.StartPosition, authoring.EndPosition);
                builder.WithSpeed(authoring.Speed);
                builder.WithProgress(authoring.Progress);

                if (authoring.HasRotation)
                {
                    builder.WithRotation(authoring.StartRotation, authoring.EndRotation);
                }

                // Apply to entity
                builder.ApplyTo(ref commands);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Scene view gizmo visualization for the movement path.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            var start = (Vector3)StartPosition;
            var end = (Vector3)EndPosition;

            // Draw movement path
            Gizmos.color = pathColor;
            Gizmos.DrawLine(start, end);

            // Draw start point
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(start, 0.2f);

            // Draw end point
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(end, 0.2f);

            // Draw direction arrow
            Gizmos.color = pathColor * 0.7f;
            var direction = (end - start).normalized;
            var arrowPos = Vector3.Lerp(start, end, 0.5f);
            Gizmos.DrawRay(arrowPos, direction * 0.3f);

            // Draw rotation indicators if enabled
            if (hasRotation)
            {
                var startRot = (Quaternion)StartRotation;
                var endRot = (Quaternion)EndRotation;

                UnityEditor.Handles.color = new Color(0, 1, 0, 0.5f);
                using (new UnityEditor.Handles.DrawingScope(Matrix4x4.TRS(start, startRot, Vector3.one)))
                {
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, 0.3f);
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.up, 0.3f);
                }

                UnityEditor.Handles.color = new Color(1, 0, 0, 0.5f);
                using (new UnityEditor.Handles.DrawingScope(Matrix4x4.TRS(end, endRot, Vector3.one)))
                {
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, 0.3f);
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.up, 0.3f);
                }
            }
        }

        /// <summary>
        /// Editor helper to initialize positions when component is added.
        /// </summary>
        private void Reset()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
            endPosition = transform.position + new Vector3(5f, 0f, 0f);
        }
#endif
    }
}
