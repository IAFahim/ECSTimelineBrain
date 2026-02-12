// <copyright file="PhysicsVelocityClip.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Timeline.Authoring
{
    using BovineLabs.Timeline.Tracks.Data;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Physics;
    using UnityEngine;
    using UnityEngine.Timeline;
    [TrackClipType(typeof(PhysicsVelocityClip))]
    public class PhysicsVelocityClip : DOTSClip, ITimelineClipAsset
    {

        [SerializeField]
        [Tooltip("Linear velocity in world units per second")]
        private Vector3 linearVelocity = Vector3.forward;

        [SerializeField]
        [Tooltip("Angular velocity in radians per second")]
        private Vector3 angularVelocity;

        public float3 LinearVelocity => linearVelocity;
        public float3 AngularVelocity => angularVelocity;

        public ClipCaps clipCaps => ClipCaps.Looping;

        public override double duration => 1;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new PhysicsVelocityAnimated
            {
                Value = new PhysicsVelocity
                {
                    Linear = LinearVelocity,
                    Angular = AngularVelocity
                }
            });

            if (context.Binding != null)
                context.Baker.AddTransformUsageFlags(context.Binding.Target, TransformUsageFlags.Dynamic);

            base.Bake(clipEntity, context);
        }
    }
}