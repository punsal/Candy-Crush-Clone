using System;
using System.Collections;
using Core.Grid.Abstract;
using Core.Runner.Interface;
using Gameplay.Tile.Abstract;
using UnityEngine;

namespace Gameplay.Systems.Shuffle.Abstract
{
    /// <summary>
    /// Base class for shuffling tiles on the grid.
    /// </summary>
    public abstract class ShuffleSystemBase : IDisposable
    {
        protected readonly GridSystemBase GridSystem;
        protected readonly TileManagerBase TileManager;
        private readonly ICoroutineRunner _coroutineRunner;
        
        private event Action onShuffleCompleted;
        public event Action OnShuffleCompleted
        {
            add => onShuffleCompleted += value;
            remove => onShuffleCompleted -= value;
        }
        
        protected ShuffleSystemBase(
            GridSystemBase gridSystem, 
            TileManagerBase tileManager, 
            ICoroutineRunner coroutineRunner)
        {
            GridSystem = gridSystem;
            TileManager = tileManager;
            _coroutineRunner = coroutineRunner;
        }

        public void StartShuffle()
        {
            _coroutineRunner.StartCoroutine(WaitForShuffle());
        }

        private IEnumerator WaitForShuffle()
        {
            Debug.Log("Shuffling board...");
            yield return _coroutineRunner.StartCoroutine(Shuffle());
            Debug.Log("Shuffle complete");
            onShuffleCompleted?.Invoke();
        }

        protected abstract IEnumerator Shuffle();

        public void Dispose()
        {
            onShuffleCompleted = null;
        }
    }
}