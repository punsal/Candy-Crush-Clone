# Board Refill System

The Board Refill system is responsible for restoring the board to a **fully filled** state after tiles are destroyed:

1. Visually destroy matched tiles.
2. Apply **gravity** to let tiles fall into empty cells.
3. Spawn new tiles to fill remaining gaps.
4. Notify when the refill is complete.

It works hand-in-hand with:

- `MatchDetectionSystem` (to find matches before/after refills)
- `TileManager` (to spawn/destroy/manage tiles)
- `GridSystem` (to know board dimensions and cell positions)

---

## Core Responsibilities

### 1. Start Refill
```
csharp
void StartRefill(List<TileBase> tilesToDestroy)
```
Called by `GameManager` when a set of tiles should be removed (typically after a successful swap and match detection).

Flow:

1. **Visual destruction**:
   - Play destruction animations/effects on the tiles (if implemented on `TileBase`).
2. **Logical destruction**:
   - Remove the tiles from `TileManager`.
   - Free their `Cell` in `GridSystem` (via `ICellOccupant`-like logic).
3. **Gravity**:
   - Move tiles down in each column to fill empty spaces.
4. **Spawn**:
   - Spawn new tiles at the top to fill remaining empty cells.

When all steps are complete, the system raises `OnRefillCompleted`.

---

## BoardRefillSystemBase
```
csharp
public abstract class BoardRefillSystemBase : IDisposable
{
protected readonly GridSystemBase GridSystem;
protected readonly TileManagerBase TileManager;
protected readonly ICoroutineRunner CoroutineRunner;

    public event Action OnRefillCompleted;

    public void StartRefill(List<TileBase> tiles);
    protected abstract IEnumerator Refill(List<TileBase> tiles);
    public void Dispose();
}
```
Responsibilities:

- Owns the **refill coroutine**:
  - Wraps the abstract `Refill` coroutine with timing and completion callbacks.
- Provides:
  - Access to `GridSystemBase` (rows, columns, cells).
  - Access to `TileManagerBase` (active tiles, spawn/destroy).
  - A `CoroutineRunner` (typically `GameManager`) to run coroutines.

Concrete implementations plug into `Refill(...)` to define the exact refill behaviour.

---

## BoardRefillSystem (Concrete)
```
csharp
public class BoardRefillSystem : BoardRefillSystemBase
{
protected override IEnumerator Refill(List<TileBase> tiles)
{
yield return CoroutineRunner.StartCoroutine(DestroyTiles(tiles));
yield return CoroutineRunner.StartCoroutine(ApplyGravity());
yield return CoroutineRunner.StartCoroutine(SpawnNewTiles());
}
}
```
Typical steps:

### 1. DestroyTiles

- Play any visual destruction on the tiles (scale-down, particle effect, etc.).
- Wait briefly to let the effect play.
- Remove tiles:
  - Tell `TileManager` to forget them.
  - Release their cells in `GridSystem`.

### 2. ApplyGravity

For each column:

- Scan from bottom to top:
  - When a tile is found above an empty cell:
    - Move it down to the lowest available empty cell in that column.
    - Update:
      - Tile’s `Cell` reference.
      - Grid occupancy (Add/Remove occupant).
    - Animate movement according to tile’s movement method.

This compacts tiles in each column so that there are no gaps.

### 3. SpawnNewTiles

For each empty cell (bottom to top, column by column):

- Ask `TileManager` to spawn a new tile:
  - Usually above the board, then animated down into place.
- Occupy the target cell and register the tile as active.

A small delay between spawns can be added to create a “drop-in” effect.

---

## Integration in the Game Loop

The `GameManager` coordinates refills and cascading matches:

1. **After a swap**:
   - `MatchDetectionSystem.TryGetMatch` finds tiles to destroy.
   - `BoardRefillSystem.StartRefill(matchedTiles)` is called.

2. **OnRefillCompleted**:
   - `GameManager` checks for **cascading matches**:
     - If `TryGetMatch` finds new matches:
       - Call `StartRefill` again with those tiles.
     - Otherwise:
       - If there are no possible moves → trigger a shuffle.
       - Else → enable input for the next player action.

This means `BoardRefillSystem` itself only cares about **making the board full again**. Logic about when to **stop** refilling or when to **shuffle** lives in `GameManager` + `MatchDetectionSystem`.

---

## Design Notes

- **Single Responsibility**:
  - This system strictly manages visual/logical **refill** after tiles are removed.
  - It does not decide what to destroy or when to stop cascading.
- **Animation-friendly**:
  - Uses coroutines so gravity and spawn can be animated smoothly.
- **Extensibility**:
  - You can customize:
    - Gravity direction (e.g., different board orientations).
    - Refill patterns (e.g., staggered spawns, column-wise delays).
    - Special tile handling (e.g., bombs that clear extra tiles).