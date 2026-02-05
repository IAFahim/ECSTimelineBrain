# Movements Folder Flattening Plan

## Current Structure (3-4 levels deep) ❌
```
Movement.Data/
├── Advanced/
│   ├── Facets/
│   ├── Targets/
│   └── Utilities/
├── Parameters/
│   ├── Easing/
│   ├── Motion/
│   └── Timing/
├── Tags/
│   ├── Modifiers/
│   ├── MovementTypes/
│   └── Targets/
└── Transforms/
    ├── StartEnd/
    └── Unassigned/
```

## Proposed Structure (1 level max) ✅

**Principle:** Flatten subfolders into parent categories

```
Movement.Data/
├── Parameters/
│   ├── MoveEaseComponent.cs
│   ├── TimerEaseComponent.cs
│   ├── PeakBiasComponent.cs
│   ├── AmplitudeComponent.cs
│   ├── DirectionComponent.cs
│   ├── FrequencyComponent.cs
│   ├── SpeedComponent.cs
│   ├── WobbleComponent.cs
│   ├── HeightComponent.cs
│   ├── PitchComponent.cs
│   ├── RangeComponent.cs
│   ├── PhaseComponent.cs
│   ├── TimeData.cs
│   └── NormalizedProgress.cs
│
├── Tags/
│   ├── LinearMovementTag.cs
│   ├── HelixMovementTag.cs
│   ├── ArcMovementTag.cs
│   ├── ReverseDirectionEnableableComponent.cs
│   ├── WithRotationTag.cs
│   ├── WithoutRotationTag.cs
│   ├── TargetEcsLocalTransformTag.cs
│   ├── TargetPhysicsVelocityLinearTag.cs
│   └── TargetPhysicsVelocityAngularTag.cs
│
├── Transforms/
│   ├── StartPositionComponent.cs
│   ├── StartQuaternionComponent.cs
│   ├── EndPositionComponent.cs
│   ├── EndQuaternionComponent.cs
│   ├── UnAssignedPositionComponent.cs
│   └── UnAssignedQuaternionComponent.cs
│
├── Utilities/
│   ├── LinearMovementFacet.cs
│   ├── TargetTransformComponent.cs
│   ├── LinearExtensions.cs
│   ├── PidController.cs
│   └── PreCalculatedPositionsFloat3.cs
│
└── AssemblyInfo.cs
```

## Category Rationale

| Folder | Purpose | Examples |
|--------|---------|----------|
| **Parameters/** | Movement configuration values | Amplitude, Speed, Frequency, Phase |
| **Tags/** | Identifiers and state flags | LinearTag, HelixTag, ReverseDirection |
| **Transforms/** | Position/rotation data | StartPosition, EndPosition |
| **Utilities/** | Helper types and extensions | Facets, PidController, Extensions |

## Namespace Strategy

Keep full namespaces for code organization:
```csharp
namespace Movements.Data.Parameters.Easing
{
    public struct MoveEaseComponent { }
}

namespace Movements.Data.Parameters.Motion
{
    public struct AmplitudeComponent { }
}

namespace Movements.Data.Tags.MovementTypes
{
    public struct LinearMovementTag { }
}

namespace Movements.Data.Utilities.Facets
{
    public struct LinearMovementFacet { }
}
```

**This gives you:**
- ✅ **Simple folder structure** (1 level)
- ✅ **Organized code** (via namespaces)
- ✅ **C# idiomatic names** (no underscores)
- ✅ **Developer & Designer friendly**

## Migration Mapping

### Parameters/
| From | To |
|------|-----|
| `Parameters/Easing/MoveEaseComponent.cs` | `Parameters/MoveEaseComponent.cs` |
| `Parameters/Easing/TimerEaseComponent.cs` | `Parameters/TimerEaseComponent.cs` |
| `Parameters/Easing/PeakBiasComponent.cs` | `Parameters/PeakBiasComponent.cs` |
| `Parameters/Motion/AmplitudeComponent.cs` | `Parameters/AmplitudeComponent.cs` |
| `Parameters/Motion/DirectionComponent.cs` | `Parameters/DirectionComponent.cs` |
| `Parameters/Motion/FrequencyComponent.cs` | `Parameters/FrequencyComponent.cs` |
| `Parameters/Motion/SpeedComponent.cs` | `Parameters/SpeedComponent.cs` |
| `Parameters/Motion/WobbleComponent.cs` | `Parameters/WobbleComponent.cs` |
| `Parameters/Motion/HeightComponent.cs` | `Parameters/HeightComponent.cs` |
| `Parameters/Motion/PitchComponent.cs` | `Parameters/PitchComponent.cs` |
| `Parameters/Motion/RangeComponent.cs` | `Parameters/RangeComponent.cs` |
| `Parameters/Timing/PhaseComponent.cs` | `Parameters/PhaseComponent.cs` |
| `Parameters/Timing/TimeData.cs` | `Parameters/TimeData.cs` |
| `Parameters/Timing/NormalizedProgress.cs` | `Parameters/NormalizedProgress.cs` |

### Tags/
| From | To |
|------|-----|
| `Tags/MovementTypes/LinearMovementTag.cs` | `Tags/LinearMovementTag.cs` |
| `Tags/MovementTypes/HelixMovementTag.cs` | `Tags/HelixMovementTag.cs` |
| `Tags/MovementTypes/ArcMovementTag.cs` | `Tags/ArcMovementTag.cs` |
| `Tags/Modifiers/ReverseDirectionEnableableComponent.cs` | `Tags/ReverseDirectionEnableableComponent.cs` |
| `Tags/Modifiers/WithRotationTag.cs` | `Tags/WithRotationTag.cs` |
| `Tags/Modifiers/WithoutRotationTag.cs` | `Tags/WithoutRotationTag.cs` |
| `Tags/Targets/TargetEcsLocalTransformTag.cs` | `Tags/TargetEcsLocalTransformTag.cs` |
| `Tags/Targets/TargetPhysicsVelocityLinearTag.cs` | `Tags/TargetPhysicsVelocityLinearTag.cs` |
| `Tags/Targets/TargetPhysicsVelocityAngularTag.cs` | `Tags/TargetPhysicsVelocityAngularTag.cs` |

### Transforms/
| From | To |
|------|-----|
| `Transforms/StartEnd/StartPositionComponent.cs` | `Transforms/StartPositionComponent.cs` |
| `Transforms/StartEnd/StartQuaternionComponent.cs` | `Transforms/StartQuaternionComponent.cs` |
| `Transforms/StartEnd/EndPositionComponent.cs` | `Transforms/EndPositionComponent.cs` |
| `Transforms/StartEnd/EndQuaternionComponent.cs` | `Transforms/EndQuaternionComponent.cs` |
| `Transforms/Unassigned/UnAssignedPositionComponent.cs` | `Transforms/UnAssignedPositionComponent.cs` |
| `Transforms/Unassigned/UnAssignedQuaternionComponent.cs` | `Transforms/UnAssignedQuaternionComponent.cs` |

### Utilities/ (NEW - merged from Advanced)
| From | To |
|------|-----|
| `Advanced/Facets/LinearMovementFacet.cs` | `Utilities/LinearMovementFacet.cs` |
| `Advanced/Targets/TargetTransformComponent.cs` | `Utilities/TargetTransformComponent.cs` |
| `Advanced/Utilities/LinearExtensions.cs` | `Utilities/LinearExtensions.cs` |
| `Advanced/Utilities/Pid.cs` | `Utilities/PidController.cs` (renamed) |
| `Advanced/Utilities/PreCalculatedPositionsFloat3.cs` | `Utilities/PreCalculatedPositionsFloat3.cs` |

## Benefits

| Before | After |
|--------|-------|
| 3-4 folder clicks | 1 folder click |
| "Where was that file?" | "It's in Parameters/" |
| Deep navigation | Flat navigation |
| High cognitive load | Low cognitive load |
| "Advanced?" What's that? | Clear category names |

## Implementation Steps

1. [ ] Create backup of current structure
2. [ ] Create `Utilities/` folder
3. [ ] Move files from `Advanced/` subfolders to `Utilities/`
4. [ ] Move files from `Parameters/` subfolders to `Parameters/`
5. [ ] Move files from `Tags/` subfolders to `Tags/`
6. [ ] Move files from `Transforms/` subfolders to `Transforms/`
7. [ ] Rename `Pid.cs` → `PidController.cs` for clarity
8. [ ] Update namespaces (keep full depth for code organization)
9. [ ] Remove `Advanced/` folder and all empty subdirectories
10. [ ] Verify Unity compiles
11. [ ] Test all systems

---

**Result:** Clean, simple, 1-level folder structure with 4 clear categories.
