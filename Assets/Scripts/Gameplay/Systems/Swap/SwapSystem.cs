using System.Collections;
using Core.Grid.Abstract;
using Core.Runner.Interface;
using Gameplay.Systems.Swap.Abstract;
using Gameplay.Tile.Abstract;
using UnityEngine;

namespace Gameplay.Systems.Swap
{
    /// <summary>
    /// Provides functionality to handle the swapping of tiles on a grid system.
    /// This class is a concrete implementation of <see cref="SwapSystemBase"/>
    /// and defines the behavior for swapping two tiles within the game,
    /// including updating their positions and grid relationships.
    /// </summary>
    public class SwapSystem : SwapSystemBase
    {
        private readonly GridSystemBase _gridSystem;
        
        public SwapSystem(GridSystemBase gridSystem, ICoroutineRunner coroutineRunner) : base(coroutineRunner)
        {
            _gridSystem = gridSystem;
        }

        protected override IEnumerator Swap(TileBase tile1, TileBase tile2)
        {
            var tile1Cell = tile1.Cell;
            var tile2Cell = tile2.Cell;
            
            tile1.Release();
            _gridSystem.RemoveOccupant(tile1);
            tile2.Release();
            _gridSystem.RemoveOccupant(tile2);
            yield return null;
            
            tile1.Occupy(tile2Cell);
            _gridSystem.AddOccupant(tile1);
            tile2.Occupy(tile1Cell);
            _gridSystem.AddOccupant(tile2);
            yield return null;
            
            tile1.MoveTo(tile2Cell.Position, 0.2f);
            tile2.MoveTo(tile1Cell.Position, 0.2f);
            yield return new WaitForSeconds(0.2f);
        }
    }
}