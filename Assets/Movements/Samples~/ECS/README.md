# ECS Intents Samples

This folder contains high-performance Data-Oriented Design (DOD) examples using Unity DOTS with ECS, Jobs, and Burst compilation.

## Samples

### 1. ECSIntentComponents.cs

Defines ECS component data structures for intents.

**Components:**
- `AttackIntent` - Attack state and configuration
- `MoveIntent` - Movement target and parameters
- `EffectIntent` - Effect timing and intensity
- `IntentExecutor` - Execution trigger data
- `IntentChain` - Chain state tracking
- `PriorityTag` - Priority marking

**Key Features:**
- All data as `IComponentData` (structs)
- Cache-efficient layout
- Burst-compatible types
- No managed objects

---

### 2. ECSIntentSystems.cs

Implements intent processing systems with Burst compilation.

**Systems:**
- `AttackIntentSystem` - Handles attack execution and cooldown
- `MoveIntentSystem` - Processes movement to targets
- `EffectIntentSystem` - Manages effect timing
- `IntentChainSystem` - Handles sequential chains

**Key Features:**
- `[BurstCompile]` for optimal performance
- `IJobEntity` for parallel execution
- `EntityCommandBuffer` for structural changes
- Zero GC allocations in hot paths

---

### 3. ECSIntentAuthoring.cs

Authoring components for converting GameObjects to ECS entities.

**Authoring Components:**
- `AttackIntentAuthoring` - Creates attack intent entities
- `MoveIntentAuthoring` - Creates move intent entities
- `EffectIntentAuthoring` - Creates effect intent entities
- `IntentChainAuthoring` - Creates intent chain entities
- `ECSIntentController` - MonoBehaviour controller for testing

**Key Features:**
- Baker pattern for entity creation
- `TransformUsageFlags` for optimization
- EntityManager manipulation from MonoBehaviour
- Debug visualization and controls

---

### 4. ECSJobExample.cs

Advanced job scheduling and parallel execution patterns.

**Jobs:**
- `ProcessIntentsJob` - Multi-intent processing in single job
- `IntentParallelJob` - `IJobParallelFor` demonstration
- `IntentParallelSystem` - SystemBase integration

**Key Features:**
- NativeArray manipulation
- Parallel job scheduling
- Job dependency management
- Batch size optimization

---

## Setup Instructions

### Basic ECS Setup

1. Create a new scene in `Samples~/ECS/Scenes/`
2. Add a `SubScene` (optional for baking)
3. Create an empty GameObject
4. Add authoring components:
   - `AttackIntentAuthoring`
   - `MoveIntentAuthoring`
   - `EffectIntentAuthoring`
5. Add `ECSIntentController` for testing
6. Enter Play mode

### SubScene Workflow (Recommended)

1. Create a `SubScene` in your scene
2. Add authoring components to GameObjects in SubScene
3. Unity automatically bakes entities on scene save
4. Systems run automatically in DOTS world

---

## Key Concepts

### Component Data (IComponentData)

```csharp
public struct AttackIntent : IComponentData
{
    public float Damage;
    public float Range;
    public byte State;
}
```

- Pure data structs
- No logic or methods
- Burst- compatible
- Cache-efficient layout

### SystemBase with Burst

```csharp
[BurstCompile]
public partial struct AttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new AttackJob { }.ScheduleParallel();
    }
}
```

- Burst compilation for native speed
- `IJobEntity` for automatic query generation
- Parallel scheduling for performance

### IJobEntity

```csharp
[BurstCompile]
private partial struct AttackJob : IJobEntity
{
    void Execute(ref AttackIntent intent, in IntentExecutor executor)
    {
        if (executor.ShouldExecute)
        {
            intent.State = 1;
        }
    }
}
```

- Automatic component querying
- Burst- compiled execution
- Ref/in parameters for optimization

---

## Performance Characteristics

### Burst Compiler Benefits

- **10-100x faster** than managed C#
- **Zero GC allocations** in hot paths
- **SIMD optimization** for math operations
- **Native code generation** via LLVM

### Job System Benefits

- **Multi-threading** across all cores
- **Dependency tracking** for safety
- **Batch processing** for cache efficiency
- **Load balancing** automatically

### Memory Layout

- **Sequential layout** for cache efficiency
- **No boxing** of value types
- **Native containers** for large data
- **Archetype chunks** for iteration

---

## Usage Examples

### Execute Move Intent

```csharp
var executor = _entityManager.GetComponentData<IntentExecutor>(_entity);
executor.TargetPosition = transform.position + transform.forward * 5f;
executor.IntentType = 0; // Move
executor.ShouldExecute = true;
_entityManager.SetComponentData(_entity, executor);
```

### Execute Attack Intent

```csharp
var executor = _entityManager.GetComponentData<IntentExecutor>(_entity);
executor.IntentType = 1; // Attack
executor.ShouldExecute = true;
_entityManager.SetComponentData(_entity, executor);
```

### Execute Effect Intent

```csharp
var executor = _entityManager.GetComponentData<IntentExecutor>(_entity);
executor.IntentType = 2; // Effect
executor.ShouldExecute = true;
_entityManager.SetComponentData(_entity, executor);
```

---

## Debugging and Visualization

### Gizmos

```csharp
private void OnDrawGizmosSelected()
{
    Gizmos.color = Color.green;
    Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5f);

    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, 5f);
}
```

### Entity Debugger

Use Unity's Entity Debugger window (`Window > Analysis > Entity Debugger`) to inspect:
- Component values
- System execution
- Query matching
- Archetype storage

### Burst Inspector

Use the Burst Inspector (`Window > Analysis > Burst Inspector`) to view:
- Generated native code
- Optimization applied
- SIMD usage
- Register allocation

---

## Best Practices

### 1. Always Use Burst

```csharp
[BurstCompile(CompileSynchronously = true)]
public partial struct MySystem : ISystem { }
```

### 2. Ref Parameters for Mutation

```csharp
void Execute(ref AttackIntent intent) // Correct
void Execute(AttackIntent intent) // Wrong: creates copy
```

### 3. In Parameters for Read-Only

```csharp
void Execute(in LocalTransform transform) // Correct
void Execute(LocalTransform transform) // Less efficient
```

### 4. Avoid Managed Types

```csharp
public string Name; // Wrong: not Burst-compatible
public FixedString64Bytes Name; // Correct: Burst-compatible
```

### 5. Use Type-Specific Math

```csharp
math.distance(a, b); // Correct: Burst-aware
UnityEngine.Vector3.Distance(a, b); // Wrong: managed call
```

---

## Learning Goals

After completing these samples, you should understand:

- **ECS Architecture**: Component-System separation
- **Burst Compilation**: Native code generation
- **Job System**: Multi-threaded execution
- **Authoring**: GameObject-to-Entity conversion
- **EntityManager**: Runtime entity manipulation
- **Type Safety**: Compile-time query validation

---

## Next Steps

1. **Profile**: Use Unity Profiler to measure performance
2. **Optimize**: Apply Burst to all systems
3. **Scale**: Test with thousands of entities
4. **Customize**: Create your own intent systems
5. **Integrate**: Combine with MonoBehaviour systems

---

## Resources

- [Unity DOTS Documentation](https://docs.unity3d.com/Packages/com.unity.entities@latest)
- [Burst Compiler Documentation](https://docs.unity3d.com/Packages/com.unity.burst@latest)
- [Job System Documentation](https://docs.unity3d.com/Manual/JobSystemTroubleshooting.html)
