using System;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    public struct CompanionLinkInstantiateComponent: IComponentData
    {
        public Entity prefab;
    }
}