using UnityEngine;

namespace Core.Pool.Interface
{
    public interface IPoolSystem
    {
        void Prewarm<T>(T original, int count) where T : Component;
        T Spawn<T>(T original, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component;
        void Despawn<T>(T instance) where T : Component;
        void Clear();
    }
}