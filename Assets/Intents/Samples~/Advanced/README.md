# Advanced Intents Samples

This folder contains advanced examples demonstrating complex intent patterns including chaining, composition, priority queues, and state machine integration.

## Samples

### 1. IntentChainExample.cs

Demonstrates sequential intent execution with looping support.

**Features:**
- NativeArray-based intent chains
- Sequential execution with index tracking
- Looping support for repeated patterns
- Chain reset functionality

**How to Use:**
1. Attach to a GameObject
2. Configure chain size and looping in Inspector
3. Press SPACE to execute next intent in chain
4. Press R to reset chain to start

**Key Concepts:**
- Intent chains for scripted sequences
- Native arrays for performance
- Index tracking and completion detection

---

### 2. CompositeIntentsExample.cs

Shows how to combine multiple intents for complex behaviors.

**Features:**
- Multi-intent execution (Attack + Move + Effect)
- Configurable success requirements (all vs any)
- Intent composition patterns
- Combo system implementation

**How to Use:**
1. Attach to a GameObject with all three intent components
2. Configure success requirement (Require All toggle)
3. Use keys 1-3 for different composite patterns
4. Press SPACE for full composite execution

**Composite Patterns:**
- SPACE: Execute all intents
- 1: Attack + Move composite
- 2: Attack + Effect composite
- 3: Full combo (Attack + Move + Effect)

**Key Concepts:**
- Intent composition and combination
- Success/failure handling strategies
- Parallel vs sequential execution

---

### 3. PriorityQueueExample.cs

Demonstrates priority-based intent selection and execution.

**Features:**
- Priority queue with NativeArray
- Timestamp-based tiebreaking
- Dynamic intent addition
- Queue management (clear, add, execute)

**How to Use:**
1. Attach to a GameObject with all intent components
2. Configure queue size in Inspector
3. Press SPACE to execute highest-priority intent
4. Press A to add high-priority intent
5. Press C to clear queue

**Priority Logic:**
- Higher priority = executed first
- Same priority = newer timestamp wins
- Invalid intents are skipped

**Key Concepts:**
- Priority-based scheduling
- Timestamp tiebreaking
- Dynamic queue management
- Native array performance

---

### 4. StateMachineIntegration.cs

Shows how to integrate intents with a state machine.

**Features:**
- Four states (Idle, Moving, Combat, Effect)
- State-specific intent execution
- Automatic and manual transitions
- Intent-state synchronization

**How to Use:**
1. Attach to a GameObject with all intent components
2. Configure state delays in Inspector
3. Use keys 1-4 for manual state transitions
4. Use state-specific actions (SPACE, Mouse buttons)

**State Actions:**
- Idle: Auto-transition to Moving after delay
- Moving: SPACE to move, Mouse0 to combat
- Combat: SPACE to attack, Mouse1 to idle
- Effect: SPACE to stop effect

**Key Concepts:**
- State-driven intent execution
- State lifecycle management
- Intent-state synchronization
- Transition patterns

---

## Setup Instructions

For each sample:

1. Create a new scene in `Samples~/Advanced/Scenes/`
2. Create an empty GameObject
3. Add the sample script
4. Add required intent components:
   - AttackComponent
   - MoveComponent
   - EffectComponent
5. Configure component and script settings
6. Enter Play mode and test

---

## Learning Goals

After completing these samples, you should understand:

- **Intent Chaining**: Sequential execution patterns
- **Intent Composition**: Combining multiple intents
- **Priority Queues**: Priority-based execution
- **State Integration**: Intent-state synchronization
- **Native Arrays**: Performance-critical data structures
- **Advanced Patterns**: Complex gameplay behaviors

---

## Architecture Patterns

### Chain of Responsibility
```csharp
if (chain.TryExecuteNext(out var intent))
{
    ProcessIntent(intent);
}
```

### Composite Pattern
```csharp
var success = TryExecuteAll(); // Combines multiple intents
return moveSuccess && attackSuccess && effectSuccess;
```

### Priority Queue
```csharp
if (queue.TryGetHighest(out var intent))
{
    ExecuteIntent(intent);
}
```

### State Machine
```csharp
switch (_currentState)
{
    case State.Combat:
        HandleCombatState();
        break;
}
```

---

## Performance Considerations

- **NativeArrays**: Use for intent storage in hot paths
- **Structs**: Pass by ref to avoid copying
- **Guard Clauses**: Early exit on invalid states
- **Burst Compilation**: Ready for ECS implementation

---

## Next Steps

Once comfortable with Advanced samples, proceed to:
- **ECS Samples**: DOTS implementation with Jobs/Burst
- **Custom Patterns**: Create your own intent systems
