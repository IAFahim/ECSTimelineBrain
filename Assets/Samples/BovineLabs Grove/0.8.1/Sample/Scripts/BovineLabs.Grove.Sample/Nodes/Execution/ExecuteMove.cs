// <copyright file="MoveNode.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample.Nodes.Execution
{
    using BovineLabs.Grove.Core;
    using BovineLabs.Grove.Sample.Data.Core;
    using BovineLabs.Grove.Sample.Data.Nodes.Execution;
    using Unity.Mathematics;

    public static class ExecuteMove
    {
        [ExecuteNode((int)ExecutionType.Move)]
        public static void Execute(in ExecuteMoveData data, in EntityContext entityContext, ref MyContext context)
        {
            var state = context.GetState(entityContext.EntityIndexInChunk);
            if (!state.TryGetValue((short)StateKeys.Direction, out float3 direction))
            {
                return;
            }

            ref var lt = ref context.LocalTransform.GetRW(entityContext.EntityIndexInChunk).ValueRW;
            lt.Position += direction * data.Speed * entityContext.DeltaTime;
        }
    }
}
