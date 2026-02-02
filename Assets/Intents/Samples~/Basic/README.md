# Basic Intents Samples

This folder contains fundamental examples demonstrating the Intent system with MonoBehaviour integration.

## Samples

### 1. BasicIntentExample.cs

Demonstrates the simplest possible intent execution flow.

**Features:**
- Single intent system (Attack)
- Direct interface calls
- Basic cooldown management
- Debug visualization

**How to Use:**
1. Attach to a GameObject
2. Add an AttackComponent
3. Press SPACE to execute attack
4. See Gizmos for range visualization

---

### 2. IntentStateExample.cs

Shows how to manage intent lifecycles across multiple systems.

**Features:**
- Three separate intent systems (Attack, Move, Effect)
- Independent state management
- Multiple interface implementations
- On-screen status display

**How to Use:**
1. Attach to a GameObject
2. Add AttackComponent, MoveComponent, EffectComponent
3. Use Q/E/R/T keys to control each system
4. Watch state changes in Inspector and GUI

**Key Bindings:**
- Q: Move forward
- E: Execute attack
- R: Play effect
- T: Stop effect

---

### 3. MultipleIntentsExample.cs

Demonstrates combining multiple intents for complex gameplay behaviors.

**Features:**
- Intent sequencing and chaining
- Combo systems (Attack + Effect)
- Mouse raycasting for targeting
- Multiple input systems

**How to Use:**
1. Attach to a GameObject
2. Add all three intent components
3. Left-click to attack
4. Press F to move, G to play effect
5. See Gizmos for range and direction

**Key Bindings:**
- Left Click: Attack + Effect combo
- F: Move forward
- G: Play effect

---

## Setup Instructions

For each sample:

1. Create a new scene in `Samples~/Basic/Scenes/`
2. Create an empty GameObject
3. Add the sample script
4. Add required intent components (Attack, Move, Effect)
5. Configure component settings in Inspector
6. Enter Play mode and test

---

## Learning Goals

After completing these samples, you should understand:

- **Intent Creation**: How to define intent data structures
- **Interface Implementation**: How to use explicit interfaces
- **System Integration**: How to bridge to Unity
- **State Management**: How intent states work
- **Basic Usage**: TryX pattern and guard clauses

---

## Next Steps

Once comfortable with Basic samples, proceed to:
- **Advanced Samples**: Intent chaining, composition, priorities
- **ECS Samples**: DOTS implementation with Jobs/Burst
