using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data.Animations;
using Rukhanka;
using Rukhanka.Hybrid;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    // Data attached to the track entity

    [Serializable][TrackClipType(typeof(RukhankaAnimationClip))]
    [TrackColor(0.25f, 0.25f, 0)][TrackBindingType(typeof(RigDefinitionAuthoring))] // Target the Rig
    [DisplayName("DOTS/Animation Track")]
    public class RukhankaAnimationTrack : DOTSTrack
    {[Tooltip("Layer Index allows you to blend multiple tracks. 0 = Base, 1+ = Overrides.")]
        public int LayerIndex = 0;

        protected override void Bake(BakingContext context)
        {
            var rigDef = context.Director.ResolveRigDefinition(this);
            if (rigDef == null)
            {
                base.Bake(context);
                return;
            }

            // 1. Tag the Track Entity with the Layer ID
            context.Baker.AddComponent(context.TrackEntity, new RukhankaSingleTrackData
            {
                LayerIndex = LayerIndex
            });

            // 2. Bake clips to Rukhanka DB
            var clipsToBake = GetClips()
                .Select(c => c.asset as RukhankaAnimationClip)
                .Where(h => h?.animationClipHolder != null)
                .Select(h => h.animationClipHolder)
                .ToHashSet();

            if (clipsToBake.Count > 0)
            {
                var bakedAnimations = new AnimationClipBaker().BakeAnimations(
                    context.Baker, clipsToBake.ToArray(), rigDef.GetAvatar(), rigDef.gameObject);

                var e = context.Baker.CreateAdditionalEntity(TransformUsageFlags.None, false, name + "_AnimationAssets");
                var buffer = context.Baker.AddBuffer<NewBlobAssetDatabaseRecord<AnimationClipBlob>>(e);
                buffer.AddValidAnimations(bakedAnimations);

                if (bakedAnimations.IsCreated) bakedAnimations.Dispose();
            }

            base.Bake(context);
        }
    }
}