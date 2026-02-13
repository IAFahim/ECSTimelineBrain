// <copyright file="MyGraphInstanceEditor.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample.Editor
{
    using BovineLabs.Grove.Authoring;
    using BovineLabs.Grove.Editor;
    using BovineLabs.Grove.Sample.Authoring.Core;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(GraphInstance<MyGraphAsset>))]
    public class MyGraphInstanceEditor : GraphInstanceEditor<MyGraphAsset>
    {
    }
}
