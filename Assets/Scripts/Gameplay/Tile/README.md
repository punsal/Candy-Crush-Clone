# Tile System

The Tile system represents the **playable pieces** on the grid (the "candies").

To support the project's **<2ms logic budget**, this system has been optimized for:
-   **Zero Allocations**: Uses `IPoolSystem` instead of `Instantiate`/`Destroy`.
-   **O(1) Spatial Lookups**: Uses `GridSystem` direct access instead of list iteration.
-   **Instant Logic**: State changes happen immediately; visuals follow asynchronously.

---

## Folder Structure

-   **Abstract/**
  -   `TileBase.cs` – Base class for all tiles.
  -   `TileManagerBase.cs` – Base manager for spawning/tracking tiles.
-   **Components/**
  -   `TileAnimatorComponent.cs` – Handles visual state (Selection, Movement, Destruction).
-   `BasicTile.cs` – Concrete tile implementation.
-   `TileManager.cs` – High-performance manager implementation.

---

## Key Performance Architecture

### 1. Object Pooling (Zero Garbage)
Instead of creating new GameObjects at runtime, `TileManager` uses `Core.Pool.Interface.IPoolSystem`.

-   **Spawn**: Retrieves an inactive tile from the pool.
-   **Despawn**: Deactivates the tile and returns it to the pool.
-   **Prewarm**: During initialization, the manager creates a buffer of tiles (e.g., 10 per color) to ensure the first match cycle is lag-free.

### 2. O(1) Lookups
Previously, finding a tile at `(row, col)` required iterating the `ActiveTiles` list (O(N)).

**Optimized Approach:**
```
csharp
public override TileBase FindTileAt(int row, int col)
{
    // O(1) Direct Access via GridSystem
    var cell = GridSystem.GetCellAt(row, col);
    return GridSystem.GetCellOccupant(cell) as TileBase;
}
```
This removes the bottleneck from `MatchDetectionSystem` and `BoardRefillSystem`.

---

## TileManager
```
csharp
public class TileManager : TileManagerBase
{
    // Dependencies
    private readonly IPoolSystem _poolSystem; 
    
    // ...
}
```
**Responsibilities:**
1.  **Lifecycle**:
  -   `SpawnTileAt`: Gets tile from pool, resets visual state, registers with `GridSystem`.
  -   `DestroyTile`: Unregisters from `GridSystem`, releases cell, returns to pool.
2.  **Prewarming**:
  -   Iterates all tile prefabs on startup and pre-fills the pool.
3.  **Queries**:
  -   Provides fast access to tiles via `FindTileAt`.

---

## TileBase & Components

### TileBase
Inherits from `SelectableBase`. Now includes a `Reset()` method to restore the tile's state (scale, color, alpha) when it is reused from the pool.

### TileAnimatorComponent
Handles the visual decoupling.
-   **Logic Phase**: Tile position is updated instantly in data.
-   **Visual Phase**: `AnimateMovement` tweens the `Transform` to the target position over time.

---

## Integration

### With BoardRefillSystem
-   **Spawning**: `TileManager.SpawnRandomTileAt` is called during the logic phase. The new tile is often spawned `SetActive(false)` so it can be revealed later during the animation phase.
-   **Gravity**: Tiles are moved logically first (`GridSystem` update), then animated visually.

### With MatchDetectionSystem
-   Because `FindTileAt` is now **O(1)**, the detection system can scan the entire board (36 cells) in microseconds without performance cost.