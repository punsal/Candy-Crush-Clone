# Tile System

The Tile system represents the **playable pieces** on the grid (the “candies” in a Candy Crush–style game).

It provides:

- A base tile implementation (`TileBase`) with selection and animation hooks.
- A manager for spawning, tracking, and destroying tiles (`TileManager`).
- Animation components for movement, selection feedback, and destruction.

This layer is responsible for **what** lives on the grid and **how it looks/moves**, not for match logic or refills (those are handled by other systems).

---

## Folder Structure

- **Abstract/**
  - `TileBase.cs` – Base class for all tiles.
  - `TileManagerBase.cs` – Base manager for spawning/destroying tiles.
- **Components/**
  - **Abstract/**
    - `TileAnimatorComponentBase.cs` – Base animation contract for tiles.
  - `TileAnimatorComponent.cs` – Default animation implementation.
- `BasicTile.cs` – Concrete tile implementation (basic matchable tile).
- `TileManager.cs` – Concrete tile manager.

---

## TileBase
```
csharp
public abstract class TileBase : SelectableBase
{
[SerializeField] private TileAnimatorComponentBase animator;

    public abstract MatchType MatchType { get; set; }

    public abstract bool IsTypeMatch(MatchType type);

    protected override void OnSelected();
    protected override void OnUnselected();

    public void Destroy();
    public void MoveTo(Vector3 position, float duration);
}
```
Key points:

- Inherits from `SelectableBase`:
  - Integrates with the **Select system** (click/tap to select).
  - Provides `OnSelected` / `OnUnselected` hooks that trigger visual feedback.
- `MatchType`:
  - Represents the logical type of the tile (e.g., color).
  - Used by `MatchDetectionSystem` to determine line matches.
- Animation hooks:
  - `Destroy()` → delegates to `TileAnimatorComponentBase.AnimateDestruction`.
  - `MoveTo(position, duration)` → delegates to `AnimateMovement`.

`TileBase` itself does not implement matching rules; it just exposes a type and basic helpers used by higher-level systems.

---

## BasicTile
```
csharp
public class BasicTile : TileBase
{
[SerializeField] private MatchType matchType;

    public override MatchType MatchType { get; set; }

    public override bool IsTypeMatch(MatchType type);
    public override bool IsAdjacent(ISelectable other);
}
```
Responsibilities:

- Provides a concrete, simple tile implementation suitable for classic match-3:
  - Stores a `MatchType` field.
  - Implements `IsTypeMatch`:
    - Typically returns `true` if `type == MatchType`, with special handling for `MatchType.Any`.
- Implements **adjacency** in grid terms:
  - `IsAdjacent(other)`:
    - Uses `Cell.Row` / `Cell.Column` from both tiles.
    - Returns `true` if they are horizontally or vertically adjacent
      (`(rowDiff == 1 && colDiff == 0)` or `(rowDiff == 0 && colDiff == 1)`).

This is useful for:

- Validating that a player only swaps adjacent tiles.
- Providing a simple, reusable adjacency concept for selection or swapping logic.

---

## TileAnimatorComponentBase
```
csharp
public abstract class TileAnimatorComponentBase : MonoBehaviour
{
public abstract void PlaySelectEffect();
public abstract void PlayUnselectEffect();
public abstract void AnimateDestruction();
public abstract void AnimateMovement(Vector3 target, float duration);
}
```
Defines the **visual contract** for tile animations:

- Selection feedback (scale, glow, etc.).
- Unselection.
- Destruction.
- Movement (e.g., swapping, falling, refilling).

Concrete implementations can use tweening libraries, custom curves, or Unity coroutines.

---

## TileAnimatorComponent
```
csharp
public class TileAnimatorComponent : TileAnimatorComponentBase
{
[SerializeField] private float selectScaleMultiplier = 1.2f;
[SerializeField] private float destroyDuration = 0.2f;
[SerializeField] private float moveDuration = 0.2f;

    public override void PlaySelectEffect();
    public override void PlayUnselectEffect();
    public override void AnimateDestruction();
    public override void AnimateMovement(Vector3 target, float duration);
}
```
Default implementation using coroutines:

- **Select / Unselect**:
  - Scales the tile up/down using a simple multiplier.
- **Destruction**:
  - Scales the tile down to zero over `destroyDuration`.
- **Movement**:
  - Linearly interpolates position from current to `target` over `moveDuration`.

This is the out-of-the-box visual behaviour for tiles; you can swap it out or subclass it for more complex effects.

---

## TileManagerBase
```
csharp
public abstract class TileManagerBase : IDisposable
{
protected readonly GridSystemBase GridSystem;
private readonly List<TileBase> _tilePrefabs;

    public abstract IReadOnlyList<TileBase> ActiveTiles { get; }

    public abstract void FillGrid();
    protected abstract TileBase SpawnTileAt(ICell cell, TileBase tilePrefab);

    public void SpawnRandomTileAt(ICell cell);

    protected abstract void DestroyTile(TileBase tile);
    protected abstract void DestroyAllTiles();
    protected abstract void CleanupDestroyedTiles();

    public void DestroyTiles(IEnumerable<TileBase> tiles);

    public TileBase FindTileAt(int row, int col);
}
```
Responsibilities:

- **Spawning**:
  - `FillGrid()` fills the board with tiles (implementation in derived class).
  - `SpawnRandomTileAt(cell)` picks a random prefab and delegates to `SpawnTileAt`.
- **Destruction**:
  - `DestroyTiles`:
    - Destroys a collection of tiles (used by `BoardRefillSystem`).
  - `DestroyAllTiles`:
    - Cleans up on disposal.
  - `CleanupDestroyedTiles`:
    - Removes null references from internal tracking.
- **Queries**:
  - `FindTileAt(row, col)`:
    - Returns the tile occupying a specific grid cell (if any).
    - Validates that at most one tile occupies a cell.

`TileManagerBase` knows how to manage tiles at a logical level but not how they look or move.

---

## TileManager
```
csharp
public class TileManager : TileManagerBase
{
public override IReadOnlyList<TileBase> ActiveTiles { get; }

    public override void FillGrid();
    protected override TileBase SpawnTileAt(ICell cell, TileBase tilePrefab);
    protected override void DestroyTile(TileBase tile);
    protected override void DestroyAllTiles();
    protected override void CleanupDestroyedTiles();
}
```
Concrete implementation:

- Uses a `List<TileBase>` as the backing store for `ActiveTiles`.
- **FillGrid**:
  - Repeatedly calls `GridSystem.TryGetEmptyCell(out cell)` and spawns random tiles until the grid is full.
- **SpawnTileAt**:
  - Validates that the target cell is not already occupied.
  - Spawns the tile slightly above the cell (for a drop-in effect).
  - Calls:
    - `tile.Occupy(cell)`
    - `GridSystem.AddOccupant(tile)`
    - `tile.MoveTo(cell.Position, duration)`
- **DestroyTile / DestroyAllTiles**:
  - Removes tiles from `ActiveTiles`.
  - Releases cell occupancy in `GridSystem`.
  - Destroys the tile GameObject.

---

## How Tiles Fit Into the Game

Tiles are used by several systems:

- **MatchDetectionSystem**:
  - Reads `MatchType` and grid positions (`Cell.Row`, `Cell.Column`) to find matches and possible moves.
- **BoardRefillSystem**:
  - Calls `TileManager.DestroyTiles(...)` to remove matched tiles.
  - Calls `TileManager.SpawnRandomTileAt(...)` to refill empty cells.
- **SwapSystem**:
  - Uses tile positions and `MoveTo` to animate swaps.
  - Updates tile `Cell` references and grid occupancy.

The Tile system itself remains focused on **representing, animating, and managing tiles**, while higher-level systems orchestrate gameplay rules.