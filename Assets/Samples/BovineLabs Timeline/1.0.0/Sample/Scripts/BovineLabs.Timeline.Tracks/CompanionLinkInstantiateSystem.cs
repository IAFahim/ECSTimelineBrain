using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace BovineLabs.Timeline.Tracks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct CompanionLinkInstantiateSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            // FIX: Do NOT use 'using' here. The System owns the query.
            var query = state.GetEntityQuery(ComponentType.ReadOnly<CompanionInstance>());
            
            // We only dispose the NativeArray, not the Query.
            var instances = query.ToComponentDataArray<CompanionInstance>(Allocator.Temp);

            // Force destroy all lingering instances immediately
            for (int i = 0; i < instances.Length; i++)
            {
                var instanceEntity = instances[i].Value;
                if (state.EntityManager.Exists(instanceEntity))
                {
                    state.EntityManager.DestroyEntity(instanceEntity);
                }
            }
            
            instances.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // -----------------------------------------------------------------------
            // 1. SPAWN (Start of Clip)
            // -----------------------------------------------------------------------
            foreach (var (clipData, entity) in SystemAPI.Query<RefRO<CompanionLinkInstantiateComponent>>()
                         .WithAll<TimelineActive>()
                         .WithNone<CompanionInstance>() 
                         .WithEntityAccess())
            {
                // Instantiate
                var instance = ecb.Instantiate(clipData.ValueRO.prefab);

                // Enable (Remove Disabled tag if prefab was disabled)
                ecb.RemoveComponent<Disabled>(instance);

                // Add Cleanup Tracking
                ecb.AddComponent(entity, new CompanionInstance { Value = instance });
            }

            // -----------------------------------------------------------------------
            // 2. DESPAWN (End of Clip)
            // -----------------------------------------------------------------------
            // Use a NativeList to gather entities to modify, so we don't invalidate the query while iterating
            var entitiesToCleanup = new NativeList<Entity>(Allocator.Temp);
            var instancesToDestroy = new NativeList<Entity>(Allocator.Temp);

            // Case A: Timeline Cursor left the clip (TimelineActive removed)
            foreach (var (instanceRef, entity) in SystemAPI.Query<RefRO<CompanionInstance>>()
                         .WithNone<TimelineActive>()
                         .WithEntityAccess())
            {
                instancesToDestroy.Add(instanceRef.ValueRO.Value);
                entitiesToCleanup.Add(entity);
            }

            // Case B: The Clip Entity was deleted entirely (Data component removed)
            foreach (var (instanceRef, entity) in SystemAPI.Query<RefRO<CompanionInstance>>()
                         .WithNone<CompanionLinkInstantiateComponent>()
                         .WithEntityAccess())
            {
                instancesToDestroy.Add(instanceRef.ValueRO.Value);
                entitiesToCleanup.Add(entity);
            }

            // Execute Logic
            foreach (var e in instancesToDestroy)
            {
                // We use ECB here for runtime safety
                ecb.DestroyEntity(e);
            }

            foreach (var e in entitiesToCleanup)
            {
                // Remove the tracking component so the Clip Entity can finally be destroyed
                ecb.RemoveComponent<CompanionInstance>(e);
            }

            entitiesToCleanup.Dispose();
            instancesToDestroy.Dispose();
        }
    }
}