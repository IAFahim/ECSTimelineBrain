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
        public HashSet<AnimationClip> AnimationClips { get; private set; } = new();

        protected override void Bake(BakingContext context)
        {
            foreach (var clip in GetClips())
            {
                if (clip.asset is RukhankaAnimationClip holder && holder.animationClipHolder != null)
                {
                    AnimationClips.Add(holder.animationClipHolder);
                }
            }

            if (AnimationClips.Count == 0)
            {
                base.Bake(context);
                return;
            }

            var binding = context.Director.GetGenericBinding(this);
            RigDefinitionAuthoring rigDef = binding as RigDefinitionAuthoring;

            if (rigDef == null && binding is GameObject go)
            {
                rigDef = go.GetComponent<RigDefinitionAuthoring>();
            }

            if (rigDef == null)
            {
                base.Bake(context);
                return;
            }

            var avatar = rigDef.GetAvatar();

            var animationBaker = new AnimationClipBaker();
            var bakedAnimations = animationBaker.BakeAnimations(
                context.Baker,
                AnimationClips.ToArray(),
                avatar,
                rigDef.gameObject);

            var e = context.Baker.CreateAdditionalEntity(TransformUsageFlags.None, false, name + "_AnimationAssets");
            var newAnimArr = context.Baker.AddBuffer<NewBlobAssetDatabaseRecord<AnimationClipBlob>>(e);

            foreach (var ba in bakedAnimations)
            {
                if (ba != BlobAssetReference<AnimationClipBlob>.Null)
                {
                    newAnimArr.Add(new NewBlobAssetDatabaseRecord<AnimationClipBlob>
                    {
                        hash = ba.Value.hash,
                        value = ba
                    });
                }
            }

            if (bakedAnimations.IsCreated)
            {
                bakedAnimations.Dispose();
            }
            
            context.Baker.AddComponent<RukhankaTimelineTargetTag>(context.TrackEntity);
            base.Bake(context);
        }
    }
}