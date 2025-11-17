# Camera System

Manages camera operations for dynamic grid-based gameplay, including positioning and orthographic sizing with flexible camera provider abstraction.

## Structure

- **Abstract/** - Base classes for camera systems
  - **CameraSystemBase.cs** - Abstract camera system foundation
- **Provider/** - Camera access abstraction layer
  - **Abstract/** - Base classes for camera providers
    - **UnityCameraProviderBase.cs** - MonoBehaviour-based provider base
  - **Interface/** - Camera provider contracts
    - **ICameraProvider.cs** - Camera access interface
  - **UnityCameraProvider.cs** - Component-based camera provider
  - **FallbackCameraProvider.cs** - Non-MonoBehaviour camera provider
- **CameraSystem.cs** - Concrete camera system implementation

## Key Components

### ICameraProvider
Interface providing access to a Unity Camera instance.
- Enables decoupled camera access
- Supports dependency injection
- Allows for testing and mocking

### CameraSystemBase
Abstract base class for camera systems.
- Accepts `ICameraProvider` for flexible camera sourcing
- Defines `CenterOnGrid(rows, columns)` contract
- Provides protected `Camera` property access

### UnityCameraProviderBase
Abstract MonoBehaviour base class for camera providers implementing `ICameraProvider`.
- Used when camera provider needs Unity lifecycle hooks
- Enables component-based camera management

### UnityCameraProvider
Concrete MonoBehaviour implementation that manages a Camera component.
- Auto-detects camera via `GetComponent<Camera>()`
- Falls back to `Camera.main` if no camera is attached
- Requires Camera component (enforced via `RequireComponent`)

### FallbackCameraProvider
Non-MonoBehaviour implementation of `ICameraProvider`.
- Used for direct camera reference without Unity components
- Useful for testing or non-scene-based camera management
- Accepts camera instance via constructor

### CameraSystem
Concrete camera system implementation.

**Features:**
- Centers and scales camera to fit the grid
- Calculates center position based on grid dimensions
- Adjusts orthographic size with aspect ratio handling
- Adds 1-unit padding for visual clarity
- Null-safe camera validation

**Method:**
- `CenterOnGrid(rows, columns)` - Positions camera to frame grid perfectly

## Usage

### Standard Setup (MonoBehaviour Provider)
```
csharp
// In Unity: Attach UnityCameraProvider to Camera GameObject

// In your game manager:
var cameraProvider = FindObjectOfType<UnityCameraProvider>();
var cameraSystem = new CameraSystem(cameraProvider);

// Center on grid
cameraSystem.CenterOnGrid(8, 8); // 8x8 board
```
### Fallback Setup (Direct Reference)
```
csharp
// Create fallback provider with direct camera reference
var camera = Camera.main;
var cameraProvider = new FallbackCameraProvider(camera);
var cameraSystem = new CameraSystem(cameraProvider);

// Center on grid
cameraSystem.CenterOnGrid(10, 6); // 10x6 board
```
### Custom Camera System
```
csharp
public class CustomCameraSystem : CameraSystemBase
{
public CustomCameraSystem(ICameraProvider cameraProvider) : base(cameraProvider)
{
}

    public override void CenterOnGrid(int rows, int columns)
    {
        // Custom centering logic
        // Access camera via protected Camera property
    }
    
    public void ZoomTo(float size)
    {
        Camera.orthographicSize = size;
    }
}
```
## Design Patterns

### Provider Pattern
Decouples camera access from concrete Unity Camera implementation:
- Easy testing and mocking via `ICameraProvider`
- Supports runtime camera switching
- Separation of concerns between camera sourcing and camera operations

### Strategy Pattern
Different provider implementations (`UnityCameraProvider`, `FallbackCameraProvider`) allow flexible camera sourcing strategies.

### Template Method
`CameraSystemBase` defines structure while derived classes implement specific camera operations.

## Camera Positioning Logic
```

Grid Layout (8x8 example):
(0,0)   (7,0)
┌───────┐
│       │
│   ●   │  ● = Camera center at (3.5, -3.5)
│       │
└───────┘
(0,-7)  (7,-7)

Center X = (columns - 1) / 2 = 3.5
Center Y = -(rows - 1) / 2 = -3.5
```
## Orthographic Size Calculation
```
csharp
verticalSize = rows / 2     // Fit height
horizontalSize = columns / (2 * aspect)  // Fit width accounting for aspect ratio

orthographicSize = Max(verticalSize, horizontalSize) + 1  // Use larger + padding
```
## Important Notes

- **Orthographic Only**: Assumes orthographic camera projection
- **Cell Size**: Grid cells must occupy 1 Unity unit each
- **Z-Position**: Camera Z-position is preserved during centering
- **Padding**: Adds 1 unit padding around grid edges
- **Aspect Ratio**: Automatically adjusts for different screen ratios
- **Grid Origin**: Assumes cells are positioned at (column, -row, 0)
- **Null Safety**: `CenterOnGrid()` validates camera before operations

## Provider Selection Guide

| Use Case | Recommended Provider |
|----------|---------------------|
| Scene-based camera | `UnityCameraProvider` |
| Testing/Mocking | `FallbackCameraProvider` or custom `ICameraProvider` |
| Runtime camera switching | `FallbackCameraProvider` |
| Multiple cameras | Custom `ICameraProvider` implementation |

## Performance Considerations

- Provider lookup happens once during initialization
- Camera operations are direct property access (no searching)
- Minimal overhead from abstraction layer

## Extensibility

To extend the system:

1. **Custom Providers**: Implement `ICameraProvider` for unique camera sourcing (e.g., camera pools, multi-camera setups)
2. **Custom Systems**: Inherit from `CameraSystemBase` for additional camera operations (shake, follow, zoom animations)
3. **MonoBehaviour Providers**: Inherit from `UnityCameraProviderBase` when Unity lifecycle hooks are needed
4. **POCO Providers**: Implement `ICameraProvider` directly for non-MonoBehaviour scenarios

## Example: Multi-Camera System
```
csharp
public class MultiCameraProvider : ICameraProvider
{
private readonly Dictionary<string, Camera> _cameras;
private string _activeCameraKey;

    public Camera Camera => _cameras[_activeCameraKey];
    
    public void SwitchCamera(string key)
    {
        if (_cameras.ContainsKey(key))
            _activeCameraKey = key;
    }
}
```
## Testing Support

The provider pattern enables easy unit testing:
```
csharp
// Mock camera provider for tests
public class MockCameraProvider : ICameraProvider
{
public Camera Camera { get; set; }
}

[Test]
public void TestCenterOnGrid()
{
var mockCamera = new GameObject().AddComponent<Camera>();
var provider = new MockCameraProvider { Camera = mockCamera };
var system = new CameraSystem(provider);

    system.CenterOnGrid(8, 8);
    
    Assert.AreEqual(new Vector3(3.5f, -3.5f, mockCamera.transform.position.z), 
                    mockCamera.transform.position);
}
```