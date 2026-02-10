# PhysicsVelocity Timeline Track Implementation Guide

Complete implementation for driving Unity Physics velocity through Timeline clips using BovineLabs.Timeline.

## Architecture Overview

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Authoring Clip │ ──> │   Animated Data  │ ──> │   Mixer System  │
│  (Editor Only)  │     │  (Runtime Data)  │     │   (Runtime)     │
└─────────────────┘     └──────────────────┘     └─────────────────┘
                                │                        │
                                v                        v
                        ┌──────────────────┐     ┌─────────────────┐
                        │ TrackBlendImpl   │     │ PhysicsVelocity │
                        │ (Generic Blend)  │ ──> │   Component     │
                        └──────────────────┘     └─────────────────┘
```

## Required Components

| Component | Purpose | Location |
|-----------|---------|----------|
| `PhysicsVelocityMixer` | Blends velocity values | `BovineLabs.Timeline.Tracks/Mixers/` |
| `PhysicsVelocityAnimated` | Holds clip data | `BovineLabs.Timeline.Tracks.Data/` |
| `PhysicsVelocityTrackSystem` | Applies blended velocity | `BovineLabs.Timeline.Tracks/` |
| `PhysicsVelocityClip` | Authoring & baking | `BovineLabs.Timeline.Authoring/` |

---

## 1. Mixer Implementation

**File:** `BovineLabs.Timeline.Tracks/Mixers/PhysicsVelocityMixer.cs`

```csharp
using Unity.Physics;
using Unity.Mathematics;
using BovineLabs.Timeline;

namespace BovineLabs.Timeline.Tracks
{
    public readonly struct PhysicsVelocityMixer : IMixer<PhysicsVelocity>
    {
        [BurstCompile]
        public PhysicsVelocity Lerp(in PhysicsVelocity a, in PhysicsVelocity b, in float s)
        {
            return new PhysicsVelocity
            {
                Linear = math.lerp(a.Linear, b.Linear, s),
                Angular = math.lerp(a.Angular, b.Angular, s)
            };
        }

        [BurstCompile]
        public PhysicsVelocity Add(in PhysicsVelocity a, in PhysicsVelocity b)
        {
            return new PhysicsVelocity
            {
                Linear = a.Linear + b.Linear,
                Angular = a.Angular + b.Angular
            };
        }
    }
}
```

---

## 2. Animated Component

**File:** `BovineLabs.Timeline.Tracks.Data/PhysicsVelocityAnimated.cs`

```csharp
using Unity.Entities;
using Unity.Physics;
using BovineLabs.Timeline.Data;

namespace BovineLabs.Timeline.Tracks.Data
{
    public struct PhysicsVelocityAnimated : IAnimatedComponent<PhysicsVelocity>
    {
        public PhysicsVelocity Value { get; set; }
    }
}
```

---

## 3. Track System

**File:** `BovineLabs.Timeline.Tracks/PhysicsVelocityTrackSystem.cs`

```csharp
using BovineLabs.Timeline;
using BovineLabs.Timeline.Data;
using BovineLabs.Timeline.Tracks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace BovineLabs.Timeline.Tracks
{
    [UpdateInGroup(typeof(TimelineComponentAnimationGroup))]
    public partial struct PhysicsVelocityTrackSystem : ISystem
    {
        private TrackBlendImpl<PhysicsVelocity, PhysicsVelocityAnimated> impl;

        public const int INNERLOOP_BATCH_COUNT = 64; // Batch size for parallel job scheduling

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            impl.OnCreate(ref state);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            impl.OnDestroy(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var velocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>(false);
            var blendData = impl.Update(ref state);

            state.Dependency = new WriteVelocityJob
            {
                BlendData = blendData,
                VelocityLookup = velocityLookup
            }.ScheduleParallel(blendData, INNERLOOP_BATCH_COUNT, state.Dependency);
        }

        [BurstCompile]
        private partial struct WriteVelocityJob : IJobParallelHashMapDefer
        {
            [ReadOnly]
            public NativeParallelHashMap<Entity, MixData<PhysicsVelocity>>.ReadOnly BlendData;

            [NativeDisableParallelForRestriction]
            public ComponentLookup<PhysicsVelocity> VelocityLookup;

            public void ExecuteNext(int entryIndex, int jobIndex)
            {
                Read(BlendData, entryIndex, out var entity, out var mixResult);

                if (!VelocityLookup.HasComponent(entity))
                    return;

                ref var currentVelocity = ref VelocityLookup.GetRefRW(entity).ValueRW;

                var timelineContribution = JobHelpers.Blend<PhysicsVelocity, PhysicsVelocityMixer>(ref mixResult, default);

                currentVelocity.Linear += timelineContribution.Linear;
                currentVelocity.Angular += timelineContribution.Angular;
            }
        }
    }
}
```

---

## 4. Authoring Clip

**File:** `BovineLabs.Timeline.Authoring/Tracks/PhysicsVelocityClip.cs`

```csharp
using BovineLabs.Timeline.Authoring;
using BovineLabs.Timeline.Tracks.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.Timeline;

namespace BovineLabs.Timeline.Authoring
{
    [TrackClipType(typeof(PhysicsVelocityClip))]
    public class PhysicsVelocityClip : DOTSClip, ITimelineClipAsset
    {
        public const string DEFAULT_DISPLAY_NAME = "Physics Velocity";

        [SerializeField]
        [Tooltip("Linear velocity in world units per second")]
        private Vector3 linearVelocity;

        [SerializeField]
        [Tooltip("Angular velocity in radians per second")]
        private Vector3 angularVelocity;

        public float3 LinearVelocity => linearVelocity;
        public float3 AngularVelocity => angularVelocity;

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override void Bake(Entity clipEntity, BakingContext context)
        {
            context.Baker.AddComponent(clipEntity, new PhysicsVelocityAnimated
            {
                Value = new PhysicsVelocity
                {
                    Linear = LinearVelocity,
                    Angular = AngularVelocity
                }
            });

            if (context.Binding != null)
                context.Baker.AddTransformUsageFlags(context.Binding.Target, TransformUsageFlags.Dynamic);

            base.Bake(clipEntity, context);
        }
    }
}
```

---

## 5. Track Asset (Optional but Recommended)

**File:** `BovineLabs.Timeline.Authoring/Tracks/PhysicsVelocityTrack.cs`

```csharp
using UnityEngine.Timeline;
using BovineLabs.Timeline.Authoring;

namespace BovineLabs.Timeline.Authoring
{
    [TrackClipType(typeof(PhysicsVelocityClip))]
    [TrackBindingType(typeof(Transform))]
    public class PhysicsVelocityTrack : TrackAsset
    {
        public const string TRACK_NAME = "Physics Velocity";

        public override TrackAsset[] GetChildTracks()
        {
            return System.Array.Empty<TrackAsset>();
        }
    }
}
```

---

## Execution Flow

```
Timeline Playback
       │
       ▼
┌──────────────────────────────┐
│  Baking Phase (Editor)       │
│  - PhysicsVelocityClip       │
│    → PhysicsVelocityAnimated │
└──────────────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│  Runtime Phase               │
│  - TrackBlendImpl gathers    │
│    all active clips          │
│  - Calculates blend weights  │
└──────────────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│  WriteVelocityJob            │
│  - Reads blended value       │
│  - Adds to PhysicsVelocity   │
│  - Physics simulation runs   │
│    in FixedStep group        │
└──────────────────────────────┘
```

---

## Usage Examples

### Constant Forward Thrust
```csharp
// Clip settings
LinearVelocity: (0, 0, 10)
AngularVelocity: (0, 0, 0)
```

### Rotational Impulse
```csharp
// Clip settings
LinearVelocity: (0, 0, 0)
AngularVelocity: (0, 5, 0) // 5 rad/s around Y axis
```

### Complex Motion Curve
Create multiple clips with different values:
1. 0-2s: Acceleration forward
2. 2-4s: Rotate while maintaining speed
3. 4-6s: Decelerate

---

## Advanced: Additive vs Absolute Modes

**Current Implementation:** Additive (timeline adds to existing physics)

For absolute control (override physics), modify `WriteVelocityJob`:

```csharp
// ABSOLUTE MODE (Overrides gravity, collisions)
currentVelocity.Linear = timelineContribution.Linear;
currentVelocity.Angular = timelineContribution.Angular;
```

To support both modes:

```csharp
public struct PhysicsVelocityMode
{
    public const int ADDITIVE = 0;
    public const int ABSOLUTE = 1;
    public const int MULTIPLY = 2;
}

// In clip
public int BlendMode = PhysicsVelocityMode.ADDITIVE;

// In job
switch (blendMode)
{
    case PhysicsVelocityMode.ADDITIVE:
        currentVelocity.Linear += timelineContribution.Linear;
        break;
    case PhysicsVelocityMode.ABSOLUTE:
        currentVelocity.Linear = timelineContribution.Linear;
        break;
    case PhysicsVelocityMode.MULTIPLY:
        currentVelocity.Linear *= timelineContribution.Linear;
        break;
}
```

---

## Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| Object doesn't move | No PhysicsVelocity on target | Add PhysicsBody component |
| Gravity not working | Using absolute mode | Switch to additive (default) |
| Jittery movement | System update order wrong | Ensure system runs before FixedStepSimulation |
| Clips not blending | Missing ClipCaps.Blending | Verify clipCaps returns Blending |

---

## Performance Notes

- System runs in `TimelineComponentAnimationGroup` (before simulation)
- Job is parallelized with batch size of 64 entities
- Burst-compiled for maximum performance
- No GC allocation after initialization

---

## Integration Checklist

- [ ] Mixer registered with Timeline system
- [ ] System added to ECS world (auto via attribute)
- [ ] Authoring clips can be created in Timeline
- [ ] Runtime entities receive PhysicsVelocityAnimated
- [ ] Timeline playback drives PhysicsVelocity
- [ ] Additive mode preserves gravity/collisions
