# Pool System

A generic, high-performance object pooling system for Unity. This module is designed to eliminate runtime memory allocations and garbage collection spikes by reusing `GameObjects` instead of repeatedly instantiating and destroying them.

It is particularly critical for ensuring gameplay logic (such as match-3 refills) stays within strict performance budgets (e.g., <2ms).

## Structure

- **Interface (`IPoolSystem`)**: Defines the contract for pooling operations.
- **Abstract (`PoolSystemBase`)**: Provides a base implementation and `IDisposable` support.
- **Implementation (`PoolSystem`)**: The concrete, generic implementation using `Dictionary` and `Stack`.

---

## 1. IPoolSystem (Interface)

Located in: `Core.Pool.Interface`

Defines the essential operations for any pooling system.
```
csharp
public interface IPoolSystem
{
    /// <summary>
    /// Creates a buffer of inactive instances for a specific prefab.
    /// Call this during initialization to prevent lag spikes during the first spawn.
    /// </summary>
    void Prewarm<T>(T original, int count) where T : Component;

    /// <summary>
    /// Retrieves an object from the pool. If none exist, a new one is created.
    /// </summary>
    T Spawn<T>(T original, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component;

    /// <summary>
    /// Returns an object to the pool to be reused later.
    /// </summary>
    void Despawn<T>(T instance) where T : Component;

    /// <summary>
    /// Destroys all pooled objects and cleans up resources.
    /// </summary>
    void Clear();
}
```
---

## 2. Usage Example

### Initialization & Prewarming
Initialize the system in your Game Manager or Bootstrap class. Prewarming is recommended for frequently used objects (like tiles or projectiles).
```
csharp
// 1. Create the system
IPoolSystem _poolSystem = new PoolSystem("MyGamePool");

// 2. Prewarm commonly used prefabs (e.g., 10 of each tile type)
foreach (var tilePrefab in tilePrefabs)
{
    _poolSystem.Prewarm(tilePrefab, 10);
}
```
### Spawning Objects
Replace `Object.Instantiate` with `_poolSystem.Spawn`. The system automatically handles activating the object and setting its transform.
```
csharp
// Old: var tile = Instantiate(prefab, pos, rot);
var tile = _poolSystem.Spawn(prefab, targetPosition, Quaternion.identity);

// Optional: Reset specific logic on the object
tile.ResetVisuals();
```
### Despawning Objects
Replace `Object.Destroy` with `_poolSystem.Despawn`. The object is deactivated and hidden, ready for reuse.
```
csharp
// Old: Destroy(tile.gameObject);
_poolSystem.Despawn(tile);
```
### Cleanup
When unloading the scene or destroying the game session, clear the pool to free memory.
```
csharp
_poolSystem.Clear();
// or via Dispose pattern if using PoolSystemBase
// _poolSystem.Dispose();
```
---

## 3. How It Works

1.  **Prefab Identity**: The system uses `prefab.GetInstanceID()` as a key. This allows you to pool multiple variants of the same class (e.g., `RedTile`, `BlueTile`) without mixing them up.
2.  **Tracking**: When an object is spawned, its Instance ID is mapped back to its original Prefab ID. This allows `Despawn(instance)` to know exactly which pool stack the object belongs to.
3.  **Hierarchy**: Pooled objects are kept under a persistent root GameObject (default: `"ObjectPool"`) marked with `DontDestroyOnLoad`.

## Performance Benefits

-   **Zero Allocation Spawns**: Reusing an object avoids memory allocation and Garbage Collection overhead.
-   **Instant "Instantiation"**: Enabling a GameObject is significantly faster than `Instantiate()`.
-   **<2ms Logic Cycles**: Essential for ensuring game logic runs instantly without frame drops during heavy operations like board refills.