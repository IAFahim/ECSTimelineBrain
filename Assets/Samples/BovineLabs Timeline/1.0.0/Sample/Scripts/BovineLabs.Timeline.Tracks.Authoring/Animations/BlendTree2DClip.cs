using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka;
using Rukhanka.Hybrid;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    // --- 1. THE CLIP ---
    public class BlendTree2DClip : DOTSClip, ITimelineClipAsset
    {[Tooltip("The X/Y direction to feed into the Blend Tree (e.g., Velocity X/Z)")]
        public Vector2 BlendParameter;

        // Allows clips to be dragged into each other for smooth interpolation
        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | ClipCaps.Looping;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new BlendTree2DClipData
            {
                Value = BlendParameter
            });

            base.Bake(clipEntity, context);
        }
    }

    
}