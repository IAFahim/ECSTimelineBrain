// <copyright file="MyGraphBakingSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample.Authoring.Core
{
    using BovineLabs.Grove.Authoring;
    using BovineLabs.Grove.Sample.Data.Core;
    using Unity.Entities;

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial class MyGraphBakingSystem : GraphBakingSystem<MyGraph, MyGraphAsset>
    {
        protected override string VersionKey => "my_graph_version";
    }
}
