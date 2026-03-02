using Unity.Entities;
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
        public void Play(Hash128 clip, float speed = 1f, float transitionDuration = -1f, bool forceRestart = true)
        {
            // Ignore if we are already playing this clip and don't want to restart
            if (!forceRestart && CurrentClip == clip && !IsTransitioning)
                return;

            float actualTransitionDuration = transitionDuration >= 0f ? transitionDuration : DefaultTransitionDuration;

            // FIX: If transition is 0 or less, immediately snap to the new clip.
            if (actualTransitionDuration <= 0f)
            {
                CurrentClip = clip;
                CurrentTime = 0f;
                CurrentSpeed = speed;
                CurrentMotionId++; // Increment to ensure Rukhanka restarts it
                
                NextClip = default;
                TransitionDuration = 0f;
                TransitionElapsed = 0f;
            }
            else
            {
                NextClip = clip;
                NextTime = 0f;
                NextSpeed = speed;
                TransitionDuration = actualTransitionDuration;
                TransitionElapsed = 0f;
                
                NextMotionId = CurrentMotionId + 1; 
            }
        }
    }
}