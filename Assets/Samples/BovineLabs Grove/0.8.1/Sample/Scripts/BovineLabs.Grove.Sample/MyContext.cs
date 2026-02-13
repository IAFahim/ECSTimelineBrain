// <copyright file="MyContext.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample
{
    using BovineLabs.Core.Iterators;
    using BovineLabs.Grove.Core;
    using BovineLabs.Grove.Sample.Data.Core;
    using BovineLabs.Grove.Utility;
    using Unity.Transforms;

    public partial struct MyContext : IContext<MyContext>
    {
        public ComponentContainer<LocalTransform> LocalTransform;

        public BufferContainer<GraphState> GraphStates;


        public DynamicUntypedHashMap<short> GetState(int index)
        {
            return this.GraphStates.GetRW(index).AsMap();
        }
    }
}
