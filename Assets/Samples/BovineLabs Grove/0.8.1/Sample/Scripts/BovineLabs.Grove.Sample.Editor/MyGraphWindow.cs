// <copyright file="MyGraphWindow.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample.Editor
{
    using System;
    using BovineLabs.Grove.Authoring.Nodes;
    using BovineLabs.Grove.Editor;
    using BovineLabs.Grove.Sample.Authoring.Core;
    using BovineLabs.Grove.Sample.Authoring.Nodes;
    using UnityEditor;
    using UnityEditor.Callbacks;

    public class MyGraphWindow: GraphEditorWindow<MyGraphWindow, MyContext, MyGraphAsset>
    {
        protected override Type[] ValidNodes { get; } = { typeof(IDefaultNode), typeof(IMyNode) };

        [OnOpenAsset(1)]
        private static bool OpenGraphAsset(int instanceId, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceId) is not MyGraphAsset graphAsset)
            {
                return false;
            }

            ShowWindow(graphAsset);
            return true;
        }
    }
}
