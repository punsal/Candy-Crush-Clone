using Core.Grid.Abstract;
using Core.Grid.Cell.Abstract;
using Core.Grid.Cell.Interface;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Grid
{
    /// <summary>
    /// Represents a grid system that manages a two-dimensional array of cells and provides
    /// functionality for initializing and interacting with the cells in the grid. This class
    /// extends the functionality of the GridSystemBase class and uses a predefined cell prefab
    /// to instantiate grid cells.
    /// </summary>
    public class GridSystem : GridSystemBase
    {
        private readonly CellBase _cellPrefab;

        public GridSystem(int rowCount, int columnCount, CellBase cellPrefab) : base(rowCount, columnCount)
        {
            _cellPrefab = cellPrefab;
        }
        
        public override void Initialize()
        {
            if (_cellPrefab == null)
            {
                Debug.LogWarning("Tile prefab is null, will not initialize grid");
                return;
            }
            
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    ICell cell = Object.Instantiate(_cellPrefab, new Vector3(j, -i, 0), Quaternion.identity);
                    cell.Initialize(i, j);
                    cell.Name = $"Cell_{i}_{j}";
                    Cells[i, j] = cell;
                }
            }
        }

        public override void Dispose()
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    var cell = Cells[i, j];
                    if (cell == null)
                    {
                        continue;
                    }
                    cell.Destroy();
                }
            }
        }
    }
}
