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
        
        private readonly ICellOccupant[,] _occupantMap;
        private readonly HashSet<ICellOccupant> _occupants; 
        
        protected GridSystemBase(int rowCount, int columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            Cells = new ICell[rowCount, columnCount];
            
            _occupantMap = new ICellOccupant[rowCount, columnCount];
            _occupants = new HashSet<ICellOccupant>();
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

            // Find the first empty tile without creating a full list
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j < ColumnCount; j++)
                {
                    if (_occupantMap[i, j] != null)
                    {
                        continue;
                    }
                    cell = Cells[i, j];
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

            _occupantMap[occupant.Cell.Row, occupant.Cell.Column] = occupant;
            _occupants.Add(occupant);
        }
        
        // Defensive check to ensure that the tile is in the grid
        private bool IsCellInGrid(ICell cell)
        {
            if (cell.Row < 0 || cell.Row >= RowCount || cell.Column < 0 || cell.Column >= ColumnCount)
                return false;
        
            return Cells[cell.Row, cell.Column] == cell;
        }

        public ICellOccupant GetCellOccupant(ICell cell)
        {
            return cell == null 
                ? null 
                : _occupantMap[cell.Row, cell.Column];
        }

        public void RemoveOccupant(ICellOccupant occupant)
        {
            if (occupant == null) return;

            // if the cell is known, clear the map directly
            if (occupant.Cell != null)
            {
                var r = occupant.Cell.Row;
                var c = occupant.Cell.Column;
                if (r >= 0 && r < RowCount && c >= 0 && c < ColumnCount)
                {
                    if (_occupantMap[r, c] == occupant)
                    {
                        _occupantMap[r, c] = null;
                    }
                }
            }
            // if the cell is released, find and clear from the map
            else 
            {
                ClearOccupantFromMap(occupant);
            }

            _occupants.Remove(occupant);
        }

        private void ClearOccupantFromMap(ICellOccupant occupant)
        {
            for (var r = 0; r < RowCount; r++)
            {
                for (var c = 0; c < ColumnCount; c++)
                {
                    if (_occupantMap[r, c] != occupant)
                    {
                        continue;
                    }
                    _occupantMap[r, c] = null;
                    return;
                }
            }
        }
    }
}