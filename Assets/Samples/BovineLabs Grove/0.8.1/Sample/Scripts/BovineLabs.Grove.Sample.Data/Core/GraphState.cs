// <copyright file="GraphState.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample.Data.Core
{
    using BovineLabs.Core.Iterators;
    using JetBrains.Annotations;
    using Unity.Entities;

    [InternalBufferCapacity(0)]
    public struct GraphState : IDynamicUntypedHashMap<short>
    {
        [UsedImplicitly]
        byte IDynamicUntypedHashMap<short>.Value { get; }
    }
}
