using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Movements.Movement.Data.Utilities
{
    [Serializable]
    public struct PidBlob : IComponentData
    {
        public BlobAssetReference<PidGains> Gains;
    }

    [Serializable]
    public struct PidBlobGain3 : IComponentData
    {
        public BlobAssetReference<PidGains3> Gains;
    }

    [Serializable]
    public struct PidGains : IComponentData
    {
        public float proportional;
        public float integral;
        public float derivative;
    }

    [Serializable]
    public struct PidError : IComponentData
    {
        public float integral;
        public float lastError;
    }

    [Serializable]
    public struct PidComponent3 : IComponentData
    {
    }

    [Serializable]
    public struct PidGains3 : IComponentData
    {
        public float3 proportional;
        public float3 integral;
        public float3 derivative;
    }

    [Serializable]
    public struct PidError3 : IComponentData
    {
        public float3 integral;
        public float3 lastError;
    }
}