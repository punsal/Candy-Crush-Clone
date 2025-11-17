using System;
using System.Collections;
using Core.Runner.Interface;
using Gameplay.Tile.Abstract;
using UnityEngine;

namespace Gameplay.Systems.Swap.Abstract
{
    /// <summary>
    /// Serves as the abstract base class for handling the swapping system between tiles within a game.
    /// Provides the infrastructure for initiating and managing swap operations, revert operations, and
    /// their associated lifecycle events. Intended to be inherited by concrete implementations.
    /// </summary>
    public abstract class SwapSystemBase : IDisposable
    {
        private readonly ICoroutineRunner _coroutineRunner;
        
        private event Action<TileBase, TileBase> onSwapCompleted;
        public event Action<TileBase, TileBase> OnSwapCompleted
        {
            add => onSwapCompleted += value;
            remove => onSwapCompleted -= value;
        }
        
        private event Action onRevertCompleted;
        public event Action OnRevertCompleted
        {
            add => onRevertCompleted += value;
            remove => onRevertCompleted -= value;
        }

        protected SwapSystemBase(ICoroutineRunner coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        public void StartSwap(TileBase tile1, TileBase tile2)
        {
            _coroutineRunner.StartCoroutine(WaitForSwap(tile1, tile2));
        }

        public void StartRevert(TileBase tile1, TileBase tile2)
        {
            _coroutineRunner.StartCoroutine(WaitForRevert(tile1, tile2));
        }

        private IEnumerator WaitForSwap(TileBase tile1, TileBase tile2)
        {
            Debug.Log("Swapping tiles...");
            yield return _coroutineRunner.StartCoroutine(Swap(tile1, tile2));
            Debug.Log("Swap complete");
            onSwapCompleted?.Invoke(tile1, tile2);
        }

        private IEnumerator WaitForRevert(TileBase tile1, TileBase tile2)
        {
            Debug.Log("Reverting tiles...");
            yield return _coroutineRunner.StartCoroutine(Swap(tile1, tile2));
            Debug.Log("Revert complete");
            onRevertCompleted?.Invoke();
        }
        
        protected abstract IEnumerator Swap(TileBase tile1, TileBase tile2);
        
        public void Dispose()
        {
            onSwapCompleted = null;
        }
    }
}