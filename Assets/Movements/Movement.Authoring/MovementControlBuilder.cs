using BovineLabs.Core.EntityCommands;
using Movements.Movement.Data.Tags.Modifiers;

namespace Movements.Movement.Authoring
{
    /// <summary>
    /// Builder for configuring movement control behaviors, including Boomerang, PingPong, and Loop modes.
    /// </summary>
    public struct MovementControlBuilder
    {
        private bool hasBoomerang;
        private bool hasPingPong;
        private bool hasLoop;

        /// <summary>
        /// Enables boomerang mode (reverses direction at end).
        /// </summary>
        public void WithBoomerang()
        {
            hasBoomerang = true;
        }

        /// <summary>
        /// Enables ping-pong mode (oscillates back and forth).
        /// </summary>
        public void WithPingPong()
        {
            hasPingPong = true;
        }

        /// <summary>
        /// Enables loop mode (restarts from beginning at end).
        /// </summary>
        public void WithLoop()
        {
            hasLoop = true;
        }

        /// <summary>
        /// Applies the configured control behaviors to the specified entity builder.
        /// Boomerang and PingPong are added as disabled components (enableable).
        /// Loop is added as a regular component (always active).
        /// </summary>
        /// <typeparam name="T">The type of entity command builder.</typeparam>
        /// <param name="builder">The entity builder to apply control settings to.</param>
        public void ApplyTo<T>(ref T builder)
            where T : struct, IEntityCommands
        {
            if (hasBoomerang)
            {
                builder.AddComponent<BoomerangTag>();
                builder.SetComponentEnabled<BoomerangTag>(false);
            }

            if (hasPingPong)
            {
                builder.AddComponent<PingPongTag>();
                builder.SetComponentEnabled<PingPongTag>(false);
            }

            if (hasLoop)
            {
                builder.AddComponent<LoopTag>();
            }
        }
    }
}
