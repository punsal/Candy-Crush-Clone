# Match Detection System

The Match Detection system is responsible for all **match-3 logic** in the game:

- Detecting **existing matches** (lines of 3+ tiles).
- Determining if the board has **any possible swap move** left.
- Supporting **cascading matches** after refills.

It operates purely on the logical board state (grid + tiles), without knowing anything about input, selection, or visuals.

---

## Responsibilities

### 1. Existing Matches (`TryGetMatch`)
```
csharp
bool TryGetMatch(out List<TileBase> tiles)
```
- Scans the board for **Candy-Crush-style matches**:
  - Horizontal runs: 3+ tiles of the same `TileType` in a row.
  - Vertical runs: 3+ tiles of the same `TileType` in a column.
- Returns:
  - `true` if at least one match exists.
  - `tiles` containing **all tiles** that are part of any match (rows and columns combined, with duplicates removed).

The GameManager and BoardRefill logic use this to:

- Destroy matched tiles.
- Trigger gravity and refills.
- Cascade: keep resolving while `TryGetMatch` continues to find matches after each refill.

---

### 2. Possible Moves (`HasPossibleMoves`)
```
csharp
bool HasPossibleMoves()
```
- Answers the question:  
  **“Is there at least one swap of two adjacent tiles that would create a match?”**
- Algorithm:
  1. Iterate all grid positions.
  2. For each tile, test swaps with:
     - Right neighbor `(row, col + 1)`
     - Down neighbor `(row + 1, col)`
  3. For each candidate swap:
     - Temporarily swap the two tiles’ `TileType`.
     - Check whether either of the swapped positions now belongs to a horizontal or vertical run of 3+.
     - Revert the swap.
  4. If any such swap produces a match, return `true`. Otherwise, `false`.

This is used to:

- Validate the **initial board** (must have at least one move).
- Decide when to **shuffle** the board:
  - If `HasPossibleMoves()` is `false`, the board is “dead” and must be reshuffled.

---

## Architecture

### Base Class
```
csharp
abstract class MatchDetectionSystemBase
{
protected readonly GridSystemBase GridSystem;
protected readonly TileManagerBase TileManager;

    public abstract bool HasPossibleMoves();
    public abstract bool TryGetMatch(out List<TileBase> tiles);
}
```
- **`GridSystemBase`** provides board dimensions (`RowCount`, `ColumnCount`).
- **`TileManagerBase`** provides tile access:
  - `FindTileAt(row, col)` → `TileBase`
- The base class defines the public contract but leaves implementation to concrete systems.

### Concrete Implementation
```
csharp
class MatchDetectionSystem : MatchDetectionSystemBase
{
// Implements:
// - TryGetMatch: row/column scan for 3+ in a line
// - HasPossibleMoves: swap simulation with neighbors
}
```
Internally it uses:

- **Line scanning**:
  - For each row: walk columns and collect runs of same-type tiles.
  - For each column: walk rows and collect runs.
- **Local line checks**:
  - To test if a tile is part of a match, count same-type tiles:
    - Left and right (horizontal).
    - Up and down (vertical).

---

## How It Fits Into the Game Loop

High-level flow with `GameManager`:

1. **Board creation**
   - Fill grid with random tiles.
   - If `TryGetMatch` finds matches → shuffle.
   - If `HasPossibleMoves` is false → shuffle.
   - Otherwise → enable player input.

2. **After a swap**
   - If `TryGetMatch` returns matched tiles:
     - Pass them to `BoardRefillSystem.StartRefill(...)`.
   - Else:
     - Revert the swap.

3. **After refill completes**
   - If `TryGetMatch` finds new matches:
     - Start another refill (**cascade**).
   - Else if `HasPossibleMoves` is false:
     - Trigger a shuffle (board is dead).
   - Else:
     - Re-enable input.

---

## Design Notes

- **Match shape**: Only straight lines (horizontal/vertical). No L-shapes or T-shapes are treated specially; they are just overlapping line matches.
- **Board-agnostic**: The system:
  - Does not handle input, animation, or tile destruction.
  - Only reads tile types and positions via `TileManagerBase` + `GridSystemBase`.
- **Performance**:
  - Line scanning is O(R * C) for matches.
  - Possible-move detection is O(R * C) * small constant (only right/down swaps, local checks only).

This makes the system easy to reason about and extend when you later introduce:

- Special tiles (bombs, stripes, etc.).
- Different match rules.
- Additional board shapes or sizes.