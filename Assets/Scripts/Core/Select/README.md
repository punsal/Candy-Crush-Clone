# Select System

The Select system is responsible for converting **raw input** (mouse/touch) into a pair of **selected tiles** that can be swapped.

It does **not** perform any swapping or match logic itself. Instead, it:

1. Detects what was clicked/tapped (an `ISelectable`).
2. Tracks the current selection (first and second selectable).
3. Raises an event when a **valid selection pair** is completed.

This keeps selection concerns separate from swapping and match detection.

---

## Core Concepts

### ISelectable
```
csharp
public interface ISelectable
{
string Name { get; }
// Typically exposes positional info via a Cell or Transform
}
```
- Implemented by objects that can be selected by the player (e.g. `TileBase`).
- Provides a minimal contract needed for selection and logging.

### SelectableBase
```
csharp
public abstract class SelectableBase : MonoBehaviour, ISelectable
{
public string Name => name;
// Shared utility / base behaviour for selectable objects
}
```
- Base MonoBehaviour for objects that participate in selection.
- Common place to handle naming, caching, or simple visual feedback for “selected” state (if needed).

---

## SelectSystemBase
```
csharp
public abstract class SelectSystemBase : IDisposable
{
public abstract event Action<ISelectable, ISelectable> OnSelectionCompleted;

    public abstract void HandleSelection(ISelectable selectable);
    public abstract void Dispose();
}
```
Responsibilities:

- Holds the **selection state**:
  - First selected object.
  - Second selected object.
- Decides **when** a selection is considered “complete” (i.e., when it has two valid selectables).
- Notifies listeners via `OnSelectionCompleted(first, second)`.

Does **not** know about:

- Swapping.
- Matching.
- Refills or scoring.

---

## SelectSystem (Concrete)
```
csharp
public class SelectSystem : SelectSystemBase
{
// Uses input position + camera raycast to resolve ISelectable.
// Tracks first and second selection.
// Fires OnSelectionCompleted when a pair is ready.
}
```
Typical behaviour:

1. Input handler (mouse/touch) calls into `SelectSystem` with either:
   - A resolved `ISelectable`, or
   - A screen position that `SelectSystem` raycasts from.
2. `SelectSystem` logic:
   - If **no current selection** → store this as the first selectable.
   - If there is a first selectable and the second one is:
     - The same object → update selection (e.g. reselect).
     - A different valid object → fire `OnSelectionCompleted(first, second)` and clear internal state.

---

## Integration with Other Systems

### Input

`Gameplay/Input` handlers (e.g. `MouseInputHandler`, `TouchInputHandler`) are responsible for:

- Reading Unity input (mouse button, touch).
- Calling `SelectSystem` with:
  - Either an `ISelectable` directly (via raycast in input layer), or
  - A screen position that `SelectSystem` internally raycasts.

This keeps the **input platform** (mouse vs touch) separate from selection logic.

### SwapSystem

The `GameManager` subscribes to `SelectSystem.OnSelectionCompleted`:
```
csharp
_selectSystem.OnSelectionCompleted += HandleSelectCompleted;

private void HandleSelectCompleted(ISelectable first, ISelectable second)
{
// Disable input while swap is in progress
_inputHandler.Disable();

    // Convert ISelectable to TileBase and start swap
    _swapSystem.StartSwap(first as TileBase, second as TileBase);
}
```
So the Select system’s single job is to produce a **pair** of tiles for the `SwapSystem` to operate on.

---

## Design Notes

- **Single Responsibility**:
  - SelectSystem does *only* selection; it doesn’t assume how selections are used.
- **Extensibility**:
  - You can later add:
    - Visual feedback (highlight selected tiles).
    - Selection rules (e.g. only allow selecting adjacent tiles).
    - Long-press / drag selection behaviour if needed.
- **Testability**:
  - Because selection is abstracted behind `SelectSystemBase` and `ISelectable`, it can be unit-tested without Unity input or visuals.