# Candy Crush–Style Match‑3 (Unity)

A small, architecture‑focused **Candy Crush–style match‑3 prototype** built in Unity.

The project emphasizes clean separation of concerns:

- **Grid** (cells & layout)
- **Tiles** (matchable pieces)
- **Selection + Swap** (input → swap pair)
- **Match Detection** (Candy‑Crush line matches)
- **Board Refill** (destroy → gravity → refill)
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
   - Fill it with random tiles.
   - Ensure:
     - No initial matches.
     - At least one valid swap (`HasPossibleMoves()`).

2. **Player interaction**
   - Click/tap tile A → selected.
   - Drag to tile B:
     - If adjacent → attempt swap.
     - Otherwise → update selection.

3. **After a swap**
   - If the swap creates any match (horizontal/vertical 3+):
     - Destroy matched tiles.
     - Apply gravity.
     - Refill board.
     - Cascade:
       - If new matches appear after refill, repeat the same process.
   - If no match:
     - Revert the swap.

4. **Dead‑board handling**
   - After the board is stable:
     - If there are no possible moves:
       - Shuffle tiles.
       - Validate again.
     - Otherwise:
       - Wait for the next player input.

This is essentially a minimal, testable “Candy Crush core” without the UI and scoring layers.

---

## Project Layout

Relevant folders under `Assets/Scripts`:

### Core

- **`Core/Grid`**  
  Grid and cells.
  - `GridSystemBase`, `GridSystem` – 2D grid of `ICell`.
  - `CellBase`, `ICell` – MonoBehaviour cell with row/column + world position.

- **`Core/Select`**  
  Selection model (what the player has clicked).
  - `ISelectable`, `SelectableBase` – objects that can be selected (tiles).
  - `SelectSystemBase`, `SelectSystem` – tracks first & second selection, raises `OnSelectionCompleted`.

- **`Core/Camera`**  
  Camera abstraction and centering on grid.

- **`Core/Runner`**  
  - `ICoroutineRunner` – implemented by `GameManager` to run gameplay coroutines.

### Gameplay

- **`Gameplay/Tile`**  
  Tiles and their manager.
  - `TileBase` – base tile type; integrates with selection and animation.
  - `BasicTile` – simple matchable tile with `MatchType`.
  - `TileManagerBase`, `TileManager` – spawn, track, and destroy tiles.

- **`Gameplay/Input`**  
  Input glue.
  - `InputHandlerBase` – platform‑agnostic enable/disable & update.
  - `MouseInputHandler` / `TouchInputHandler` – mouse or touch → selection.

- **`Gameplay/Systems/Swap`**  
  Swap orchestration.
  - `SwapSystemBase`, `SwapSystem` – perform swap/revert between two tiles, raising:
    - `OnSwapCompleted`
    - `OnRevertCompleted`

- **`Gameplay/Systems/MatchDetection`**  
  Match‑3 logic.
  - `MatchDetectionSystemBase`, `MatchDetectionSystem`
  - `MatchType` enum
  - Responsibilities:
    - `TryGetMatch(out tiles)` – find all horizontal/vertical lines of 3+ same‑type tiles.
    - `HasPossibleMoves()` – simulate swaps with neighbors to see if any move can create a match.

- **`Gameplay/Systems/BoardRefill`**  
  Destroy → gravity → refill.
  - `BoardRefillSystemBase`, `BoardRefillSystem`
  - Given a list of matched tiles:
    - Play destruction.
    - Drop tiles down (gravity).
    - Spawn new tiles into empty cells.
    - Raise `OnRefillCompleted`.

- **`Gameplay/Systems/Shuffle`**  
  No‑move recovery.
  - `ShuffleSystemBase`, `ShuffleSystem`
  - Randomly reassign tiles to cells while keeping board full, then raise `OnShuffleCompleted`.

- **`GameManager`**  
  The game’s “conductor”.
  - Creates all systems.
  - Wires events between:
    - Input → Select → Swap
    - Swap → MatchDetection → BoardRefill → MatchDetection (cascade) → Shuffle
  - Manages the main loop:
    - Initialization
    - Cascades
    - Shuffles
    - Replay when shuffle recovery fails too many times.

---

## How to Run

1. **Open in Unity**
   - Open the project in Unity **2022.3.x** (or compatible LTS).
2. **Open the scene**
   - `Assets/Scenes/GameScene.unity`
3. **Play**
   - Press **Play** in the editor.

### Controls

- **Desktop / Editor**
  - Left‑click on a tile to select it.
  - Left‑click drag on an **adjacent** tile to attempt a swap.
- **Mobile (if built)**
  - Tap/drag tiles similarly (driven by `TouchInputHandler`).

If the swap results in a match, tiles are destroyed and the board refills with gravity and cascades.
If not, the tiles slide back to their original positions.

---

## Design Goals

This repository is structured for **reviewability and extensibility**:

- **Separation of concerns**
  - Each system focuses on one responsibility (selection, swap, match detection, refill, shuffle).
- **Testability**
  - Core logic (MatchDetection, TileManager, GridSystem) is decoupled from Unity UI and can be tested in isolation.
- **Clarity over cleverness**
  - Algorithms are intentionally straightforward (row/column scanning, swap simulation) to make the behaviour obvious in code.

---

## Possible Extensions (Future Work)

The current codebase is a solid base for:

- Score system:
  - Points per tile, per cascade, or per special tile.
- Turn or move limits:
  - Classic “X moves to reach Y score” levels.
- Special tiles:
  - Striped, wrapped, bombs, color clears.
- Level goals:
  - Clear specific tiles, reach target scores, drop items, etc.
- UI and feedback:
  - HUD, animations, SFX, particle systems.
- Persistence & meta:
  - Level maps, progression, save/load.

The core loop is intentionally kept lean so these features can be layered on top without rewriting the gameplay foundation.

---

## For Reviewers

If you’re reviewing this repository, useful starting points are:

- `Assets/Scripts/GameManager.cs` – overview of system orchestration.
- `Assets/Scripts/Gameplay/Systems/MatchDetection` – line‑based match‑3 and possible‑move detection.
- `Assets/Scripts/Gameplay/Systems/BoardRefill` – gravity + refill logic.
- `Assets/Scripts/Gameplay/Tile` – tile abstraction and management.
- `Assets/Scripts/Core/Select` + `Gameplay/Systems/Swap` – how input becomes a swap and how swaps are validated.

Each major folder includes its own `README.md` with more detail about its responsibilities and interactions.