// <copyright file="MyGraphAuthoring.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample.Authoring.Core
{
    using BovineLabs.Grove.Authoring;
    using BovineLabs.Grove.Sample.Data.Core;
    using Unity.Entities;
    using UnityEngine;

    [DisallowMultipleComponent]
    public class MyGraphAuthoring : GraphAuthoring<MyGraph, MyGraphAsset>
    {
        // Note this is inheriting from a custom baker GraphAuthoring<TG, TA>.Baker<T>
        private class Baker : Baker<MyGraphAuthoring>
        {
            public override void Bake(MyGraphAuthoring authoring)
            {
                base.Bake(authoring);

                this.AddBuffer<GraphState>(this.GetEntity(TransformUsageFlags.Dynamic)).Initialize();
            }
        }
    }
}
