using System;
using Unity.Entities;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    public struct CompanionLinkInstantiateComponent : IComponentData
    {
        public Entity prefab;
    }
}