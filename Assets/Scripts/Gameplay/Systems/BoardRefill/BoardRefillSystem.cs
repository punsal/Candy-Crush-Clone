using System.Collections;
using System.Collections.Generic;
using Core.Grid.Abstract;
using Core.Runner.Interface;
using Gameplay.Systems.BoardRefill.Abstract;
using Gameplay.Tile.Abstract;
using Unity.Profiling;
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
        // ProfileMarkers for the LOGIC cycle (should be <2ms total)
        private static readonly ProfilerMarker RefillLogicMarker = new("BoardRefill.LogicCycle");
        private static readonly ProfilerMarker ProcessColumnMarker = new("BoardRefill.ProcessColumn");

        private readonly List<TileBase> _movedTiles;
        private readonly List<TileBase> _newlySpawnedTiles;


        public BoardRefillSystem(GridSystemBase gridSystem, TileManagerBase tileManager,
            ICoroutineRunner coroutineRunner) : base(gridSystem, tileManager, coroutineRunner)
        {
            _movedTiles = new List<TileBase>(gridSystem.RowCount * gridSystem.ColumnCount);
            _newlySpawnedTiles = new List<TileBase>(10);
        }

        protected override IEnumerator Refill(List<TileBase> tiles)
        {
            RefillLogicMarker.Begin();
            
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var tile in tiles)
            {
                if (!tile)
                {
                    continue;
                }
                tile.Release();
                GridSystem.RemoveOccupant(tile);
            }
            
            _movedTiles.Clear();
            _newlySpawnedTiles.Clear();
            
            var columnCount = GridSystem.ColumnCount;
            for (var col = 0; col < columnCount; col++)
            {
                ProcessColumnLogic(col);
            }
            
            RefillLogicMarker.End();
            
            yield return CoroutineRunner.StartCoroutine(AnimateRefill(tiles));
        }
        
        private void ProcessColumnLogic(int col)
        {
            ProcessColumnMarker.Begin();
            
            var rowCount = GridSystem.RowCount;
            var writeRow = rowCount - 1; // Start writing from the bottom

            // Scan from bottom to top to find survivors
            for (var readRow = rowCount - 1; readRow >= 0; readRow--)
            {
                var tile = TileManager.FindTileAt(readRow, col);

                if (tile == null)
                {
                    continue;
                }
                if (readRow != writeRow)
                {
                    tile.Release();
                    GridSystem.RemoveOccupant(tile);
                        
                    var targetCell = GridSystem.GetCellAt(writeRow, col);
                    tile.Occupy(targetCell);
                    GridSystem.AddOccupant(tile);
                        
                    _movedTiles.Add(tile);
                }
                    
                writeRow--;
            }
            
            // Any rows remaining above 'writeRow' (inclusive) are empty and need new tiles
            for (var row = writeRow; row >= 0; row--)
            {
                var cell = GridSystem.GetCellAt(row, col);
                var cellOccupant = GridSystem.GetCellOccupant(cell);
                if (cell != null && cellOccupant != null)
                {
                    Debug.LogWarning($"Cell {cell.Name} is occupied by {cellOccupant.Name}");
                    var ghostOccupant = cellOccupant as TileBase;
                    if (ghostOccupant != null)
                    {
                        ghostOccupant.Release();
                    }
                }
                
                var newTile = TileManager.SpawnRandomTileAt(cell);
                
                if (newTile != null)
                {
                    // track it for animation
                    _newlySpawnedTiles.Add(newTile);
                    newTile.gameObject.SetActive(false);
                }
            }
            
            ProcessColumnMarker.End();
        }
        
        private IEnumerator AnimateRefill(List<TileBase> destroyedTiles)
        {
            foreach (var tile in destroyedTiles)
            {
                if (tile) tile.Destroy();
            }
            
            yield return new WaitForSeconds(0.2f);
            
            TileManager.DestroyTiles(destroyedTiles);
            
            var hasGravity = _movedTiles.Count > 0;
            foreach (var tile in _movedTiles)
            {
                if (tile && tile.Cell != null)
                {
                    // Move visual transform to the logical cell position
                    tile.MoveTo(tile.Cell.Position, 0.2f);
                }
            }

            if (hasGravity)
            {
                yield return new WaitForSeconds(0.2f);
            }
            
            foreach (var tile in _newlySpawnedTiles)
            {
                if (tile && tile.Cell != null)
                {
                    tile.gameObject.SetActive(true);
                    var finalPosition = tile.Cell.Position;
                    tile.transform.position = finalPosition + Vector3.up * 5f;
                    
                    tile.MoveTo(tile.Cell.Position, 0.2f);
                }
                yield return new WaitForSeconds(0.05f);
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }
}