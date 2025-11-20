using System;
using Core.Pool.Interface;
using UnityEngine;

namespace Core.Pool.Abstract
{
    public abstract class PoolSystemBase : IPoolSystem, IDisposable
    {
        public abstract void Prewarm<T>(T original, int count) where T : Component;
        public abstract T Spawn<T>(T original, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component;
        public abstract void Despawn<T>(T instance) where T : Component;
        public abstract void Clear();

        public virtual void Dispose()
        {
            Clear();
        }
    }
}