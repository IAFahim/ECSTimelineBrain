using AV.Day03.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AV.Day03
{

    [WithAny(typeof(SineObjectRef))]
    public partial struct SineMoveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (objRef, sine) in SystemAPI.Query<RefRO<SineObjectRef>, RefRW<SineMovement>>())
            {
                var transform = objRef.ValueRO.Value.Value.transform;
                sine.ValueRW.elapsedTime += deltaTime;
                var offset = math.sin(sine.ValueRO.elapsedTime * sine.ValueRO.speed) * sine.ValueRO.amplitude;
                transform.position = sine.ValueRO.originalPosition + new float3(0, offset, 0);
            }
        }
    }
}