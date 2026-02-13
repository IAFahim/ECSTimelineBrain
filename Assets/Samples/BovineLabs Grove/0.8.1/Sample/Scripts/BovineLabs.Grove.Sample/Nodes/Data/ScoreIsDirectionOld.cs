// <copyright file="DirectionOld.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample.Nodes.Data
{
    using BovineLabs.Grove.Core;
    using BovineLabs.Grove.Sample.Data.Core;
    using BovineLabs.Grove.Sample.Data.Nodes.Data;

    public static class ScoreIsDirectionOld
    {
        [DataNode((int)DataType.IsDirectionOld)]
        public static float Calculate(in ScoreIsDirectionOldData data, in EntityContext entityContext, ref MyContext context)
        {
            var state = context.GetState(entityContext.EntityIndexInChunk);
            ref var lastDirectionChange = ref state.GetOrAddRef((short)StateKeys.LastDirectionChange, double.MinValue);
            var isOutOfData = lastDirectionChange + data.Duration <= entityContext.ElapsedTime;
            return data.Score.Score(isOutOfData);
        }
    }
}
