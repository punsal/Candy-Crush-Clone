using System.Collections.Generic;
using Core.Pool.Abstract;
using UnityEngine;

namespace Core.Pool
{
    public class PoolSystem : PoolSystemBase
    {
        // Key: Prefab Instance ID -> Stack of pooled instances
        private readonly Dictionary<int, Stack<Component>> _pools;
        
        // Key: Instance Instance ID -> Prefab Instance ID (to know where to return it)
        private readonly Dictionary<int, int> _instanceToPrefabId;

        private readonly Transform _poolRoot;

        public PoolSystem(string poolRootName = "ObjectPool")
        {
            _poolRoot = new GameObject(poolRootName).transform;
            Object.DontDestroyOnLoad(_poolRoot);
            
            _pools = new Dictionary<int, Stack<Component>>();
            _instanceToPrefabId = new Dictionary<int, int>();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            if (_poolRoot != null)
            {
                Object.Destroy(_poolRoot.gameObject);
            }
        }

        public override void Prewarm<T>(T original, int count)
        {
            if (original == null || count <= 0) return;

            var prefabId = original.GetInstanceID();

            // Ensure the stack exists
            if (!_pools.ContainsKey(prefabId))
            {
                _pools[prefabId] = new Stack<Component>();
            }

            var stack = _pools[prefabId];

            for (var i = 0; i < count; i++)
            {
                var instance = Object.Instantiate(original, Vector3.zero, Quaternion.identity, _poolRoot);
                instance.gameObject.SetActive(false);

                // Track the relationship so Despawn knows where to put it back
                _instanceToPrefabId[instance.GetInstanceID()] = prefabId;
                
                stack.Push(instance);
            }
        }

        public override T Spawn<T>(T original, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (original == null)
            {
                Debug.LogError("Cannot pool null prefab");
                return null;
            }

            var prefabId = original.GetInstanceID();
            T instance = null;

            if (_pools.TryGetValue(prefabId, out var stack) && stack.Count > 0)
            {
                var pooledObj = stack.Pop();
                if (pooledObj != null)
                {
                    instance = pooledObj as T;
                }
            }

            // Create new if needed
            if (instance == null)
            {
                instance = Object.Instantiate(original, position, rotation, parent);
                _instanceToPrefabId[instance.GetInstanceID()] = prefabId;
            }
            else
            {
                // reset
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.transform.SetParent(parent);
                instance.gameObject.SetActive(true);
            }

            return instance;
        }

        public override void Despawn<T>(T instance)
        {
            if (instance == null) return;

            var instanceId = instance.GetInstanceID();

            if (_instanceToPrefabId.TryGetValue(instanceId, out var prefabId))
            {
                instance.gameObject.SetActive(false);
                instance.transform.SetParent(_poolRoot);

                if (!_pools.ContainsKey(prefabId))
                {
                    _pools[prefabId] = new Stack<Component>();
                }

                _pools[prefabId].Push(instance);
            }
            else
            {
                // Not a pooled object (or tracking lost), just destroy it
                Object.Destroy(instance.gameObject);
            }
        }

        public override void Clear()
        {
            foreach (var stack in _pools.Values)
            {
                while (stack.Count > 0)
                {
                    var obj = stack.Pop();
                    if (obj != null)
                    {
                        Object.Destroy(obj.gameObject);
                    }
                }
            }
            _pools.Clear();
            _instanceToPrefabId.Clear();
            
            if (_poolRoot != null)
            {
                Object.Destroy(_poolRoot.gameObject);
            }
        }
    }
}