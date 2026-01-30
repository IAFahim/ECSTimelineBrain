using GameVariable.Intent;
using Intents.Intent.Data;
using Unity.Entities;
using UnityEngine;

namespace Intents.Intent.Authoring
{
    public class IntentComponentAuthoring : MonoBehaviour
    {
        public IntentState value;

        public class IntentComponentBaker : Baker<IntentComponentAuthoring>
        {
            public override void Bake(IntentComponentAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new IntentComponent { value = authoring.value });
            }
        }
    }
}