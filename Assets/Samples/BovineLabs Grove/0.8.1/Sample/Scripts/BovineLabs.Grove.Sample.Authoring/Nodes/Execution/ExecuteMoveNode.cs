// <copyright file="MoveNode.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample.Authoring.Nodes.Execution
{
    using BovineLabs.Grove.Authoring;
    using BovineLabs.Grove.Authoring.Nodes;
    using BovineLabs.Grove.Sample.Data.Core;
    using BovineLabs.Grove.Sample.Data.Nodes;
    using BovineLabs.Grove.Sample.Data.Nodes.Execution;
    using Unity.Entities;

    [Node((int)ExecutionType.Move, typeof(IMyNode))]
    public class ExecuteMoveNode : ExecutionNode
    {
        public float Speed = 1;

        protected override INodeElement CreateElement()
        {
            return new MoveNodeElement(this);
        }
    }

    internal sealed class MoveNodeElement : ExecutionNodeElement<ExecuteMoveData>
    {
        private readonly ExecuteMoveNode data;

        public MoveNodeElement(ExecuteMoveNode data)
            : base(data)
        {
            this.data = data;
        }

        protected override void Init(ref BlobBuilder builder, ref ExecuteMoveData execution, GraphBuildState state)
        {
            execution.Speed = this.data.Speed;
        }
    }
}
