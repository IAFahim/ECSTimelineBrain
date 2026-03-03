using BovineLabs.Timeline.Tracks.Data.Animations;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    public class BlendTree2DClip : DOTSClip, ITimelineClipAsset
    {[Tooltip("The X/Y direction to feed into the Blend Tree (e.g., Velocity X/Z)")]
        public Vector2 BlendParameter;

        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | ClipCaps.Looping;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new BlendTree2DDirectionClipData
            {
                Value = BlendParameter
            });

            base.Bake(clipEntity, context);
        }
    }
}