// <copyright file="RotationTrack.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using BovineLabs.Timeline.Tracks.Data;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    [Serializable]
    [TrackClipType(typeof(RotationLookAtTargetClip))]
    [TrackClipType(typeof(RotationLookAtStartClip))]
    [TrackColor(0, 0.25f, 0)]
    [TrackBindingType(typeof(Transform))]
    [DisplayName("DOTS/Transform Rotation Track")]
    public class TransformRotationTrack : DOTSTrack
    {
        public bool ResetRotationOnDeactivate;

        protected override void Bake(BakingContext context)
        {
            if (ResetRotationOnDeactivate) context.Baker.AddComponent<RotationResetOnDeactivate>(context.TrackEntity);
        }
    }
}