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
    [Serializable]
    [TrackClipType(typeof(RukhankaAnimationClip))]
    [TrackColor(0.25f, 0.25f, 0)]
    [TrackBindingType(typeof(RigDefinitionAuthoring))]
    [DisplayName("DOTS/Animation Track")]
    public class RukhankaAnimationTrack : DOTSTrack
    {
        public AnimationClip exitIdleClip;
        public float exitTransitionDuration = 0.25f;

        protected override void Bake(BakingContext context)
        {
            var rigDef = context.Director.ResolveRigDefinition(this);
            if (rigDef == null)
            {
                base.Bake(context);
                return;
            }

            var clipsToBake = CollectClipsToBake();
            if (clipsToBake.Count == 0)
            {
                base.Bake(context);
                return;
            }

            BakeAnimationsToEntity(context, rigDef, clipsToBake);

            context.Baker.AddComponent(context.TrackEntity, new RukhankaTimelineTrack
            {
                ExitIdleClipHash = exitIdleClip.ComputeHashOrDefault(rigDef.GetAvatar()),
                ExitTransitionDuration = exitTransitionDuration
            });

            base.Bake(context);
        }

        private HashSet<AnimationClip> CollectClipsToBake()
        {
            var clips = GetClips()
                .Select(c => c.asset as RukhankaAnimationClip)
                .Where(h => h?.animationClipHolder != null)
                .Select(h => h.animationClipHolder)
                .ToHashSet();

            if (exitIdleClip != null)
                clips.Add(exitIdleClip);

            return clips;
        }

        private void BakeAnimationsToEntity(BakingContext context, RigDefinitionAuthoring rigDef,
            HashSet<AnimationClip> clipsToBake)
        {
            var bakedAnimations = new AnimationClipBaker().BakeAnimations(
                context.Baker,
                clipsToBake.ToArray(),
                rigDef.GetAvatar(),
                rigDef.gameObject
            );

            var e = context.Baker.CreateAdditionalEntity(TransformUsageFlags.None, false, name + "_AnimationAssets");
            var buffer = context.Baker.AddBuffer<NewBlobAssetDatabaseRecord<AnimationClipBlob>>(e);

            buffer.AddValidAnimations(bakedAnimations);

            if (bakedAnimations.IsCreated) bakedAnimations.Dispose();
        }
    }
}