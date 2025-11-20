using System;
using System.Collections.Generic;
using System.Linq;
using Core.Grid.Abstract;
using Core.Grid.Cell.Interface;
using Core.Pool.Interface;
using Core.Random.Abstract;
using UnityEngine;

namespace Gameplay.Tile.Abstract
{
    /// <summary>
    /// Provides a base implementation for managing gameplay tiles in a grid system.
    /// This abstract class handles the spawning, destruction, and querying of tiles.
    /// </summary>
    public abstract class TileManagerBase : IDisposable
    {
        protected readonly GridSystemBase GridSystem;
        protected readonly IPoolSystem PoolSystem;
        private readonly RandomSystemBase _randomSystem;
        private readonly List<TileBase> _tilePrefabs;

        public abstract IReadOnlyList<TileBase> ActiveTiles { get; }

        protected TileManagerBase(RandomSystemBase randomSystem, IPoolSystem poolSystem, GridSystemBase gridSystem, List<TileBase> tilePrefabs)
        {
            _randomSystem = randomSystem;
            PoolSystem = poolSystem;
            GridSystem = gridSystem;
            _tilePrefabs = tilePrefabs ?? new List<TileBase>();
        }

        public void Dispose()
        {
            DestroyAllTiles();
        }

        public void Initialize()
        {
            if (_tilePrefabs == null)
            {
                Debug.LogError("No tile prefabs provided");
                return;
            }
            foreach (var tilePrefab in _tilePrefabs)
            {
                PoolSystem.Prewarm(tilePrefab, 10);
            }
        }

        // Spawning
        public abstract void FillGrid();
        protected abstract TileBase SpawnTileAt(ICell cell, TileBase tilePrefab);
        public TileBase SpawnRandomTileAt(ICell cell)
        {
            var randomPrefab = _tilePrefabs[_randomSystem.Next(0, _tilePrefabs.Count)];
            return SpawnTileAt(cell, randomPrefab);
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
            var cell = GridSystem.GetCellAt(row, col);
            if (cell == null) return null;
    
            return GridSystem.GetCellOccupant(cell) as TileBase;
        }
    }
}