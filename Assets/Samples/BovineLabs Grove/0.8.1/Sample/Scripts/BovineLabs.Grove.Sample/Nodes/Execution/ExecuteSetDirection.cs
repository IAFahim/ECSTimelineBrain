// <copyright file="SetDirection.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample.Nodes.Execution
{
    using BovineLabs.Grove.Core;
    using BovineLabs.Grove.Sample.Data.Core;
    using Unity.Mathematics;

    public static class ExecuteSetDirection
    {
        [ExecuteNode((int)ExecutionType.SetDirection)]
        public static void Execute(in EntityContext entityContext, ref MyContext context)
        {
            var state = context.GetState(entityContext.EntityIndexInChunk);
            ref var direction = ref state.GetOrAddRef((short)StateKeys.Direction, math.forward());
            direction = -direction;

            state.AddOrSet((short)StateKeys.LastDirectionChange, entityContext.ElapsedTime);
        }
    }
}
