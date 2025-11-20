using System.Collections;
using System.Linq;
using Core.Grid.Abstract;
using Core.Random.Abstract;
using Core.Runner.Interface;
using Gameplay.Systems.Shuffle.Abstract;
using Gameplay.Tile.Abstract;
using UnityEngine;

namespace Gameplay.Systems.Shuffle
{
    /// <summary>
    /// Shuffles tiles on the grid by swapping their positions randomly.
    /// Includes visual animation for the shuffle process.
    /// </summary>
    public class ShuffleSystem : ShuffleSystemBase
    {
        private readonly RandomSystemBase _randomSystem;
        
        public ShuffleSystem(
            GridSystemBase gridSystem, 
            TileManagerBase tileManager,
            ICoroutineRunner coroutineRunner,
            RandomSystemBase randomSystem) 
            : base(gridSystem, tileManager, coroutineRunner)
        {
            _randomSystem = randomSystem;
        }

        protected override IEnumerator Shuffle()
        {
            var tiles = TileManager.ActiveTiles
                .Where(tile => tile != null && tile.Cell != null).
                ToList();
            
            Debug.Log($"Shuffling {tiles.Count} tiles");

            if (tiles.Count == 0)
            {
                Debug.LogWarning("No tiles to shuffle");
                yield break;
            }
            
            var expectedTileCount = GridSystem.RowCount * GridSystem.ColumnCount;
            if (tiles.Count != expectedTileCount)
            {
                Debug.LogWarning($"Tiles count mismatch! Expected: {expectedTileCount}, Got: {tiles.Count}");
                var duplicates = tiles
                    .GroupBy(tile => (tile.Cell.Row, tile.Cell.Column))
                    .Where(g => g.Count() > 1)
                    .ToList();
                
                if (duplicates.Any())
                {
                    Debug.LogError($"Found {duplicates.Count} duplicate positions:");
                    foreach (var dup in duplicates)
                    {
                        Debug.LogError($"Position ({dup.Key.Row}, {dup.Key.Column}): {dup.Count()} tiles");
                    }
                }
            }
            
            var originalTiles = tiles.Select(tile => tile.Cell).ToList();
            
            foreach (var tile in tiles)
            {
                tile.Release();
                GridSystem.RemoveOccupant(tile);
            }

            var shuffledTiles = originalTiles.OrderBy(x => _randomSystem.FloatValue).ToList();

            for (var i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                var newTile = shuffledTiles[i];

                tile.Occupy(newTile);
                GridSystem.AddOccupant(tile);
                
                tile.MoveTo(newTile.Position, 0.3f);
                
                yield return null;
            }

            yield return new WaitForSeconds(0.35f);
        }
    }
}