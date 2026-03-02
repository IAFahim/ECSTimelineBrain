using Unity.Entities;
using Unity.Mathematics;
using Hash128 = Unity.Entities.Hash128;

namespace BovineLabs.Timeline.Tracks.Data.Animations
{
    public struct SimpleAnimatorComponent : IComponentData
    {
        // Settings
        public float DefaultTransitionDuration;

        // Current State
        public Hash128 CurrentClip;
        public float CurrentTime;
        public float CurrentSpeed;
        public uint CurrentMotionId;

        // Next State (Transitioning)
        public Hash128 NextClip;
        public float NextTime;
        public float NextSpeed;
        public uint NextMotionId;

        // Blending State
        public float TransitionDuration;
        public float TransitionElapsed;

        public bool IsTransitioning => TransitionElapsed < TransitionDuration && TransitionDuration > 0f;

        /// <summary>
        /// Requests a new animation. 
        /// </summary>
        /// <param name="clip">The hash of the animation clip to play.</param>
        /// <param name="speed">Playback speed.</param>
        /// <param name="transitionDuration">Duration of the crossfade. -1 uses the default.</param>
        /// <param name="forceRestart">If true, blends the clip with itself if it's already playing. If false, ignores the request.</param>
        public void Play(Hash128 clip, float speed = 1f, float transitionDuration = -1f, bool forceRestart = true)
        {
            // Ignore if we are already playing this clip and don't want to restart
            if (!forceRestart && CurrentClip == clip && !IsTransitioning)
                return;

            NextClip = clip;
            NextTime = 0f;
            NextSpeed = speed;
            TransitionDuration = transitionDuration >= 0f ? transitionDuration : DefaultTransitionDuration;
            TransitionElapsed = 0f;
            
            // CRITICAL: Increment Motion ID so Rukhanka knows this is a brand new instance,
            // allowing us to smoothly blend "Idle" into "Idle".
            NextMotionId = CurrentMotionId + 1; 
        }
    }
}