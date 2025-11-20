using System.Collections.Generic;
using Core.Grid.Abstract;
using Core.Grid.Cell.Interface;
using Core.Pool.Interface;
using Core.Random.Abstract;
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

        public TileManager(
            RandomSystemBase randomSystemBase,
            GridSystemBase gridSystem,
            List<TileBase> tilePrefabs,
            IPoolSystem poolSystem) 
            : base(randomSystemBase, poolSystem, gridSystem, tilePrefabs)
        {
            _activeTiles = new List<TileBase>(gridSystem.RowCount * gridSystem.ColumnCount);
            PreparePool();
        }

        private void PreparePool()
        {
            
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
            var tile = PoolSystem.Spawn(tilePrefab, cell.Position, Quaternion.identity);
            tile.ResetState();
            
            tile.name = $"Tile_{cell.Row}_{cell.Column}";
            tile.Occupy(cell);
            GridSystem.AddOccupant(tile);
            _activeTiles.Add(tile);
            
            return tile;
        }

        protected override void DestroyTile(TileBase tile)
        {
            if (tile == null)
            {
                return;
            }
            
            _activeTiles.Remove(tile);
            
            if (tile.Cell != null)
            {
                GridSystem.RemoveOccupant(tile);
                tile.Release();
            }
            
            PoolSystem.Despawn(tile);
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
                PoolSystem.Despawn(tile);
            }

            _activeTiles.Clear();
        }

        protected override void CleanupDestroyedTiles()
        {
            _activeTiles.RemoveAll(tile => tile == null);
        }
    }
}