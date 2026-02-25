using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka.Hybrid;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class RukhankaAnimationClip : DOTSClip, ITimelineClipAsset
    {
        // Support blending, trimming (ClipIn), speed multipliers, and looping in the Timeline window
        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | ClipCaps.Looping;
        
        public AnimationClip animationClipHolder;

        // Automatically snap the clip duration to the Unity AnimationClip length in the editor
        public override double duration => animationClipHolder != null ? animationClipHolder.length : base.duration;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            if (animationClipHolder != null)
            {
                Avatar avatar = null;
                
                // 1. Resolve the binding to find the target RigDefinitionAuthoring and its Avatar
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

                // 2. Compute the Rukhanka Hash for this specific clip and avatar combination
                var animHash = BakingUtils.ComputeAnimationHash(animationClipHolder, avatar);

                // 3. Store the hash on the clip entity for the runtime system to read
                context.Baker.AddComponent(clipEntity, new RukhankaAnimationClipData
                {
                    AnimationHash = animHash
                });
            }

            // Let BovineLabs.Timeline bake the standard LocalTime, ClipWeight, etc.
            base.Bake(clipEntity, context);
        }
    }
}