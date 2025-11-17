using System;
using System.Collections.Generic;
using System.Linq;
using Core.Grid.Abstract;
using Core.Grid.Cell.Interface;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Tile.Abstract
{
    /// <summary>
    /// Provides a base implementation for managing gameplay tiles in a grid system.
    /// This abstract class handles the spawning, destruction, and querying of tiles.
    /// </summary>
    public abstract class TileManagerBase : IDisposable
    {
        protected readonly GridSystemBase GridSystem;
        private readonly List<TileBase> _tilePrefabs;

        public abstract IReadOnlyList<TileBase> ActiveTiles { get; }

        protected TileManagerBase(GridSystemBase gridSystem, List<TileBase> tilePrefabs)
        {
            GridSystem = gridSystem;
            _tilePrefabs = tilePrefabs ?? new List<TileBase>();
        }

        public void Dispose()
        {
            DestroyAllTiles();
        }

        // Spawning
        public abstract void FillGrid();
        protected abstract TileBase SpawnTileAt(ICell cell, TileBase tilePrefab);
        public void SpawnRandomTileAt(ICell cell)
        {
            var randomPrefab = _tilePrefabs[Random.Range(0, _tilePrefabs.Count)];
            SpawnTileAt(cell, randomPrefab);
        }

        // Destruction
        protected abstract void DestroyTile(TileBase tile);
        protected abstract void DestroyAllTiles();
        protected abstract void CleanupDestroyedTiles();
        public void DestroyTiles(IEnumerable<TileBase> tiles)
        {
            // cache to modify and iterate
            var tileList = tiles.ToList();
            
            Debug.Log($"Destroying {tileList.Count} tiles. Current active: {ActiveTiles.Count}");
            
            foreach (var tile in tileList)
            {
                DestroyTile(tile);
            }
            
            CleanupDestroyedTiles();
        }

        // Queries
        public TileBase FindTileAt(int row, int col)
        {
            TileBase foundTile = null;
            var foundCount = 0;
            
            // Don't use LINQ here, it's too slow
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var tile in ActiveTiles)
            {
                if (!tile || tile.Cell == null || tile.Cell.Row != row || tile.Cell.Column != col)
                {
                    continue;
                }
                foundTile = tile;
                foundCount++;
            }
            
            if (foundCount > 1)
            {
                Debug.LogError($"Multiple tiles ({foundCount}) found at ({row}, {col})!");
            }

            return foundTile;
        }
    }
}