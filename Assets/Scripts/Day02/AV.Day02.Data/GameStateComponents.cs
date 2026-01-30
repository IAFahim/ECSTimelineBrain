using System;
using System.Runtime.InteropServices;
using Unity.Entities;

namespace AV.Day02.Data
{
    // LAYER A: DATA

    public enum GameStates : byte
    {
        None = 0,
        Lobby = 1,
        Gameplay = 2,
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct GameState : IComponentData
    {
        public byte Value;

        public override string ToString() => $"[GameState] {(GameStates)Value}";
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct GameStatePrevious : IComponentData
    {
        public byte Value;

        public override string ToString() => $"[GameStatePrev] {(GameStates)Value}";
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct LobbyState : IComponentData, IEnableableComponent
    {
        // Tag component for Lobby state
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct GameplayState : IComponentData, IEnableableComponent
    {
        // Tag component for Gameplay state
    }
}
