using System;
using System.Collections.Generic;
using System.Linq;
using Core.Grid.Cell.Interface;
using Core.Grid.Interface;
using UnityEngine;

namespace Core.Grid.Abstract
{
    /// <summary>
    /// Represents the abstract base class for a grid system, which defines the structure
    /// and behavior of a two-dimensional grid of cells. It provides methods for managing cells
    /// and their occupants within the grid and supports initialization, retrieval, and occupancy management.
    /// </summary>
    public abstract class GridSystemBase : IDisposable
    {
        public int RowCount { get; }
        public int ColumnCount { get; }
        protected ICell[,] Cells { get; }
        private readonly List<ICellOccupant> _occupants;

        protected GridSystemBase(int rowCount, int columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            Cells = new ICell[rowCount, columnCount];
            _occupants = new List<ICellOccupant>();
        }

        public abstract void Initialize();
        public abstract void Dispose();

        public ICell GetCellAt(int row, int column)
        {
            if (row >= 0 && row < RowCount && column >= 0 && column < ColumnCount)
            {
                return Cells[row, column];
            }
            
            Debug.LogWarning($"Cell position ({row}, {column}) is out of bounds");
            return null;
        }
        
        public bool TryGetEmptyCell(out ICell cell)
        {
            cell = null;
    
            // Use HashSet for O(1) lookup instead of O(n) Contains
            var occupiedCells = new HashSet<ICell>(
                _occupants
                    .Select(occupant => occupant.Cell)
                    .Where(t => t != null)
            );

            // Find the first empty tile without creating a full list
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    var currentCell = Cells[i, j];
                    if (currentCell == null || occupiedCells.Contains(currentCell))
                    {
                        continue;
                    }
                    cell = currentCell;
                    return true;
                }
            }

            return false;
        }

        public void AddOccupant(ICellOccupant occupant)
        {
            if (occupant == null)
            {
                Debug.LogError("Occupant cannot be null");
                return;
            }
            
            if (_occupants.Contains(occupant))
            {
                Debug.LogError("Occupant already exists");
                return;
            }

            if (occupant.Cell == null)
            {
                Debug.LogError("Occupant does not have a cell");
                return;
            }
            
            // Validate the cell belongs to this grid
            if (!IsCellInGrid(occupant.Cell))
            {
                Debug.LogError($"Cell at ({occupant.Cell.Row}, {occupant.Cell.Column}) does not belong to this grid");
                return;
            }

            _occupants.Add(occupant);
        }
        
        // Defensive check to ensure that the tile is in the grid
        private bool IsCellInGrid(ICell cell)
        {
            if (cell.Row < 0 || cell.Row >= RowCount || cell.Column < 0 || cell.Column >= ColumnCount)
                return false;
        
            return Cells[cell.Row, cell.Column] == cell;
        }

        public void RemoveOccupant(ICellOccupant occupant)
        {
            if (occupant == null)
            {
                Debug.LogError("Occupant cannot be null");
                return;
            }

            if (!_occupants.Contains(occupant))
            {
                Debug.LogWarning("Occupant does not exist");
                return;
            }

            // Fixed: occupant should have released its cell before removal
            if (occupant.Cell == null)
            {
                _occupants.Remove(occupant);
            }
            else
            {
                Debug.LogError($"Occupant still has a cell at ({occupant.Cell.Row}, {occupant.Cell.Column}). Call Release() first.");
            }
        }
    }
}