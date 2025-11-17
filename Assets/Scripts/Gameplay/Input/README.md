# Input System

The Input system translates **platform-specific input** (mouse, touch) into **selection commands** for the game.

It does not know about:

- Swapping tiles
- Matching
- Refilling

Its only job is to drive the `SelectSystem` with user interactions.

---

## Core Concepts

### InputHandlerBase
```
csharp
public abstract class InputHandlerBase
{
protected readonly SelectSystemBase SelectSystem;
private bool IsEnabled { get; set; }

    protected InputHandlerBase(SelectSystemBase selectSystem)
    {
        SelectSystem = selectSystem;
        IsEnabled = false;
    }

    public void Enable();
    public void Disable();
    public void Update();

    protected abstract void ProcessInput();
}
```
Responsibilities:

- Tracks whether input is currently **enabled**.
- Exposes an `Update()` method that:
  - Checks `IsEnabled`.
  - Calls `ProcessInput()` when active.
- Holds a reference to `SelectSystemBase`, and forwards selection events to it.

This allows `GameManager` to enable/disable all input (e.g., during swaps, refills, shuffles) with a single call.

---

## MouseInputHandler
```
csharp
public class MouseInputHandler : InputHandlerBase
{
public MouseInputHandler(SelectSystemBase selectSystem) : base(selectSystem) { }

    protected override void ProcessInput()
    {
        if (UnityEngine.Input.GetMouseButtonDown(0))
        {
            // Raycast from mouse position and notify SelectSystem
        }
    }
}
```
Behaviour:

- Listens to **left mouse button** (`GetMouseButtonDown(0)`).
- On click:
  1. Converts mouse position (`Input.mousePosition`) to world space.
  2. Raycasts into the scene (e.g. using a layer mask).
  3. If a tile (or other `ISelectable`) is hit:
     - Calls into `SelectSystem` to handle that selection.

This is primarily used in the Unity Editor and desktop builds.

---

## TouchInputHandler
```
csharp
public class TouchInputHandler : InputHandlerBase
{
public TouchInputHandler(SelectSystemBase selectSystem) : base(selectSystem) { }

    protected override void ProcessInput()
    {
        if (UnityEngine.Input.touchCount == 0)
            return;

        var touch = UnityEngine.Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                // Raycast from touch position and notify SelectSystem
                break;
            // (Moved / Stationary / Ended cases as needed)
        }
    }
}
```
Behaviour:

- Handles **touchscreen input** (mobile platforms).
- Uses the first touch (`GetTouch(0)`).
- Typically, on `TouchPhase.Began`:
  1. Converts touch position to world space.
  2. Raycasts to find an `ISelectable`.
  3. Forwards that selection to `SelectSystem`.

---

## Integration with SelectSystem

The input handlers never make decisions about:

- Whether two tiles are adjacent.
- Whether a swap is valid.
- Whether a match occurred.

They simply translate input into **selection events**.

Typical flow:

1. `InputHandlerBase.Update()` (called from `GameManager.Update`) runs every frame when enabled.
2. On click/tap:
   - Input handler raycasts and finds an `ISelectable` (e.g. `TileBase`).
   - Calls `SelectSystem.HandleSelection(selectable)` (or equivalent).
3. `SelectSystem`:
   - Tracks the first and second selection.
   - When a pair is complete, raises `OnSelectionCompleted(first, second)`.
4. `GameManager` listens to `OnSelectionCompleted` and passes those tiles to the `SwapSystem`.

---

## GameManager Control

`GameManager` creates and owns a single `InputHandlerBase`:
```
csharp
private void CreateInputHandler()
{
#if UNITY_EDITOR
_inputHandler = new MouseInputHandler(_selectSystem);
#elif UNITY_ANDROID || UNITY_IOS
_inputHandler = new TouchInputHandler(_selectSystem);
#else
_inputHandler = new MouseInputHandler(_selectSystem);
#endif
_inputHandler.Disable();
}
```
And controls its lifecycle:

- **Enable input** when the board is stable and ready for the player:
  ```csharp
  _inputHandler.Enable();
  ```
- **Disable input** during:
  - Swaps
  - Reverts
  - Refills
  - Shuffles
  ```csharp
  _inputHandler.Disable();
  ```

This ensures the player cannot interfere with animations or game state transitions.

---

## Design Notes

- **Platform abstraction**:
  - Mouse and touch are handled in separate classes but share the same base behaviour.
- **Separation of concerns**:
  - Input only knows how to interpret hardware events.
  - Selection logic lives in `SelectSystem`.
  - Swap/match logic lives in `SwapSystem` and `MatchDetectionSystem`.
- **Extensibility**:
  - Additional input modes (keyboard, controller, drag-based gestures) can be added by deriving from `InputHandlerBase` and wiring them to `SelectSystem`.