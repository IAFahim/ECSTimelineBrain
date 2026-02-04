using BovineLabs.Core.Authoring.EntityCommands;
using Movements.Movement.Data.Advanced.Targets;
using Movements.Movement.Data.Transforms.Unassigned;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
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

        [Tooltip("Use a target Transform for the end position")] [SerializeField]
        private bool useTargetTransform;

        [Tooltip("End position in world space (only used if Target Transform is not set)")] [SerializeField]
        private float3 endPosition = new(5f, 0f, 0f);

        [Header("Movement Settings")] [Tooltip("Movement speed in units per second")] [SerializeField] [Min(0f)]
        private float speed = 2f;

        [Tooltip("Initial progress (0-1)")] [SerializeField] [Range(0f, 1f)]
        private float progress;

        [Header("Rotation Settings")] [Tooltip("Enable rotation interpolation")] [SerializeField]
        private bool hasRotation;

        [Tooltip("Use the current GameObject rotation as the start rotation")] [SerializeField]
        private bool currentAsStartRotation = true;

        [Tooltip("Start rotation Euler angles (only used if 'Current As Start Rotation' is false)")] [SerializeField]
        private Vector3 startRotationEuler;

        [Tooltip("End rotation Euler angles")] [SerializeField]
        private Vector3 endRotationEuler = new(0f, 180f, 0f);

        [Header("Visualization")] [Tooltip("Show movement path in Scene view")] [SerializeField]
        private bool showGizmos = true;

        [Tooltip("Color of the path visualization")] [SerializeField]
        private Color pathColor = Color.cyan;

        public float3 StartPosition => currentAsStartPosition ? transform.position : startPosition;
        public float3 EndPosition => endPosition;
        public float Speed => speed;
        public float Progress => progress;
        public bool HasRotation => hasRotation;

        public quaternion StartRotation => currentAsStartRotation
            ? transform.rotation
            : quaternion.Euler(math.radians(startRotationEuler));

        public quaternion EndRotation => quaternion.Euler(math.radians(endRotationEuler));

        /// <summary>
        ///     Baker that converts authoring data to ECS components.
        ///     Uses BakerCommands to implement IEntityCommands pattern.
        /// </summary>
        private class Baker : Baker<LinearMovementAuthoring>
        {
            public override void Bake(LinearMovementAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var commands = new BakerCommands(this, entity);

                var builder = default(LinearMovementBuilder);
                builder.WithPositions(authoring.StartPosition, authoring.EndPosition);
                builder.WithSpeed(authoring.Speed);
                builder.WithProgress(authoring.Progress);

                if (authoring.HasRotation) builder.WithRotation(authoring.StartRotation, authoring.EndRotation);

                builder.ApplyTo(ref commands);

                if (authoring.useTargetTransform)
                {
                    AddComponent(entity, new TargetTransformComponent());
                    SetComponentEnabled<TargetTransformComponent>(entity, false);

                    AddComponent(entity, new UnAssignedPositionComponent());

                    if (authoring.HasRotation) AddComponent(entity, new UnAssignedQuaternionComponent());
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Scene view gizmo visualization for the movement path.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            if (useTargetTransform) return;

            var start = (Vector3)StartPosition;
            var end = (Vector3)EndPosition;

            Gizmos.color = pathColor;
            Gizmos.DrawLine(start, end);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(start, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(end, 0.2f);

            Gizmos.color = pathColor * 0.7f;
            var direction = (end - start).normalized;
            var arrowPos = Vector3.Lerp(start, end, 0.5f);
            Gizmos.DrawRay(arrowPos, direction * 0.3f);

            if (hasRotation)
            {
                var startRot = (Quaternion)StartRotation;
                var endRot = (Quaternion)EndRotation;

                Handles.color = new Color(0, 1, 0, 0.5f);
                using (new Handles.DrawingScope(Matrix4x4.TRS(start, startRot, Vector3.one)))
                {
                    Handles.DrawWireDisc(Vector3.zero, Vector3.forward, 0.3f);
                    Handles.DrawWireDisc(Vector3.zero, Vector3.up, 0.3f);
                    Handles.ArrowHandleCap(0, Vector3.zero, Quaternion.LookRotation(Vector3.forward), 0.2f,
                        EventType.Repaint);
                }

                Handles.color = new Color(1, 0, 0, 0.5f);
                using (new Handles.DrawingScope(Matrix4x4.TRS(end, endRot, Vector3.one)))
                {
                    Handles.DrawWireDisc(Vector3.zero, Vector3.forward, 0.3f);
                    Handles.DrawWireDisc(Vector3.zero, Vector3.up, 0.3f);
                    Handles.ArrowHandleCap(0, Vector3.zero, Quaternion.LookRotation(Vector3.forward), 0.2f,
                        EventType.Repaint);
                }
            }
        }

        /// <summary>
        ///     Editor helper to initialize positions when component is added.
        /// </summary>
        private void Reset()
        {
            startPosition = transform.position;
            startRotationEuler = transform.rotation.eulerAngles;
            endPosition = transform.position + new Vector3(5f, 0f, 0f);
        }
#endif
    }
}