# GameManager: Orchestrator

`GameManager` is the central orchestrator of the gameplay loop. It wires all systems together and controls when input is enabled or disabled.

## System Creation

On `Awake`, `GameManager` creates:

- Camera system
- Grid system
- Select system
- Tile manager
- Board refill system
- Match detection system
- Shuffle system
- Swap system
- Input handler (mouse/touch) bound to the Select system

## Game Setup

On start (`CreateGameplay`):

1. Initialize the grid (`_gridSystem.Initialize()`).
2. Fill the grid with tiles (`_tileManager.FillGrid()`).
3. Center the camera on the grid.
4. Validate the initial board:
   - If there is any existing match (`TryGetMatch`) → shuffle.
   - Else if there is no possible move (`HasPossibleMoves` is false) → shuffle.
   - Else:
     - Board is valid → enable input.

## Event Wiring

`GameManager` subscribes to:

- **Selection**:
  - `SelectSystem.OnSelectionCompleted` → `HandleSelectCompleted`
    - Disables input.
    - Starts a swap via `_swapSystem.StartSwap(...)`.

- **Swap**:
  - `SwapSystem.OnSwapCompleted` → `HandleSwapCompleted`
    - Checks for matches with `TryGetMatch`.
    - If matches exist:
      - Starts refill: `_boardRefillSystem.StartRefill(matchedTiles)`.
    - If no matches:
      - Starts revert: `_swapSystem.StartRevert(tile1, tile2)`.

  - `SwapSystem.OnRevertCompleted` → `HandleRevertCompleted`
    - Re-enables input (board state unchanged).

- **Refill**:
  - `BoardRefillSystem.OnRefillCompleted` → `HandleRefillCompleted`
    - Cascades:
      - If `TryGetMatch` finds new matches → `StartRefill` again.
    - Otherwise:
      - If `HasPossibleMoves` is false → start shuffle.
      - Else → re-enable input.

- **Shuffle**:
  - `ShuffleSystem.OnShuffleCompleted` → `HandleShuffleCompleted`
    - If board still has no possible moves:
      - If shuffle attempts < limit → shuffle again.
      - Else → replay (destroy and recreate all systems, restart gameplay).
    - If board has moves:
      - Re-enable input.

## Gameplay Loop Summary

In terms of state transitions:

1. **Idle / Waiting for Input**
   - Input enabled.
   - Player selects two tiles.
2. **Swapping**
   - Input disabled.
   - SwapSystem animates swap and updates grid.
3. **Post-Swap Check**
   - If match:
     - Destroy → gravity → refill (BoardRefillSystem).
     - On each refill completion, cascade while new matches exist.
   - If no match:
     - Revert swap → back to idle.
4. **Post-Refill Check**
   - If no moves → shuffle (ShuffleSystem).
   - Else → back to idle.
5. **Shuffle**
   - Rearrange tiles, then check `HasPossibleMoves` again.
   - If still dead and attempts exhausted → replay (full restart).

`GameManager` is the only place that knows the **full game lifecycle**. Individual systems (Swap, MatchDetection, BoardRefill, Shuffle) each focus on a single responsibility and communicate via events.

---

## Extensibility

With this structure in place, `GameManager` is the natural place to:

- Introduce **score tracking** (e.g., on each match resolve).
- Add **move limits** (tracking and enforcing turn counts).
- Trigger **UI updates** (score, moves, messages) on events like:
  - Match resolved
  - Refill completed
  - Shuffle started/completed
- Add **level goals** (e.g., collect certain MatchTypes, clear blockers) and win/loss conditions.

Because the core systems are already decoupled and event-driven, new features can be layered on top without major changes to the game loop.