using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ThirdPersonPlayer : IComponentData
{
    public Entity ControlledCharacter;
    public Entity ControlledCamera;
}

[Serializable]
public struct ThirdPersonPlayerInputs : IComponentData
{
    public float2 MoveInput;
    public FixedInputEvent JumpPressed;
}
