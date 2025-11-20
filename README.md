# Candy Crush–Style Match‑3 (Unity)

A small, architecture‑focused **Candy Crush–style match‑3 prototype** built in Unity.

The project emphasizes clean separation of concerns and strict performance targets (**<2ms Logic Cycle**):

- **Grid** (O(1) lookup system)
- **Tiles** (Pooled, zero-allocation)
- **Selection + Swap** (input → swap pair)
- **Match Detection** (Candy‑Crush line matches)
- **Board Refill** (Logic/Visual split, Column Compaction)
- **Shuffle** (no‑move recovery)

UI, score, and meta‑game systems have been intentionally removed to keep the core gameplay loop easy to read and review.

---

## Tech Stack

- **Unity**: 2022.3.x LTS
- **C#**: 9.0 (project target: .NET 4.7.1)
- **Renderer**: Built‑in pipeline
- **IDE**: JetBrains Rider (recommended, but not required)

---

## What This Project Shows

### Core Gameplay Loop

1. **Board initialization**
  - Create a grid of cells.
  - Fill it with pre-warmed pooled tiles.
  - Ensure no initial matches and at least one valid swap.

2. **Player interaction**
  - Click/tap tile A → selected.
  - Drag to tile B:
    - If adjacent → attempt swap.

3. **The Match Cycle (<2ms)**
  - Logic Phase (Instant):
    - **Match**: Detect lines.
    - **Clear**: Remove tiles logically.
    - **Gravity**: Compact columns (O(N)).
    - **Refill**: Spawn new tiles from pool (hidden).
  - Visual Phase (Sequenced):
    - Animate destruction.
    - Animate gravity falls.
    - Drop new tiles in ("Rain effect").

4. **Dead‑board handling**
  - If no moves exist after refill:
    - Shuffle tiles.
    - If shuffle fails repeatedly, restart board.

---

## Project Layout

Relevant folders under `Assets/Scripts`:

### Core

- **`Core/Grid`**  
  Grid and cells.
  - `GridSystem` – Manages `ICell` and `ICellOccupant[,]` map for O(1) lookups.
  - `ICell` – Pure coordinate data.

- **`Core/Pool`**  
  Generic Pooling System.
  - `PoolSystem` – Dictionary-based pool for zero-allocation spawning.
  - Supports `Prewarm` to ensure lag-free start.

- **`Core/Select`**  
  Selection model.
  - `SelectSystem` – Tracks clicked tiles and raises `OnSelectionCompleted`.

### Gameplay

- **`Gameplay/Tile`**  
  Tiles and their manager.
  - `TileManager` – Uses `PoolSystem` to spawn/despawn.
  - `TileBase` – Visuals handled by `TileAnimatorComponent`.

- **`Gameplay/Systems/MatchDetection`**  
  Match‑3 logic.
  - `MatchDetectionSystem` – Fast scanning for lines of 3+.

- **`Gameplay/Systems/BoardRefill`**  
  The optimized refill pipeline.
  - `BoardRefillSystem` – Coordinates the Logic/Visual split.
  - Uses `ProfilerMarker` to track performance.

- **`GameManager`**  
  The game’s “conductor”.
  - Initializes systems.
  - Wires events (Input → Swap → Match → Refill).
  - Enforces Determinism via `RandomSystem`.

---

## Design Goals

This repository is structured for **reviewability and extensibility**:

- **Performance First**
  - **<2ms Logic**: Logic runs instantly; animations play asynchronously.
  - **O(1) Lookups**: No list iterations for spatial queries.
  - **Zero Allocations**: Pooling and pre-allocated buffers.
- **Separation of concerns**
  - Each system focuses on one responsibility.
- **Determinism**
  - All RNG is centralized in `RandomSystem` seeded by `GameManager`.

---

## Case Work Status

### 1. Grid & init
- [x] Lock board size to 6×6.
- [x] Confirm no-starting-matches + at least one valid move.

### 2. APIs
- [x] Add Match type and expose `TryGetMatch`.
- [x] Split BoardRefillSystem: Logic is now distinct from Animation.
- [x] Add `HasPossibleMoves()` API.

### 3. Booster
- [ ] Add a simple *RowClear* or similar booster tile.
- [ ] Integrate booster into FindMatches/Clear.

### 4. Determinism
- [x] Add randomSeed to GameManager.
- [x] Call `RandomSystem` (wrapper) instead of `UnityEngine.Random`.
- [x] Ensure `HasPossibleMoves` doesn’t consume randomness.

### 5. Performance
- [x] Add profiler marker around logic cycle.
- [x] **Profile and ensure < 2 ms**: Achieved ~0.2ms - 0.5ms logic cycle.
- [x] **Remove/avoid allocations**: Implemented Object Pooling and O(1) Grid Lookup.

### 6. Mapping (API to System)

| Required API | Implemented In |
| :--- | :--- |
| `FindMatches` | `MatchDetectionSystem.TryGetMatch` |
| `Clear` | `BoardRefillSystem.StartRefill` (Logic Phase) |
| `ApplyGravity` | `BoardRefillSystem.ProcessColumnLogic` |
| `Refill` | `TileManager.SpawnRandomTileAt` (via Pool) |
| `HasAnyMoves` | `MatchDetectionSystem.HasPossibleMoves` |