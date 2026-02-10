using Samples.BovineLabs_Timeline._1._0._0.Sample.Scripts.BovineLabs.Timeline.Tracks.Data;
using Unity.Entities;
using UnityEngine;

namespace Samples.BovineLabs_Timeline._1._0._0.Sample.Scripts.BovineLabs.Timeline.Tracks.Authoring
{
    public class TimelineReferenceAuthoring : MonoBehaviour
    {
        private class Baker : Baker<TimelineReferenceAuthoring>
        {
            public override void Bake(TimelineReferenceAuthoring authoring)
            {
                this.AddComponent<TimelineReference>(this.GetEntity(TransformUsageFlags.None));
            }
        }
    }
}