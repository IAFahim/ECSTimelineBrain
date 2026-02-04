// <copyright file="LinearMovementAuthoring.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace Movements.Movement.Authoring
{
    using System;
    using BovineLabs.Core.Authoring.EntityCommands;
    using Movements.Movement.Data;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    /// <summary>
    /// Authoring configuration for linear movement behavior.
    /// </summary>
    [Serializable]
    public class LinearMovementAuthoring
    {
        [SerializeField]
        private float3 startPosition;

        [SerializeField]
        private float3 endPosition;

        [SerializeField]
        private float speed = 1.0f;

        [SerializeField]
        [Range(0, 1)]
        private float progress;

        [SerializeField]
        private bool hasRotation;

        [SerializeField]
        private quaternion startRotation = quaternion.identity;

        [SerializeField]
        private quaternion endRotation = quaternion.identity;

        public float3 StartPosition
        {
            get => this.startPosition;
            set => this.startPosition = value;
        }

        public float3 EndPosition
        {
            get => this.endPosition;
            set => this.endPosition = value;
        }

        public float Speed
        {
            get => this.speed;
            set => this.speed = value;
        }

        public float Progress
        {
            get => this.progress;
            set => this.progress = value;
        }

        public bool HasRotation
        {
            get => this.hasRotation;
            set => this.hasRotation = value;
        }

        public quaternion StartRotation
        {
            get => this.startRotation;
            set => this.startRotation = value;
        }

        public quaternion EndRotation
        {
            get => this.endRotation;
            set => this.endRotation = value;
        }

        public void Bake(IBaker baker, Entity entity)
        {
            var builder = default(LinearMovementBuilder);
            
            builder.WithPosition(this.StartPosition, this.EndPosition);
            builder.WithSpeed(this.Speed);
            builder.WithProgress(this.Progress);

            if (this.HasRotation)
            {
                builder.WithRotation(this.StartRotation, this.EndRotation);
            }

            var commands = new BakerCommands(baker, entity);
            builder.ApplyTo(ref commands);
        }
    }

    /// <summary>
    /// Component for adding linear movement to an entity.
    /// </summary>
    [AddComponentMenu("Movements/Linear Movement")]
    public class LinearMovementAuthoringComponent : MonoBehaviour
    {
        [SerializeField]
        private LinearMovementAuthoring movement;

        private class LinearMovementBaker : Baker<LinearMovementAuthoringComponent>
        {
            public override void Bake(LinearMovementAuthoringComponent authoring)
            {
                var entity = this.GetEntity(TransformUsageFlags.Dynamic);
                authoring.movement.Bake(this, entity);
            }
        }
    }
}