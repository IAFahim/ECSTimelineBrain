using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class GameObjectInstantiateClip : DOTSClip, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Looping;

        public override double duration => 1;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new GameObjectInstantiateComponent
            {
                prefab = context.Baker.GetEntity(context.Director.GetGenericBinding(context.Track) as GameObject, TransformUsageFlags.None)
            });
            base.Bake(clipEntity, context);
        }
    }
}