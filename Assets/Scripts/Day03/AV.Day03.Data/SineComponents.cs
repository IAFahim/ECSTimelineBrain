using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AV.Day03.Data
{
    [Serializable]
    public struct SineMovement : IComponentData
    {
        public float Speed;
        public float Amplitude;
        public float3 OriginalPosition;
    }

    public struct SineSpawner : IComponentData
    {
        public UnityObjectRef<GameObject> Prefab;
    }

    public struct SineObjectRef : IComponentData
    {
        public UnityObjectRef<GameObject> Value;
    }
}
