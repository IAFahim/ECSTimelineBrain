using AV.Day03.Data;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AV.Day03
{
    public partial struct SineSpawnerSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (spawner, entity) in SystemAPI.Query<RefRO<SineSpawner>>().WithNone<SineObjectRef>().WithEntityAccess())
            {
                if (spawner.ValueRO.Prefab.Value != null)
                {
                    var instance = Object.Instantiate(spawner.ValueRO.Prefab.Value);
                    
                    // Add the reference component so we can move it later
                    // And mark it as spawned (SineObjectRef acts as a tag/ref holder)
                    ecb.AddComponent(entity, new SineObjectRef
                    {
                        Value = instance
                    });

                    // Set initial position in the movement component
                    // We need to defer this or set it here? 
                    // Since we can't write to RefRO, we rely on the MoveSystem to pick it up, 
                    // or we could use RefRW if we wanted to write OriginalPosition here.
                    // But wait, accessing SineMovement via SystemAPI.Query isn't thread safe if we use ECB? 
                    // No, main thread is fine.
                    
                    // Let's just initialize the position in the move system if it's zero? 
                    // Or let the move system capture the initial pos.
                }
            }
        }
    }

    public partial struct SineMoveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var time = (float)SystemAPI.Time.ElapsedTime;

            // Iterate over entities that have a UnityObjectRef and SineMovement
            foreach (var (objRef, sine) in SystemAPI.Query<RefRO<SineObjectRef>, RefRW<SineMovement>>())
            {
                if (objRef.ValueRO.Value != null)
                {
                    var transform = objRef.ValueRO.Value.Value.transform;

                    // Initialize OriginalPosition if it hasn't been set (simple check)
                    // Note: This is a bit hacky, normally we'd do this on spawn. 
                    // But for this simple example it works.
                    if (math.lengthsq(sine.ValueRO.OriginalPosition) < 0.001f)
                    {
                        sine.ValueRW.OriginalPosition = transform.position;
                    }

                    var offset = math.sin(time * sine.ValueRO.Speed) * sine.ValueRO.Amplitude;
                    transform.position = sine.ValueRO.OriginalPosition + new float3(0, offset, 0);
                }
            }
        }
    }
}
