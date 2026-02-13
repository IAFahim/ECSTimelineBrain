// <copyright file="MyGraphSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample
{
    using BovineLabs.Grove.Sample.Data.Core;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;

    public partial struct MyGraphSystem : ISystem
    {
        private GraphImpl<MyContext> impl;

        /// <inheritdoc />
        public void OnCreate(ref SystemState state)
        {
            this.impl = new GraphImpl<MyContext>(ref state);
        }

        /// <inheritdoc />
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var execution = this.impl.GetExecution(ref state);

            var query = SystemAPI.QueryBuilder().WithAll<MyGraph>().Build();

            state.Dependency = new ExecuteAIJob
            {
                GraphExecution = execution,
                EntityHandle = SystemAPI.GetEntityTypeHandle(),
                MyGraphHandle = SystemAPI.GetComponentTypeHandle<MyGraph>(true),
            }.ScheduleParallel(query, state.Dependency);
        }

        [BurstCompile]
        private unsafe struct ExecuteAIJob : IJobChunk
        {
            public GraphExecution<MyContext> GraphExecution;

            [ReadOnly]
            public EntityTypeHandle EntityHandle;

            [ReadOnly]
            public ComponentTypeHandle<MyGraph> MyGraphHandle;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                this.GraphExecution.SetChunk(chunk, unfilteredChunkIndex);

                var entities = chunk.GetEntityDataPtrRO(this.EntityHandle);
                var graphs = (MyGraph*)chunk.GetRequiredComponentDataPtrRO(ref this.MyGraphHandle);

                for (var entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
                {
                    this.GraphExecution.Execute(entities[entityIndex], entityIndex, graphs[entityIndex].Graph);
                }
            }
        }
    }
}
