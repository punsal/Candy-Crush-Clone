# Shuffle System

The Shuffle system is responsible for **rearranging tiles on the board** when there are **no possible moves** left (or when the initial board is invalid).

In a Candy-Crush-style game, a shuffle is used to:

- Avoid **dead boards** (no swap can create a match).
- Fix bad initial boards (existing matches on spawn or no moves).
- Keep the game flowing without restarting the level manually.

---

## Core Responsibilities

### 1. Shuffling the Board
```
csharp
public abstract class ShuffleSystemBase : IDisposable
{
protected readonly GridSystemBase GridSystem;
protected readonly TileManagerBase TileManager;
protected readonly ICoroutineRunner CoroutineRunner;

    public int ShuffleCount { get; }
    public event Action OnShuffleCompleted;

    public void StartShuffle();
    protected abstract IEnumerator Shuffle();
    public void Dispose();
}
```
Responsibilities:

- Owns the **shuffle coroutine**:
  - Wraps the abstract `Shuffle()` implementation.
  - Tracks the number of shuffles performed (`ShuffleCount`).
  - Raises `OnShuffleCompleted` when the shuffle finishes.
- Operates on:
  - `GridSystemBase` – board dimensions & cells.
  - `TileManagerBase` – currently active tiles.
  - `ICoroutineRunner` – usually the `GameManager`.

Concrete `ShuffleSystem` implements the actual shuffling behaviour.

---

## ShuffleSystem (Concrete)
```
csharp
public class ShuffleSystem : ShuffleSystemBase
{
protected override IEnumerator Shuffle()
{
// 1. Collect all active tiles with valid cells.
// 2. Release them from their cells (grid occupancy).
// 3. Randomize assignment of tiles to cells.
// 4. Animate tiles moving to their new positions.
// 5. Wait for animations to complete.
}
}
```
Typical algorithm:

1. **Gather tiles**:
   - Collect all `TileBase` instances that:
     - Are not null.
     - Have a valid `Cell` (row/column position).

2. **Detach from grid**:
   - Temporarily “release” each tile from its cell:
     - Clear their occupant registration in `GridSystem`.
     - Clear their internal cell reference or prepare for reassignment.

3. **Randomize positions**:
   - Build a list of all the original cells.
   - Shuffle that list (e.g., using `OrderBy(Random.value)` or Fisher–Yates).
   - Pair each tile with a new cell from the shuffled list.

4. **Reassign + animate**:
   - For each tile–cell pair:
     - Assign the tile to the new cell (update `Cell` and grid occupants).
     - Animate the tile moving to `cell.Position`.
   - Optionally yield between moves to create a staggered effect.

5. **Wait**:
   - After all tiles are moved, wait a short duration to let animations finish.

When `Shuffle()` completes, `ShuffleSystemBase`:

- Increments `ShuffleCount`.
- Invokes `OnShuffleCompleted`.

---

## Integration with the Game Loop

The `GameManager` decides **when** to shuffle:

1. **Initial board**:
   - After filling:
     - If `MatchDetectionSystem.TryGetMatch` finds existing matches → shuffle.
     - If `MatchDetectionSystem.HasPossibleMoves()` is false → shuffle.
   - This ensures:
     - No matches on spawn.
     - At least one valid move.

2. **After cascades/refills**:
   - In `HandleRefillCompleted`:
     - If there are no more matches (`TryGetMatch` is false) **and**
     - `HasPossibleMoves()` is false:
       - Start a shuffle.

3. **Shuffle attempt limit**:
   - `GameManager` can use `ShuffleCount` and a configured `shuffleCountBeforeFailure` to:
     - Retry shuffling a limited number of times.
     - Restart the game if a valid board cannot be produced.

Example flow:
```
csharp
private void HandleShuffleCompleted()
{
_currentShuffleCount++;

    if (!_matchDetectionSystem.HasPossibleMoves())
    {
        if (_currentShuffleCount < shuffleCountBeforeFailure)
        {
            _shuffleSystem.StartShuffle();
        }
        else
        {
            // Too many failed shuffles – restart the game
            Replay();
        }
    }
    else
    {
        // Board is valid again – enable input
        _inputHandler.Enable();
    }
}
```
---

## Design Notes

- **Board-agnostic**:
  - ShuffleSystem does not decide which tiles to destroy or when; it only rearranges existing tiles.
- **Match-3–aware (via GameManager)**:
  - The system itself does not run match detection.
  - `GameManager` and `MatchDetectionSystem` validate the shuffled board:
    - No immediate matches on initial load.
    - At least one possible swap move.
- **Extensibility**:
  - You can later:
    - Bias shuffles to avoid breaking partially completed goals.
    - Add visual effects specifically for shuffles.
    - Implement “smart” shuffles that guarantee at least one good match near the player’s last move.