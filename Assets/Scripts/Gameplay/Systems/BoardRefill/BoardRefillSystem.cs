using System.Collections;
using System.Collections.Generic;
using Core.Grid.Abstract;
using Core.Runner.Interface;
using Gameplay.Systems.BoardRefill.Abstract;
using Gameplay.Tile.Abstract;
using UnityEngine;

namespace Gameplay.Systems.BoardRefill
{
    /// <summary>
    /// Represents a system responsible for refilling a board with chips, using a coroutine-based
    /// mechanism for performing the refill operations in sequential steps such as destroying chips,
    /// applying gravity, and spawning new chips.
    /// </summary>
    public class BoardRefillSystem : BoardRefillSystemBase
    {
        public BoardRefillSystem(GridSystemBase gridSystem, TileManagerBase tileManager, ICoroutineRunner coroutineRunner) : base(gridSystem, tileManager, coroutineRunner)
        {
            // empty
        }

        protected override IEnumerator Refill(List<TileBase> tiles)
        {
            yield return CoroutineRunner.StartCoroutine(DestroyChips(tiles));
            yield return CoroutineRunner.StartCoroutine(ApplyGravity());
            yield return CoroutineRunner.StartCoroutine(SpawnNewTiles());
        }

        private IEnumerator DestroyChips(List<TileBase> tiles)
        {
            // Visual effects
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var tile in tiles)
            {
                if (tile)
                {
                    tile.Destroy();
                }
            }
        
            // Wait for effects
            yield return new WaitForSeconds(0.3f);
        
            // Actual destruction
            TileManager.DestroyTiles(tiles);
        
            yield return null;
        }

        private IEnumerator ApplyGravity()
        {
            var columnCount = GridSystem.ColumnCount;
            var rowCount = GridSystem.RowCount;
            
            var anyTileMoved = false;
        
            // Process each column from left to right (col: 0 -> N)
            for (var col = 0; col < columnCount; col++)
            {
                // Process from bottom to top (row: N -> 0)
                for (var row = rowCount - 1; row >= 0; row--)
                {
                    var tileAtPosition = TileManager.FindTileAt(row, col);

                    if (!tileAtPosition)
                    {
                        continue;
                    }
                    var targetRow = FindLowestEmptyRow(row, col);

                    if (targetRow == row)
                    {
                        continue;
                    }
                    yield return CoroutineRunner.StartCoroutine(MoveTileToCell(tileAtPosition, targetRow, col));
                    anyTileMoved = true;
                }
            }
        
            if (anyTileMoved)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        private int FindLowestEmptyRow(int startRow, int col)
        {
            var rowCount = GridSystem.RowCount;
            var lowestEmpty = startRow;
        
            // Check all rows below
            for (var row = startRow + 1; row < rowCount; row++)
            {
                var tileBelow = TileManager.FindTileAt(row, col);
            
                if (!tileBelow)
                {
                    lowestEmpty = row;
                }
                else
                {
                    break;
                }
            }
        
            return lowestEmpty;
        }
        
        private IEnumerator MoveTileToCell(TileBase tile, int targetRow, int targetCol)
        {
            if (!tile || tile.Cell == null)
            {
                yield break;
            }
        
            var targetTile = GridSystem.GetCellAt(targetRow, targetCol);
            if (targetTile == null)
            {
                yield break;
            }
        
            tile.Release();
            GridSystem.RemoveOccupant(tile);
            
            tile.MoveTo(targetTile.Position, 0.2f);
        
            tile.Occupy(targetTile);
            GridSystem.AddOccupant(tile);
            
            yield return new WaitForSeconds(0.2f);
        }

        private IEnumerator SpawnNewTiles()
        {
            var columnCount = GridSystem.ColumnCount;
            var rowCount = GridSystem.RowCount;
            
            // Fill from left to right (col: 0 -> N)
            for (var col = 0; col < columnCount; col++)
            {
                // Fill from bottom to top (row: N -> 0)
                for (var row = rowCount - 1; row >= 0; row--)
                {
                    var tileAtPosition = TileManager.FindTileAt(row, col);

                    if (tileAtPosition)
                    {
                        continue;
                    }
                    var tile = GridSystem.GetCellAt(row, col);

                    if (tile == null)
                    {
                        continue;
                    }
                    TileManager.SpawnRandomTileAt(tile);
                        
                    // Small delay for visual effect
                    yield return new WaitForSeconds(0.05f);
                }
            }
        
            yield return new WaitForSeconds(0.1f);
        }
    }
}