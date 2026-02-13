// <copyright file="IsDirectionOldNode.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Grove.Sample.Authoring.Nodes.Data
{
    using BovineLabs.Grove.Authoring;
    using BovineLabs.Grove.Authoring.Nodes;
    using BovineLabs.Grove.Core;
    using BovineLabs.Grove.Sample.Data.Core;
    using BovineLabs.Grove.Sample.Data.Nodes.Data;
    using Unity.Entities;

    [Node((int)DataType.IsDirectionOld, typeof(IMyNode))]
    public class ScoreIsDirectionOldNode : DataNode<float>
    {
        public float Duration = 10;
        public Scorer Score = Scorer.Default;

        protected override INodeElement CreateElement()
        {
            return new ScoreIsDirectionOldNodeElement(this);
        }
    }

    internal class ScoreIsDirectionOldNodeElement : DataNodeElement<ScoreIsDirectionOldData, float>
    {
        private readonly ScoreIsDirectionOldNode data;

        public ScoreIsDirectionOldNodeElement(ScoreIsDirectionOldNode data)
            : base(data)
        {
            this.data = data;
        }

        /// <inheritdoc/>
        protected override void Init(ref BlobBuilder builder, ref ScoreIsDirectionOldData score, GraphBuildState state)
        {
            score.Duration = this.data.Duration;
            score.Score = this.data.Score;
        }
    }
}
