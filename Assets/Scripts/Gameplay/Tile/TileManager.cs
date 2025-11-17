using System.Collections.Generic;
using System.Linq;
using Core.Grid.Abstract;
using Core.Grid.Cell.Interface;
using Gameplay.Tile.Abstract;
using UnityEngine;

namespace Gameplay.Tile
{
    /// <summary>
    /// Manages the spawning and destruction of tiles within a grid system.
    /// </summary>
    public class TileManager : TileManagerBase
    {
        private readonly List<TileBase> _activeTiles;

        public override IReadOnlyList<TileBase> ActiveTiles => _activeTiles;

        public TileManager(GridSystemBase gridSystem, List<TileBase> tilePrefabs) : base(gridSystem, tilePrefabs)
        {
            _activeTiles = new List<TileBase>();
        }

        public override void FillGrid()
        {
            while (GridSystem.TryGetEmptyCell(out var tile))
            {
                SpawnRandomTileAt(tile);
            }
        }

        protected override TileBase SpawnTileAt(ICell cell, TileBase tilePrefab)
        {
            var existingTile = _activeTiles.FirstOrDefault(c => c != null && c.Cell == cell);
            if (existingTile != null)
            {
                Debug.LogError($"Tile at ({cell.Row}, {cell.Column}) is already occupied by {existingTile.name}!");
                return existingTile;
            }
            
            var spawnPosition = new Vector3(
                cell.Position.x, 
                cell.Position.y + GridSystem.RowCount, 
                0);
            
            var tile = Object.Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
            tile.name = $"Tile_{cell.Row}_{cell.Column}";

            tile.Occupy(cell);
            GridSystem.AddOccupant(tile);
            _activeTiles.Add(tile);
            Debug.Log($"Spawned tile at ({cell.Row}, {cell.Column}). Total active: {_activeTiles.Count}");

            tile.MoveTo(cell.Position, 0.2f);
            
            return tile;
        }

        protected override void DestroyTile(TileBase tile)
        {
            if (tile == null)
            {
                Debug.LogWarning("Cannot destroy null tile");
                return;
            }

            Debug.Log($"Destroying tile {tile.name} at ({tile.Cell?.Row}, {tile.Cell?.Column}). Before: {_activeTiles.Count}");
            
            _activeTiles.Remove(tile);
            tile.Release();
            GridSystem.RemoveOccupant(tile);
            Object.Destroy(tile.gameObject);
            
            Debug.Log($"After destroy: {_activeTiles.Count}");
        }

        protected override void DestroyAllTiles()
        {
            // Iterate backwards to avoid issues with list modification
            for (var i = _activeTiles.Count - 1; i >= 0; i--)
            {
                var tile = _activeTiles[i];
                if (tile == null)
                {
                    continue;
                }

                tile.Release();
                GridSystem.RemoveOccupant(tile);
                Object.Destroy(tile.gameObject);
            }

            _activeTiles.Clear();
        }

        protected override void CleanupDestroyedTiles()
        {
            var nullCount = _activeTiles.RemoveAll(tile => tile == null);
            if (nullCount > 0)
            {
                Debug.LogWarning($"Cleaned up {nullCount} null references");
            }
            
            Debug.Log($"After cleanup: {_activeTiles.Count} active tiles");
        }
    }
}