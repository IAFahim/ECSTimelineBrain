using BovineLabs.Core.Authoring.EntityCommands;
using Unity.Entities;
using UnityEngine;

namespace Movements.Movement.Authoring
{
    /// <summary>
    /// Authoring component for movement control behaviors.
    /// Supports Boomerang, PingPong, and Loop modes.
    /// </summary>
    [DisallowMultipleComponent]
    public class MovementControlAuthoring : MonoBehaviour
    {
        [Tooltip("Enable boomerang mode - reverses direction when reaching end position")]
        [SerializeField]
        private bool enableBoomerang = false;

        [Tooltip("Enable ping-pong mode - oscillates back and forth between start and end")]
        [SerializeField]
        private bool enablePingPong = false;

        [Tooltip("Enable loop mode - restarts from beginning when reaching end position")]
        [SerializeField]
        private bool enableLoop = false;

        public bool EnableBoomerang => enableBoomerang;
        public bool EnablePingPong => enablePingPong;
        public bool EnableLoop => enableLoop;

        /// <summary>
        /// Baker that converts authoring data to ECS components.
        /// Uses BakerCommands to implement IEntityCommands pattern.
        /// </summary>
        private class Baker : Baker<MovementControlAuthoring>
        {
            public override void Bake(MovementControlAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var commands = new BakerCommands(this, entity);

                var builder = new MovementControlBuilder();
                if (authoring.EnableBoomerang) builder.WithBoomerang();
                if (authoring.EnablePingPong) builder.WithPingPong();
                if (authoring.EnableLoop) builder.WithLoop();

                builder.ApplyTo(ref commands);
            }
        }
    }
}
