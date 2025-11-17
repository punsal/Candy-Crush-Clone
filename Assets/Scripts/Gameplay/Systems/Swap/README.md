# Swap System

The Swap system is responsible for taking **two selected tiles** and performing a **Candy-Crush-style swap** between them:

1. Receive a pair of tiles from the `SelectSystem` (via `GameManager`).
2. Attempt to swap them (animation + logical state).
3. Ask `MatchDetectionSystem` if the swap created any matches.
4. If a match exists:
   - Let `BoardRefillSystem` handle destruction, gravity, and refill.
5. If no match:
   - Revert the swap back to the original positions.

The Swap system does **not** decide what to destroy or when to refill; it only manages the swap / revert operations.

---

## Core Concepts

### SwapSystemBase
```
csharp
public abstract class SwapSystemBase : IDisposable
{
public abstract event Action<TileBase, TileBase> OnSwapCompleted;
public abstract event Action OnRevertCompleted;

    public abstract void StartSwap(TileBase first, TileBase second);
    public abstract void StartRevert(TileBase first, TileBase second);
    public abstract void Dispose();
}
```
Responsibilities:

- Defines the contract for swapping and reverting tiles.
- Provides events:
  - `OnSwapCompleted(tile1, tile2)` – fired when a swap animation + logical update is done.
  - `OnRevertCompleted()` – fired when a revert finishes.
- Does not know about:
  - Matches
  - Refills
  - Cascades

---

## SwapSystem (Concrete)
```
csharp
public class SwapSystem : SwapSystemBase
{
// Holds GridSystem reference for position conversions,
// and a CoroutineRunner for animations.
}
```
Typical behaviour for `StartSwap`:

1. Validate the inputs:
   - Both tiles are non-null.
   - Both tiles have valid `Cell` references.
   - (Optionally) Only allow adjacent tiles (or let GameManager enforce this).
2. Temporarily disable player input (handled by GameManager).
3. Perform swap:
   - Animate the two tiles moving to each other’s positions.
   - Update:
     - `TileBase.Cell` references.
     - Grid occupancy in `GridSystem` (remove + add occupant).
4. When the animation and logical swap complete:
   - Raise `OnSwapCompleted(tile1, tile2)`.

Typical behaviour for `StartRevert`:

1. Animate the tiles back to their original cells.
2. Restore their `Cell` and grid occupancy.
3. Raise `OnRevertCompleted()` when done.

---

## Integration in the Game Loop

### 1. From Selection to Swap

`GameManager` subscribes to `SelectSystem.OnSelectionCompleted`:
```
csharp
_selectSystem.OnSelectionCompleted += HandleSelectCompleted;

private void HandleSelectCompleted(ISelectable firstSelectable, ISelectable lastSelectable)
{
// Disable input while swap is in progress
_inputHandler.Disable();

    // Convert ISelectable to TileBase and initiate the swap
    _swapSystem.StartSwap(firstSelectable as TileBase, lastSelectable as TileBase);
}
```
### 2. After Swap

`GameManager` listens to `OnSwapCompleted`:
```
csharp
_swapSystem.OnSwapCompleted += HandleSwapCompleted;

private void HandleSwapCompleted(TileBase tile1, TileBase tile2)
{
// Check if swap created any matches
if (_matchDetectionSystem.TryGetMatch(out var matchTiles))
{
// Start destruction + refill
_boardRefillSystem.StartRefill(matchTiles);
}
else
{
// No match: revert
_swapSystem.StartRevert(tile1, tile2);
}
}
```
### 3. After Revert
```
csharp
_swapSystem.OnRevertCompleted += HandleRevertCompleted;

private void HandleRevertCompleted()
{
// Simply re-enable input; board state is unchanged
_inputHandler.Enable();
}
```
The Swap system itself never decides whether a given swap is “good” or “bad”; it just executes the swap and reports completion. `GameManager` and `MatchDetectionSystem` decide what happens next.

---

## Interaction with Other Systems

- **SelectSystem**:
  - Produces the pair of tiles to swap.
  - SwapSystem is invoked once a valid selection pair is completed.

- **MatchDetectionSystem**:
  - After `OnSwapCompleted`, GameManager queries:
    - `TryGetMatch(out tiles)` to see if the swap created a match.
  - SwapSystem does not talk to MatchDetectionSystem directly.

- **BoardRefillSystem**:
  - Triggered only if a match is found after a swap.
  - SwapSystem does not handle destruction, gravity, or refill.

- **Input System**:
  - Disabled before a swap starts.
  - Re-enabled:
    - After revert completes, or
    - After refills and cascades finish and the board is stable.

---

## Design Notes

- **Single Responsibility**:
  - SwapSystem is the “animation and state update” layer for tile swaps and reverts.
  - It does not contain match/refill logic.
- **Animation-Friendly**:
  - Implementations typically use coroutines and `ICoroutineRunner` to animate swaps smoothly.
- **Extensibility**:
  - You can later add:
    - Special swap rules (e.g., disallow diagonal, allow special bonuses).
    - Visual feedback (swap sound, particle effect).
    - Different swap speeds or easing curves per tile type.