using AV.Day02.Data;
using BovineLabs.Core;
using BovineLabs.Core.States; // Assumed namespace based on asmdef
using Unity.Burst;
using Unity.Entities;

namespace AV.Day02
{
    // LAYER B: LOGIC (System Logic)

    [BurstCompile]
    public partial struct GameStateSystem : ISystem, ISystemStartStop
    {
        private StateModel _impl;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameState>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            _impl = new StateModel(ref state, ComponentType.ReadWrite<GameState>(), ComponentType.ReadWrite<GameStatePrevious>());
        }

        public void OnStopRunning(ref SystemState state)
        {
            _impl.Dispose(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            _impl.Run(ref state, ecb); // Process state changes
            ecb.Playback(state.EntityManager);
        }
    }

    [BurstCompile]
    public partial struct LobbyStateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Register LobbyState to be active when GameState.Value == GameStates.Lobby
            StateAPI.Register<GameState, LobbyState>(ref state, (byte)GameStates.Lobby);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Logic for Lobby
        }
    }

    [BurstCompile]
    public partial struct GameplayStateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Register GameplayState to be active when GameState.Value == GameStates.Gameplay
            StateAPI.Register<GameState, GameplayState>(ref state, (byte)GameStates.Gameplay);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Logic for Gameplay
        }
    }
}
