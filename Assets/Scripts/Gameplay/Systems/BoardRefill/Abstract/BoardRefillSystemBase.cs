using System;
using System.Collections;
using System.Collections.Generic;
using Core.Grid.Abstract;
using Core.Runner.Interface;
using Gameplay.Tile.Abstract;
using UnityEngine;

namespace Gameplay.Systems.BoardRefill.Abstract
{
    /// <summary>
    /// Represents the base class for a board refill system, responsible for refilling the board
    /// with tiles and managing the refill process.
    /// </summary>
    public abstract class BoardRefillSystemBase : IDisposable
    {
        protected readonly GridSystemBase GridSystem;
        protected readonly TileManagerBase TileManager;
        protected readonly ICoroutineRunner CoroutineRunner;
        
        private event Action onRefillCompleted;
        public event Action OnRefillCompleted
        {
            add => onRefillCompleted += value;
            remove => onRefillCompleted -= value;
        }

        protected BoardRefillSystemBase(
            GridSystemBase gridSystem, 
            TileManagerBase tileManager, 
            ICoroutineRunner coroutineRunner)
        {
            GridSystem = gridSystem;
            TileManager = tileManager;
            CoroutineRunner = coroutineRunner;
        }

        public void StartRefill(List<TileBase> tiles)
        {
            CoroutineRunner.StartCoroutine(WaitForRefillComplete(tiles));
        }

        private IEnumerator WaitForRefillComplete(List<TileBase> tiles)
        {
            yield return CoroutineRunner.StartCoroutine(Refill(tiles));
            
            Debug.Log("Refill completed.");
            onRefillCompleted?.Invoke();
        }

        protected abstract IEnumerator Refill(List<TileBase> tiles);
        
        public void Dispose()
        {
            onRefillCompleted = null;
        }
    }
}