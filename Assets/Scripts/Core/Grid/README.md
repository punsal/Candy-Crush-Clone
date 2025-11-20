# Grid System

A high-performance, interface-driven grid system for managing cells and their occupants.

To support the project's **<2ms logic budget**, this system uses internal **O(1) lookup maps** for spatial queries instead of iterative searches.

---

## Folder Structure

-   **Abstract/**
  -   `GridSystemBase.cs` – Core logic (Validation, Lookup maps).
-   **Cell/**
  -   `CellBase.cs` / `GridCell.cs` – Visual representation of cells.
  -   `ICell.cs` – Pure data contract (Coordinates).
-   **Interface/**
  -   `ICellOccupant.cs` – Contract for objects on the grid (Tiles).
-   `GridSystem.cs` – Concrete spawner/manager.

---

## Key Performance Architecture

### 1. O(1) Spatial Lookups
Previously, `GetCellOccupant` required iterating a list of all active objects (O(N)).
**Current Implementation:**
-   `GridSystemBase` maintains a private `ICellOccupant[,] _occupantMap`.
-   **`GetCellOccupant(cell)`**: Returns `_occupantMap[row, col]` instantly.
-   **`TryGetEmptyCell`**: Scans the array directly without allocation.

### 2. Strict Sync Contract
Because the Grid System owns the "Truth" of where things are, occupants must be removed correctly:

1.  **Remove from Grid**: Call `GridSystem.RemoveOccupant(occupant)`.
  -   The system uses `occupant.Cell` to clear the O(1) map entry.
2.  **Release Cell**: Call `occupant.Release()`.
  -   The occupant forgets its cell.

*If you Release() before Removing, the GridSystem falls back to a slower O(N) scan to find and clear the ghost reference.*

---

## Core Interfaces

### ICell
Pure data container. Does **not** know about its occupant (separation of concerns).
-   `Row`, `Column`: Immutable coordinates.
-   `Position`: World space vector.

### ICellOccupant
An object that sits on the grid.
-   `ICell Cell { get; }`: The cell it thinks it occupies.
-   `Occupy(cell)` / `Release()`: Updates internal state.

---

## GridSystemBase API

```csharp
// Initialization
void Initialize();
void Dispose();

// Queries (All O(1))
ICell GetCellAt(int row, int column);
ICellOccupant GetCellOccupant(ICell cell);
bool TryGetEmptyCell(out ICell cell);

// Management
void AddOccupant(ICellOccupant occupant);
void RemoveOccupant(ICellOccupant occupant);
```

---

## Usage Example

### Initialization
```csharp
var grid = new GridSystem(6, 6, cellPrefab);
grid.Initialize();
```

### Adding an Occupant
```csharp
var tile = _pool.Spawn(prefab, ...);
var cell = grid.GetCellAt(0, 0);

tile.Occupy(cell);       // Tile knows it is at (0,0)
grid.AddOccupant(tile);  // Grid maps (0,0) -> Tile
```

### Moving an Occupant (Teleport)
```csharp
// 1. Remove from old pos
grid.RemoveOccupant(tile);
tile.Release();

// 2. Add to new pos
tile.Occupy(newCell);
grid.AddOccupant(tile);
```

### Querying
```csharp
// Fast check: Is there something at (2, 3)?
var cell = grid.GetCellAt(2, 3);
var occupant = grid.GetCellOccupant(cell); // Instant array access

if (occupant is TileBase tile)
{
    // Found it!
}
```