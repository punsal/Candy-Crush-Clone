# Grid System

A grid-based system for managing **cells** and their **occupants** in a 2D Unity game.  
Interface-driven design makes it flexible, testable, and easy to extend.

---

## Folder Structure

- **Abstract/**
  - `GridSystemBase.cs` – Abstract grid management logic
- **Cell/**
  - **Abstract/**
    - `CellBase.cs` – Base MonoBehaviour cell with position and lifecycle
  - **Interface/**
    - `ICell.cs` – Core cell contract
  - `GridCell.cs` – Concrete visual cell
- **Interface/**
  - `ICellOccupant.cs` – Contract for objects that occupy cells
- `GridSystem.cs` – Concrete grid implementation

---

## Core Concepts

### ICell
Contract for all grid cells.

**Properties**
- `Name` – Safe identifier (empty if destroyed)
- `Row`, `Column` – Grid coordinates
- `Position` – World position (`Vector3`)

**Methods**
- `Initialize(row, column)`
- `Destroy()`
- `Highlight()`
- `Conceal()`

Use `ICell` to write grid logic that doesn’t depend on concrete MonoBehaviours.

---

### CellBase & GridCell

**CellBase**
- Abstract MonoBehaviour implementing `ICell`
- Tracks destruction state
- Provides:
  ```csharp
  Awake → OnAwake() (abstract hook)
  Initialize(row, column)
  Destroy()
  ```

**GridCell**
- Inherits `CellBase`
- Uses a `SpriteRenderer` for visual feedback:
  ```csharp
  [Header("Visuals")]
  [SerializeField] private SpriteRenderer visual;

  [Header("VFX")]
  [SerializeField] private Color highlightColor = Color.green;
  ```
- `Highlight()` → sets `visual.color` to `highlightColor`
- `Conceal()` → sets `visual.color` to `Color.white`

---

### ICellOccupant

Represents anything that can sit on a cell (chips, pieces, etc.).

**Contract**
- `ICell Cell { get; }`
- `void Occupy(ICell cell)`
- `void Release()`

Typical lifecycle:
```
csharp
occupant.Occupy(cell);
grid.AddOccupant(occupant);

// ...

occupant.Release();
grid.RemoveOccupant(occupant);
```
---

### GridSystemBase & GridSystem

**GridSystemBase**
- Manages a 2D array of `ICell` and a list of occupants.

**Key Members**
- `int RowCount`, `int ColumnCount`
- `ICell GetCellAt(int row, int column)`
- `bool TryGetEmptyCell(out ICell cell)`
- `void AddOccupant(ICellOccupant occupant)`
- `void RemoveOccupant(ICellOccupant occupant)`
- `void Initialize()` / `void Dispose()` (abstract)

Uses a `HashSet` internally to quickly find empty cells and validates:
- Null occupants
- Duplicate occupants
- Cell ownership (cell must belong to this grid)
- `Release()` must be called before `RemoveOccupant()`

**GridSystem**
- Concrete grid using a `CellBase` prefab.

Constructor:
```
csharp
public GridSystem(int rowCount, int columnCount, CellBase cellPrefab)
```
Initialization:
```
csharp
for (int row = 0; row < RowCount; row++)
{
    for (int col = 0; col < ColumnCount; col++)
    {
        ICell cell = Object.Instantiate(_cellPrefab, new Vector3(col, -row, 0), Quaternion.identity);
        cell.Initialize(row, col);
        Cells[row, col] = cell;
    }
}
```
Disposal iterates all cells and calls `Destroy()`.

---

## Basic Usage

### Creating and Using a Grid
```
csharp
// Create and initialize grid
var cellPrefab = Resources.Load<GridCell>("Prefabs/GridCell");
var grid = new GridSystem(rowCount: 8, columnCount: 8, cellPrefab);
grid.Initialize();

// Access and highlight a cell
var cell = grid.GetCellAt(row: 3, column: 5);
if (cell != null)
{
    cell.Highlight();
}
```
### Placing an Occupant
```
csharp
if (grid.TryGetEmptyCell(out var cell))
{
    occupant.Occupy(cell);
    grid.AddOccupant(occupant);

    cell.Highlight();
}
```
### Cleanup
```
csharp
grid.Dispose(); // Destroys all cell GameObjects
```
---

## Design Highlights

- **Interfaces first**: `ICell` and `ICellOccupant` decouple logic from Unity components.
- **Template pattern**: `GridSystemBase` / `GridSystem`, `CellBase` / `GridCell`.
- **Safety**:
    - Bounds checks in `GetCellAt`
    - Enforced `Release()` before `RemoveOccupant()`
    - Destruction state tracking in `CellBase`
- **Testability**:
    - You can mock `ICell` and `ICellOccupant` in pure C# tests.