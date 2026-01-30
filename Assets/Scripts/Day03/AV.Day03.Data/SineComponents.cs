using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AV.Day03.Data
{
    [Serializable]
    public struct SineMovement : IComponentData, IEnableableComponent
    {
        public float speed;
        public float amplitude;
        public float3 originalPosition;
        public float elapsedTime;
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
