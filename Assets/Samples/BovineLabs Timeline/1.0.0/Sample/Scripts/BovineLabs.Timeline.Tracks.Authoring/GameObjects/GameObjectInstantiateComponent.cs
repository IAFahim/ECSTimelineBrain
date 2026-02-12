using System;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    public struct GameObjectInstantiateComponent: IComponentData
    {
        public Entity prefab;
    }
}