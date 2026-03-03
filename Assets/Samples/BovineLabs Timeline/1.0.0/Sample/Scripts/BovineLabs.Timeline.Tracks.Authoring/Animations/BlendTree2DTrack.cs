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
    [Serializable][TrackClipType(typeof(BlendTree2DClip))][TrackColor(0.2f, 0.7f, 0.5f)]
    [TrackBindingType(typeof(RigDefinitionAuthoring))][DisplayName("DOTS/Blend Tree 2D Track")]
    public class BlendTree2DTrack : DOTSTrack
    {
        [Serializable]
        public class BlendTree2DMotionEntry
        {
            public AnimationClip clip;[Range(-180, 180)] public float degreeCalc;
            public float rangeCalc = 1;
            [SerializeField] private Vector2 directionCalc;
            public Vector2 direction;

            internal Vector2 CalcDirection()
            {
                var radians = degreeCalc * Mathf.Deg2Rad;
                directionCalc = new Vector2(Mathf.Sin(radians) * rangeCalc, Mathf.Cos(radians) * rangeCalc);
                return directionCalc;
            }
        }

        public MotionBlob.Type BlendTreeType = MotionBlob.Type.BlendTree2DSimpleDirectional;[Tooltip("Layer Index allows you to blend multiple tracks. 0 = Base, 1+ = Overrides.")]
        public int LayerIndex = 0;[Header("Exit / Fallback Override (Optional)")]
        public AnimationClip ExitIdleClip;[Min(0.001f)] public float BlendInDuration = 0.25f;[Min(0.001f)] public float BlendOutDuration = 0.25f;

        public List<BlendTree2DMotionEntry> Motions = new();

        protected override void Bake(BakingContext context)
        {
            var director = context.Director;
            var binding = director.GetGenericBinding(this);
            var rigDef = binding as RigDefinitionAuthoring ?? (binding as GameObject)?.GetComponent<RigDefinitionAuthoring>();

            if (rigDef == null) { base.Bake(context); return; }

            var baker = context.Baker;
            var trackEntity = context.TrackEntity;
            var avatar = rigDef.GetAvatar();

            baker.AddComponent(trackEntity, new BlendAnimationTree2DTrackData { BlendTreeType = BlendTreeType, LayerIndex = LayerIndex });
            baker.AddComponent(trackEntity, new BlendTreePlaybackState { IsInitialized = false });

            var motionBuffer = baker.AddBuffer<BlendTree2DMotionData>(trackEntity);
            var clipsToBake = new List<AnimationClip>();
            int index = 0;

            foreach (var motion in Motions)
            {
                if (motion.clip == null) continue;
                motionBuffer.Add(new BlendTree2DMotionData
                {
                    AnimationHash = BakingUtils.ComputeAnimationHash(motion.clip, avatar),
                    BlendTree2DMotionElement = new ScriptedAnimator.BlendTree2DMotionElement { pos = motion.direction, motionIndex = index++ }
                });
                clipsToBake.Add(motion.clip);
            }

            // Bake the Exit Idle Clip if assigned
            if (ExitIdleClip != null)
            {
                baker.AddComponent(trackEntity, new TrackFallbackOverride
                {
                    FallbackClipHash = BakingUtils.ComputeAnimationHash(ExitIdleClip, avatar),
                    BlendInSpeed = 1f / Mathf.Max(0.001f, BlendInDuration),
                    BlendOutSpeed = 1f / Mathf.Max(0.001f, BlendOutDuration)
                });
                clipsToBake.Add(ExitIdleClip);
            }

            if (clipsToBake.Count > 0)
            {
                var bakedAnimations = new AnimationClipBaker().BakeAnimations(baker, clipsToBake.ToArray(), avatar, rigDef.gameObject);
                var e = baker.CreateAdditionalEntity(TransformUsageFlags.None, false, name + "_BlendTreeAssets");
                var dbBuffer = baker.AddBuffer<NewBlobAssetDatabaseRecord<AnimationClipBlob>>(e);

                foreach (var ba in bakedAnimations.Where(ba => ba != BlobAssetReference<AnimationClipBlob>.Null))
                    dbBuffer.Add(new NewBlobAssetDatabaseRecord<AnimationClipBlob> { hash = ba.Value.hash, value = ba });

                if (bakedAnimations.IsCreated) bakedAnimations.Dispose();
            }

            base.Bake(context);
        }

        private void OnValidate() { foreach (var motion in Motions) motion.CalcDirection(); }
    }
}