# Board Refill System

The Board Refill system is responsible for restoring the board to a **fully filled** state after tiles are destroyed.

To meet strict performance requirements (**<2ms logic cycle**), this system uses a **Split-Phase Architecture**:
1.  **Logic Phase (Instant)**: Updates all data, moves, and spawns synchronously in a single frame.
2.  **Animation Phase (Sequenced)**: Plays visual effects (explosions, gravity falls, drops) over time.

It works hand-in-hand with:
-   `MatchDetectionSystem` (to find matches)
-   `TileManager` (to pool/spawn/despawn tiles)
-   `GridSystem` (to manage cell occupancy)

---

## Core Responsibilities

### 1. Start Refill
```
csharp
void StartRefill(List<TileBase> tilesToDestroy)
```
Called by `GameManager` when a match is found.

**Flow:**
1.  **Logic (Instant)**:
    -   Logically removes matched tiles from the `GridSystem`.
    -   Calculates gravity using **Column Compaction** (O(N)).
    -   Updates grid data for falling tiles immediately.
    -   **Spawns new tiles** from the `PoolSystem` into empty slots (initially hidden).

2.  **Visuals (Coroutine)**:
    -   Plays destruction effects.
    -   Animates existing tiles falling to their new logical positions.
    -   Reveals new tiles and animates them dropping in ("Rain effect").

When visuals are complete, the system raises `OnRefillCompleted`.

---

## Architecture: Logic vs Visuals

The key innovation is that **Game Data** is updated instantly, while **Game Objects** catch up visually.

### 1. Logic Phase (The "<2ms" part)
Instead of iterative gravity (bubble sort), we use **Column Compaction**:
-   Iterate each column from bottom to top.
-   "Read" existing tiles and "Write" them to the lowest available slot.
-   If a tile moves, we update the `GridSystem` lookup map immediately.
-   Empty slots at the top are filled by spawning new tiles from the Pool.
    -   *Note:* These new tiles are `SetActive(false)` initially to hide them during the gravity animation.

### 2. Visual Phase
-   **Destruction**: Waits for explosion FX.
-   **Gravity**: Tiles move from their old Transform position to their new `Cell.Position`.
-   **Spawn**: New tiles are enabled, positioned above the board, and dropped in with a stagger effect.

---

## Key Optimizations

| Feature | Old Approach | New Optimized Approach |
| :--- | :--- | :--- |
| **Execution** | Coroutine (Logic mixed with Anim) | **Synchronous Logic** + Async Anim |
| **Gravity** | Iterative Search (Slow) | **Column Compaction** (O(N)) |
| **Memory** | `Instantiate` / `Destroy` | **Object Pooling** (Zero Garbage) |
| **Data Access** | `FindTileAt` (O(N)) | **`GridSystem` Lookup** (O(1)) |

---

## Integration

The `GameManager` coordinates the cycle:

1.  **After a swap**:
    -   `MatchDetectionSystem.TryGetMatch` (Fast O(1) scan).
    -   `BoardRefillSystem.StartRefill`.

2.  **OnRefillCompleted**:
    -   Checks for **Cascading Matches**.
    -   If found, recursively calls `StartRefill`.
    -   If stable, checks for valid moves or shuffles.

---

## Performance Metrics

This system is instrumented with `Unity.Profiling.ProfilerMarker`:

-   **`BoardRefill.LogicCycle`**: Measures the total time for data updates (Target: <2ms).
-   **`BoardRefill.ProcessColumn`**: Measures gravity/spawn logic per column.

*Typical Performance:*
-   Logic Cycle: **~0.2ms - 0.5ms** (on 6x6 grid)
-   Visuals: **~0.5s** (Smooth animation)