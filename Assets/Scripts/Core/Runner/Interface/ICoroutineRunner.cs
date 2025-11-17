using System.Collections;
using UnityEngine;

namespace Core.Runner.Interface
{
    /// <summary>
    /// Provides an interface for managing Unity coroutines.
    /// </summary>
    public interface ICoroutineRunner
    {
        Coroutine StartCoroutine(IEnumerator routine);
        void StopCoroutine(Coroutine routine);
    }
}