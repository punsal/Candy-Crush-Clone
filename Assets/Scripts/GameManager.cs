using System.Collections.Generic;
using System.Linq;
using Core.Camera;
using Core.Camera.Abstract;
using Core.Camera.Provider;
using Core.Camera.Provider.Abstract;
using Core.Grid;
using Core.Grid.Abstract;
using Core.Grid.Cell.Abstract;
using Core.Pool;
using Core.Pool.Abstract;
using Core.Random;
using Core.Random.Abstract;
using Core.Runner.Interface;
using Core.Select;
using Core.Select.Abstract;
using Core.Select.Interface;
using Gameplay.Input;
using Gameplay.Input.Abstract;
using Gameplay.Systems.BoardRefill;
using Gameplay.Systems.BoardRefill.Abstract;
using Gameplay.Systems.MatchDetection;
using Gameplay.Systems.MatchDetection.Abstract;
using Gameplay.Systems.Shuffle;
using Gameplay.Systems.Shuffle.Abstract;
using Gameplay.Systems.Swap;
using Gameplay.Systems.Swap.Abstract;
using Gameplay.Tile;
using Gameplay.Tile.Abstract;
using Unity.Profiling;
using UnityEngine;

public class GameManager : MonoBehaviour, ICoroutineRunner
{
    private static readonly ProfilerMarker SwapCompletedMarker = new("GameManager.HandleSwapCompleted");
    
    [Header("Random")]
    [SerializeField] private int seed = 0;
    [SerializeField, Tooltip("After shuffle fails, should increase the seed for new random range")]
    private bool shouldIncreaseSeed = true;
    
    [Header("Camera")]
    [SerializeField] private UnityCameraProviderBase gameCameraProvider;
    
    [Header("Grid")]
    [SerializeField] private CellBase gridCellPrefab;
    
    [Header("Chip")]
    [SerializeField] private List<TileBase> tilePrefabs;
    
    [Header("Linking")]
    [SerializeField] private UnityCameraProviderBase linkCameraProvider;
    [SerializeField] private LayerMask linkLayerMask;
    
    [Header("System Configuration")]
    [SerializeField, Range(2, 5)] private int shuffleCountBeforeFailure = 2;

    private RandomSystemBase _randomSystem;
    private PoolSystemBase _poolSystem;
    private CameraSystemBase _cameraSystem;
    private GridSystemBase _gridSystem;
    private SelectSystemBase _selectSystem;
    private TileManagerBase _tileManager;
    private BoardRefillSystemBase _boardRefillSystem;
    private MatchDetectionSystemBase _matchDetectionSystem;
    private ShuffleSystemBase _shuffleSystem;
    private SwapSystemBase _swapSystem;
    private InputHandlerBase _inputHandler;
    
    private int _currentShuffleCount;
    private const int RowCount = 6;
    private const int ColumnCount = 6;
    
    private void Awake()
    {
        CreateSystems();
    }
    
    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void Start()
    {
        CreateGameplay();
    }

    private void Update()
    {
        _inputHandler?.Update();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        DestroySystems();
    }

    private void CreateSystems()
    {
        CreateRandomSystem();
        CreatePoolSystem();
        CreateGameCameraSystem();
        CreateGridSystem();
        CreateLinkSystem();
        CreateChipManager();
        CreateBoardRefillSystem();
        CreateMatchDetectionSystem();
        CreateShuffleSystem();
        CreateSwapSystem();
        CreateInputHandler();   
    }

    private void CreateGameplay()
    {
        _currentShuffleCount = 0;
        
        _gridSystem.Initialize();
        _tileManager.Initialize();
        _tileManager.FillGrid();
        _cameraSystem.CenterOnGrid(RowCount, ColumnCount);

        // 1) If there is any existing match on the freshly filled grid, shuffle first.
        if (_matchDetectionSystem.TryGetMatch(out _))
        {
            Debug.Log("Initial match detected, starting shuffle");
            _shuffleSystem.StartShuffle();
            return;
        }

        // 2) If there is no possible move, shuffle.
        if (!ShouldShuffle())
        {
            Debug.Log("Player has moves, no need to shuffle");
            _inputHandler.Enable();
            return;
        }
        
        Debug.Log("Starting shuffle");
        _shuffleSystem.StartShuffle();
    }

    private void DestroySystems()
    {
        _shuffleSystem.Dispose();
        _boardRefillSystem.Dispose();
        _swapSystem.Dispose();
        _tileManager.Dispose();
        _selectSystem.Dispose();
        _gridSystem.Dispose();
        _poolSystem.Dispose();
    }

    private void SubscribeToEvents()
    {
        _selectSystem.OnSelectionCompleted += HandleSelectCompleted;
        _swapSystem.OnSwapCompleted += HandleSwapCompleted;
        _swapSystem.OnRevertCompleted += HandleRevertCompleted;
        _boardRefillSystem.OnRefillCompleted += HandleRefillCompleted;
        _shuffleSystem.OnShuffleCompleted += HandleShuffleCompleted;
    }

    private void UnsubscribeFromEvents()
    {
        _selectSystem.OnSelectionCompleted -= HandleSelectCompleted;
        _swapSystem.OnSwapCompleted -= HandleSwapCompleted;
        _swapSystem.OnRevertCompleted -= HandleRevertCompleted;
        _boardRefillSystem.OnRefillCompleted -= HandleRefillCompleted;
        _shuffleSystem.OnShuffleCompleted -= HandleShuffleCompleted;
    }

    private void CreateRandomSystem()
    {
        _randomSystem = new RandomSystem(seed);
    }
    
    private void CreatePoolSystem()
    {
        _poolSystem = new PoolSystem();
    }

    private void CreateGameCameraSystem()
    {
        if (gameCameraProvider == null)
        {
            Debug.LogWarning("Game camera provider is null");
            if (Camera.main == null)
            {
                Debug.LogError("No main camera found");
                _cameraSystem = new CameraSystem(null);
            }
            else
            {
                Debug.LogWarning("Using main camera");
                _cameraSystem = new CameraSystem(new FallbackCameraProvider(Camera.main));
            }
        }
        else
        {
            _cameraSystem = new CameraSystem(gameCameraProvider);
        }
    }

    private void CreateGridSystem()
    {
        if (gridCellPrefab == null)
        {
            Debug.LogError("Tile prefab is null");
        }
        _gridSystem = new GridSystem(RowCount, ColumnCount, gridCellPrefab);
    }

    private void CreateLinkSystem()
    {
        if (linkCameraProvider == null)
        {
            Debug.LogWarning("Link camera provider is null");
            if (Camera.main == null)
            {
                Debug.LogError("No main camera found");
                _selectSystem = new SelectSystem(null, linkLayerMask);
            }
            else
            {
                Debug.LogWarning("Using main camera");
                _selectSystem = new SelectSystem(new FallbackCameraProvider(Camera.main), linkLayerMask);
            }
        }
        else
        {
            _selectSystem = new SelectSystem(linkCameraProvider, linkLayerMask);
        }
    }

    private void CreateChipManager()
    {
        if (tilePrefabs == null || tilePrefabs.Count == 0)
        {
            Debug.LogError("No chip prefabs found");
        }
        _tileManager = new TileManager(_randomSystem, _gridSystem, tilePrefabs, _poolSystem);
    }

    private void CreateBoardRefillSystem()
    {
        _boardRefillSystem = new BoardRefillSystem(_gridSystem, _tileManager, this);
    }

    private void CreateMatchDetectionSystem()
    {
        _matchDetectionSystem = new MatchDetectionSystem(_gridSystem, _tileManager);
    }

    private void CreateShuffleSystem()
    {
        _shuffleSystem = new ShuffleSystem(_gridSystem, _tileManager, this, _randomSystem);
    }

    private void CreateSwapSystem()
    {
        _swapSystem = new SwapSystem(_gridSystem, this);
    }

    private void CreateInputHandler()
    {
#if UNITY_EDITOR
        _inputHandler = new MouseInputHandler(_selectSystem);
        Debug.Log("Using MouseInputHandler in Unity Editor");
#elif UNITY_ANDROID || UNITY_IOS
        _inputHandler = new TouchInputHandler(_selectSystem);
        Debug.Log("Using TouchInputHandler for mobile platform");
#else
        _inputHandler = new MouseInputHandler(_selectSystem);
        Debug.Log("Using MouseInputHandler for other platforms");
#endif
        _inputHandler.Disable();
    }

    private void HandleSelectCompleted(ISelectable firstSelectable, ISelectable lastSelectable)
    {
        Debug.Log($"Selection completed: {firstSelectable.Name} -> {lastSelectable.Name}");
        
        // Disable input during refill sequence
        _inputHandler.Disable();
        
        // Try swapping tiles
        _swapSystem.StartSwap(firstSelectable as TileBase, lastSelectable as TileBase);
    }

    private void HandleSwapCompleted(TileBase tile1, TileBase tile2)
    {
        SwapCompletedMarker.Begin();
        Debug.Log($"Swap completed: {tile1.Name} -> {tile2.Name}");
        
        // check if any match
        if (_matchDetectionSystem.TryGetMatch(out var matchTiles))
        {
            Debug.Log($"Match found: {string.Join(", ", matchTiles.Select(c => c.name))}");
            // Start destruction and refill sequence
            _boardRefillSystem.StartRefill(matchTiles);
            
            SwapCompletedMarker.End();
            return;
        }
        
        Debug.Log("No match found, reverting swaps");
        _swapSystem.StartRevert(tile1, tile2);
        
        SwapCompletedMarker.End();
    }

    private void HandleRevertCompleted()
    {
        Debug.Log("Revert complete, enabling input");
        _inputHandler.Enable();
    }

    private void HandleRefillCompleted()
    {
        // 1) Cascading matches: if refill created new matches, resolve them.
        if (_matchDetectionSystem.TryGetMatch(out var cascadeMatches))
        {
            Debug.Log($"Cascade match found: {cascadeMatches.Count} tiles");
            _boardRefillSystem.StartRefill(cascadeMatches);
            return;
        }
        
        if (ShouldShuffle())
        {
            Debug.Log("No possible moves detected");
            _shuffleSystem.StartShuffle();
            return;
        }
        
        Debug.Log("Refill complete");
        _inputHandler.Enable();
    }

    private bool ShouldShuffle()
    {
        if (_matchDetectionSystem.HasPossibleMoves())
        {
            return false;
        }
        _currentShuffleCount = 0;
        return true;
    }

    private void HandleShuffleCompleted()
    {
        _currentShuffleCount++;
        Debug.Log($"Current shuffle count: {_currentShuffleCount}");
        
        // If there is any existing match on the freshly filled grid, shuffle again.
        if (_matchDetectionSystem.TryGetMatch(out _))
        {
            if (CanShuffle())
            {
                Debug.Log("Initial match detected, starting shuffle");
                _shuffleSystem.StartShuffle();
                return;
            }
            
            Debug.Log("Initial match detected but shuffle failed. No more shuffle attempts, restarting game.");
            Replay();
            return;
        }
        
        if (!_matchDetectionSystem.HasPossibleMoves())
        {
            if (_currentShuffleCount < shuffleCountBeforeFailure)
            {
                Debug.Log("No possible moves detected, shuffle failed. Trying again.");
                _shuffleSystem.StartShuffle();
                return;
            }
            
            Debug.Log("No possible moves detected, shuffle failed. No more shuffle attempts, restarting game.");
            Replay();
            return;
        }
        
        Debug.Log("Shuffle succeed.");
        _inputHandler.Enable();
    }

    private bool CanShuffle()
    {
        if (_currentShuffleCount >= shuffleCountBeforeFailure)
        {
            Debug.Log("No more shuffle.");
            return false;
        }
            
        Debug.Log("Can shuffle.");
        return true;
    }

    private void Replay()
    {
        Debug.Log("Stopping current game, destroying systems");
        UnsubscribeFromEvents();
        DestroySystems();

        if (shouldIncreaseSeed)
        {
            seed++;
        }
        
        Debug.Log("Starting next game");
        CreateSystems();
        SubscribeToEvents();
        CreateGameplay();
        
        Debug.Log("Next game ready");
    }
    
    [ContextMenu("Debug Tile Count")]
    private void DebugTileCount()
    {
        var activeTiles = _tileManager.ActiveTiles;
        var nonNullTiles = activeTiles.Where(c => c != null).ToList();
        var withTiles = nonNullTiles.Where(c => c.Cell != null).ToList();
    
        Debug.Log($"=== TILE COUNT DEBUG ===");
        Debug.Log($"Total in list: {activeTiles.Count}");
        Debug.Log($"Non-null: {nonNullTiles.Count}");
        Debug.Log($"With tiles: {withTiles.Count}");
        Debug.Log($"Expected: {RowCount * ColumnCount}");
    
        // Check for duplicates
        var duplicates = withTiles
            .GroupBy(tile => (tile.Cell.Row, tile.Cell.Column))
            .Where(g => g.Count() > 1)
            .ToList();
    
        if (duplicates.Any())
        {
            Debug.LogError($"Found {duplicates.Count} duplicate positions:");
            foreach (var dup in duplicates)
            {
                Debug.LogError($"  Position ({dup.Key.Row}, {dup.Key.Column}): {dup.Count()} tiles - {string.Join(", ", dup.Select(c => c.name))}");
            }
        }
        else
        {
            Debug.Log("No duplicates found!");
        }
    }
}
