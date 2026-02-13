// <copyright file="MyGraph.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample.Data.Core
{
    using Unity.Entities;

    public struct MyGraph : IComponentData, IGraphReference
    {
        public BlobAssetReference<GraphData> Graph;

        /// <inheritdoc />
        unsafe ref BlobAssetReference<GraphData> IGraphReference.GraphRef
        {
            get
            {
                fixed (MyGraph* ptr = &this)
                {
                    return ref ptr->Graph;
                }
            }
        }
    }
}
