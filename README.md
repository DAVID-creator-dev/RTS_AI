# RTS AI 

## Overview

A real-time strategy project in which two teams fight to destroy the opposing base. One of the two teams is entirely driven by an artificial intelligence structured into three decision-making layers.

## Overall Architecture

The AI relies on a strict separation between strategic decision-making (what to do), planning (in what order), and tactical execution (how each unit behaves on contact). Each layer runs at its own frequency and communicates with the next one.

```
 Utility System (strategy)
 "which goal should we pursue?"
        │
        │  goal queue sorted by priority
        ▼
 GOAP Planner (planning)
 "which actions should we chain?"
        │
        │  plan = [Action, Action, ...]
        ▼
 Tactical FSM (execution)
 "how each unit acts frame by frame"
```

## Layer 1 — Utility System

The first layer determines which goals the AI should pursue at any given moment. Each goal is a `MonoBehaviour` exposing a `RatePriority` method that returns an integer between 0 and 10.

### Input Metrics

The scoring functions draw on several data sources:

- The current `WorldState` (a bitmask of boolean flags).
- The `GameState`, which exposes metrics derived from the game: average army HP ratio, build point ratio, lab counts per owner, enemy base position.
- The `InfluenceMap`, which provides the military presence around a given point (positive value = allied dominance, negative value = enemy dominance).

### AnimationCurve Scoring

No threshold is hardcoded. Each metric is normalized to 0–1, then evaluated by an `AnimationCurve` configurable in the Unity Inspector. The scores from multiple curves are summed, then scaled to 0–10.

### Goal Queue

All goals with a non-zero priority enter a queue sorted by descending score. The queue is recomputed at a fixed interval to adapt to the evolving state of the world.

The objective of this Utility System implementation was to have a first decision layer with access to as much game data as possible, capable of producing varied and coherent decisions without having to add repetitive, hardcoded conditions. Goal scoring matters all the more here because, unlike in a classic GOAP, we don't keep only the top-priority goal.

## Layer 2 — GOAP Planner

When a goal reaches the top of the queue and the troops required to achieve it are available, the GOAP planner generates an action plan for the assigned squad.

The planner dictates the actions taken at medium scale (squad level). It allows a clear plan of action to be distributed efficiently, which we felt was the best choice for a strategy-oriented game, where AI agents must have a clear idea of the actions to take in order to win the match.

### WorldState

The world state is encoded in a single `uint32` bitmask of boolean flags.

Checking whether a goal is satisfied is done with a mask AND:

```csharp
public bool GoalAchieved(WorldState goal)
{
    return (_flags & goal._flags) == goal._flags;
}
```

### Search Algorithm

The planner uses a depth-first forward-chaining search. It enumerates all valid plans and selects the cheapest one. The absence of heuristics and memoization is viable because the action set is deliberately small.

### GOAP Actions

Each action is a `MonoBehaviour` that declares a cost, preconditions (required flags), and effects (produced flags).

## Layer 3 — Tactical FSM

Each unit has its own finite state machine that handles its behavior at the micro level, independently of the GOAP plan currently running. The FSM reacts to local events without escalating to the strategic layer.

The FSM works with States, Conditions and Transitions: the global FSM updates the unit's state when the conditions of one of the transitions from one state to another are met, allowing it to move from one state to another using predefined conditions.

- FSM
- FSMState
- FSMCondition
- FSMTransition

Each unit has a `CurrentOrder` variable and functions assigned to its states. `CurrentOrder` is a `UnitOrder` enum with the values "None, Move, Attack, Repair, Capture", and the unit has the corresponding functions `OrderMove`, `OrderAttack`, etc.

```csharp
public enum UnitOrder
{
    None,
    Move,
    Attack,
    Capture,
    Repair,
}
```

The FSM then reacts to this order and consumes it if it finds the conditions required to move from one state to another. We did this so that a unit can be controlled from any source, whether an AI or a player.

We chose an FSM because it makes it simple to handle the more trivial actions at the level of individual agents, and being able to continuously track the current state of the AIs is also valuable for us.

## Cross-Cutting Systems

### Influence Map

The `InfluenceMap` is a 2D grid where each cell has an influence value: positive for allied dominance, negative for enemy dominance. The `InfluenceMap` `MonoBehaviour` computes nothing itself; it merges several `InfluenceSubMap` `ScriptableObject`s, each responsible for a single source of information:

- `UnitInfluenceSubMap` tracks the units visible on the field and projects their influence over an adjustable radius around them, with an intensity that decreases with distance. When a unit leaves the field of view, its influence is not removed all at once but gradually decays over time (`decayStartTime`, `decayTime`), rather than being instantly "forgotten".
- `EnemyUnitInfluenceSubMap` inherits from the previous one, keeping only enemy units. It is read directly by certain goals (e.g. `DefendBase`) rather than merged into the final map.
- `BuildingInfluenceSubMap` applies the same logic to buildings (`BuildingInfluence`): a building projects a constant zone of influence for as long as it exists.
- `FogOfWarInfluenceSubMap` produces no influence: it bridges to the `FogOfWarSystem` and provides the `IsVisible`/`WasVisible` states used by the other submaps.
- `InformationStalenessSubmap` stores no influence but a cycle number, which makes it possible to know how many updates a cell has gone without being refreshed (used notably by scouting).

On each cycle, `InfluenceMap.MergeGrids` sums the submaps cell by cell (excluding fog, staleness, buildings and the dedicated enemy submap) so that the merged map only retains allied/enemy military presence; this is the value exposed to the Utility System. The submaps are updated over several frames (`updateFrequency`, `SetupStaggeredUpdate`) to spread the CPU cost rather than recomputing everything on the same frame.

The AI reads this data through `AIContext.influenceMap`, using either `GetInfluenceAtPosition` for the overall dominance at a given point, or `GetSubMapInfluenceAtPosition<T>` to query a specific submap (for example the enemy presence around the base to prioritize the "Defend Base" goal).

### Formation Manager

Squads use a `FormationManager` that computes a rectangular grid oriented in the direction of movement. The direction is recomputed from the group's actual center (`GetCenter`) on each order. Slots are distributed according to the units' `TypeId`. Individual pathfinding is delegated to Unity's `NavMeshAgent`s.
