using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka.Hybrid;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class RukhankaAnimationClip : DOTSClip, ITimelineClipAsset
    {
        public AnimationClip animationClipHolder;

        public override double duration => animationClipHolder != null ? animationClipHolder.length : base.duration;
        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | ClipCaps.Looping;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            if (animationClipHolder != null)
            {
                Avatar avatar = null;

                if (context.Director != null && context.Track != null)
                {
                    var binding = context.Director.GetGenericBinding(context.Track);

                    if (binding is RigDefinitionAuthoring rigDef)
                    {
                        avatar = rigDef.GetAvatar();
                    }
                    else if (binding is GameObject go)
                    {
                        var rig = go.GetComponent<RigDefinitionAuthoring>();
                        if (rig != null) avatar = rig.GetAvatar();
                    }
                }

                var animHash = BakingUtils.ComputeAnimationHash(animationClipHolder, avatar);

                context.Baker.AddComponent(clipEntity, new RukhankaAnimationClipAnimated
                {
                    AnimationHash = animHash
                });
            }

            base.Bake(clipEntity, context);
        }
    }
}