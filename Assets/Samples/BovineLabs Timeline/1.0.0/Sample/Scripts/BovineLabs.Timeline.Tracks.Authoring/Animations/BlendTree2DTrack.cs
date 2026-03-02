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
    [TrackClipType(typeof(BlendTree2DClip))]
    [TrackColor(0.2f, 0.7f, 0.5f)]
    [TrackBindingType(typeof(RigDefinitionAuthoring))]
    [DisplayName("DOTS/Blend Tree 2D Track")]
    public class BlendTree2DTrack : DOTSTrack
    {
        [Serializable]
        public class BlendTree2DMotionEntry
        {
            public AnimationClip Clip;
            public Vector2 Position;
        }

        public MotionBlob.Type BlendTreeType = MotionBlob.Type.BlendTree2DSimpleDirectional;

        [Tooltip("When enabled, all clips are treated as walk speed (magnitude 0.5) " +
                 "regardless of 'run' or 'walk' keywords in the clip name. " +
                 "Use this when your blend tree only contains walk animations " +
                 "and you want them distributed across the full −1…+1 range.")]
        public bool WalkOnly;

        public List<BlendTree2DMotionEntry> Motions = new List<BlendTree2DMotionEntry>();

        // ──────────────────────────────────────────────────────────────────────
        //  Context-menu helpers (right-click the track in Timeline)
        // ──────────────────────────────────────────────────────────────────────

        [ContextMenu("Auto-Distribute Positions (Smart)")]
        public void AutoDistributePositionsSmart()
        {
            BlendTree2DPositionUtility.WalkOnly = WalkOnly;
            BlendTree2DPositionUtility.AutoDistribute(Motions);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [ContextMenu("Auto-Distribute Positions (Uniform Circle)")]
        public void AutoDistributePositionsUniform()
        {
            BlendTree2DPositionUtility.DistributeUniform(Motions, includeCenter: false);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [ContextMenu("Auto-Distribute Positions (Uniform Circle + Centre)")]
        public void AutoDistributePositionsUniformWithCenter()
        {
            BlendTree2DPositionUtility.DistributeUniform(Motions, includeCenter: true);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Baking
        // ──────────────────────────────────────────────────────────────────────

        protected override void Bake(BakingContext context)
        {
            var director = context.Director;
            var binding  = director.GetGenericBinding(this);
            var rigDef   = binding as RigDefinitionAuthoring ??
                           (binding as GameObject)?.GetComponent<RigDefinitionAuthoring>();

            if (rigDef == null) { base.Bake(context); return; }

            var baker       = context.Baker;
            var trackEntity = context.TrackEntity;
            var avatar      = rigDef.GetAvatar();

            baker.AddComponent(trackEntity, new BlendTree2DTrackData { BlendTreeType = BlendTreeType });
            var motionBuffer = baker.AddBuffer<BlendTree2DMotionData>(trackEntity);

            var clipsToBake = new List<AnimationClip>();
            foreach (var entry in Motions)
            {
                if (entry.Clip == null) continue;
                motionBuffer.Add(new BlendTree2DMotionData
                {
                    AnimationHash = BakingUtils.ComputeAnimationHash(entry.Clip, avatar),
                    Position      = entry.Position,
                });
                clipsToBake.Add(entry.Clip);
            }

            if (clipsToBake.Count > 0)
            {
                var bakedAnimations =
                    new AnimationClipBaker().BakeAnimations(baker, clipsToBake.ToArray(), avatar, rigDef.gameObject);
                var e        = baker.CreateAdditionalEntity(TransformUsageFlags.None, false, name + "_BlendTreeAssets");
                var dbBuffer = baker.AddBuffer<NewBlobAssetDatabaseRecord<AnimationClipBlob>>(e);

                foreach (var ba in bakedAnimations.Where(ba => ba != BlobAssetReference<AnimationClipBlob>.Null))
                    dbBuffer.Add(new NewBlobAssetDatabaseRecord<AnimationClipBlob> { hash = ba.Value.hash, value = ba });

                if (bakedAnimations.IsCreated) bakedAnimations.Dispose();
            }

            base.Bake(context);
        }

        // ──────────────────────────────────────────────────────────────────────
        //  OnValidate: silently infer positions for newly-zeroed entries
        // ──────────────────────────────────────────────────────────────────────
        private void OnValidate()
        {
            BlendTree2DPositionUtility.WalkOnly = WalkOnly;

            foreach (var e in Motions)
            {
                if (e == null) continue;

                // Clip was cleared → reset position to zero
                if (e.Clip == null)
                {
                    if (e.Position != Vector2.zero)
                    {
                        e.Position = Vector2.zero;
                    }
                    continue;
                }

                if (BlendTree2DPositionUtility.TryInferPosition(e.Clip.name, out var inferred))
                {
                    e.Position = inferred;
                }
            }
        }
    }
}