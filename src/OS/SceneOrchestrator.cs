// src/OS/SceneOrchestrator.cs
using Godot;
using System.Collections.Generic;
using System.Linq;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;
using Trivale.OS.Events;

namespace Trivale.OS;

/// <summary>
/// Centralizes UI scene management and coordinates with ProcessManager while keeping
/// UI and process logic separate. Handles scene lifecycle including loading,
/// unloading, and state management.
/// 
/// Key responsibilities:
/// - Loads and unloads UI scenes (must inherit from Control)
/// - Manages scene-to-process relationships
/// - Coordinates cleanup between process and scene systems
/// - Maintains references to loaded scenes
/// 
/// Note: All scenes managed by this orchestrator must inherit from Control,
/// as they are expected to be UI scenes that can be shown/hidden in the
/// main content area.
/// 
/// This updated version uses the SystemEventBus for publishing events
/// instead of direct signal connections.
/// </summary>
public partial class SceneOrchestrator : Node
{
    private Dictionary<string, Control> _loadedScenes = new();
    private IProcessManager _processManager;
    private ISlotManager _slotManager;
    private Control _mainContent;
    private Control _mainMenuScene;
    private string _activeProcessId;
    private SystemEventBus _eventBus;
    
    [Signal]
    public delegate void SceneUnloadedEventHandler(bool returningToMainMenu);

    public void Initialize(IProcessManager processManager, ISlotManager slotManager, Control mainContent)
    {
        _processManager = processManager;
        _slotManager = slotManager;
        _mainContent = mainContent;
        _eventBus = SystemEventBus.Instance;

        // Load and setup main menu scene
        InitializeMainMenu();
        
        // Publish system started event
        _eventBus.PublishSystemStarted();
        _eventBus.PublishSystemModeChanged(SystemMode.MainMenu);
    }

    private void InitializeMainMenu()
    {
        var menuScene = LoadSceneInstance("res://Scenes/MainMenu/MainMenuScene.tscn");
        if (menuScene != null)
        {
            _mainMenuScene = menuScene;
            if (menuScene is MainMenu.MainMenuScene mainMenu)
            {
                mainMenu.MenuOptionSelected += OnMenuOptionSelected;
            }
            ShowScene(menuScene);
            
            // Publish scene loaded event
            _eventBus.PublishSceneLoaded("MainMenuScene");
        }
    }

    private void OnMenuOptionSelected(string scenePath, string processType)
    {
        LoadScene(processType, scenePath);
    }

    public bool LoadScene(string processType, string scenePath)
    {
        // First create and start the process
        var processId = _processManager.CreateProcess(processType);
        if (processId == null || !_processManager.StartProcess(processId, out var slotId))
        {
            GD.PrintErr($"Failed to create/start process for {processType}");
            return false;
        }

        // Then load the scene
        var scene = LoadSceneInstance(scenePath);
        if (scene == null) return false;

        // Store the scene
        _loadedScenes[processId] = scene;
        _activeProcessId = processId;

        // Initialize if it's a DebugScene
        if (scene is MainMenu.DebugScene debugScene)
        {
            debugScene.Initialize(_processManager, _slotManager);
        }

        // Connect to scene's unload signal
        if (scene.HasSignal("SceneUnloadRequested"))
        {
            // Store the signal connection for later disconnection
            if (scene.Connect("SceneUnloadRequested", new Callable(this, nameof(HandleSceneUnloadRequest))) != Error.Ok)
            {
                GD.PrintErr($"Failed to connect SceneUnloadRequested signal for {processId}");
            }
            else
            {
                GD.Print($"Connected SceneUnloadRequested signal for {processId}");
            }
        }

        // Show the scene (which hides main menu)
        ShowScene(scene);
        
        // Publish scene loaded and system mode changed events
        _eventBus.PublishSceneLoaded(scenePath);
        
        SystemMode mode = processType switch
        {
            "CardGame" => SystemMode.GameSession,
            "Debug" => SystemMode.Debug,
            "Settings" => SystemMode.Settings,
            _ => SystemMode.MainMenu
        };
        _eventBus.PublishSystemModeChanged(mode);
        
        return true;
    }

    private Control LoadSceneInstance(string scenePath)
    {
        var sceneResource = ResourceLoader.Load<PackedScene>(scenePath);
        if (sceneResource == null)
        {
            GD.PrintErr($"Failed to load scene: {scenePath}");
            return null;
        }

        var instance = sceneResource.Instantiate();
        if (instance is not Control control)
        {
            GD.PrintErr($"Scene must be a Control: {scenePath}");
            instance.QueueFree();
            return null;
        }
        
        // Set up Control properly
        control.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        control.SizeFlagsVertical = Control.SizeFlags.Fill;
        control.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        control.GrowHorizontal = Control.GrowDirection.Both;
        control.GrowVertical = Control.GrowDirection.Both;

        return control;
    }

    private void ShowScene(Control scene)
    {
        // Instead of destroying content, hide everything
        foreach (var child in _mainContent.GetChildren().OfType<Control>())
        {
            child.Visible = false;
        }

        // If scene isn't already in the tree, add it
        if (!scene.IsInsideTree())
        {
            _mainContent.AddChild(scene);
        }
        
        // Make requested scene visible
        scene.Visible = true;
    }

    private void HandleSceneUnloadRequest()
    {
        if (_activeProcessId == null) return;

        // Get current scene and extract path before we clear references
        var currentScene = _loadedScenes.GetValueOrDefault(_activeProcessId);
        string scenePath = currentScene?.Name ?? "Unknown";
        bool isReturningToMainMenu = true; // For now, always true. Later could be false for "minimize"

        // First disconnect all signals - crucial for preventing disposed object access
        if (currentScene != null && currentScene.HasSignal("SceneUnloadRequested"))
        {
            currentScene.Disconnect("SceneUnloadRequested", new Callable(this, nameof(HandleSceneUnloadRequest)));
            GD.Print($"Disconnected SceneUnloadRequested signal for {_activeProcessId}");
        }

        // Cache processId before clearing reference
        var processIdToUnload = _activeProcessId;
        
        // Clear references
        _loadedScenes.Remove(_activeProcessId);
        _activeProcessId = null;
        
        // Signal that scene is being unloaded 
        EmitSignal(SignalName.SceneUnloaded, isReturningToMainMenu);
        
        // Publish event through bus
        _eventBus.PublishSceneUnloaded(scenePath, isReturningToMainMenu);
        
        // Now clean up the process
        _processManager.UnloadProcess(processIdToUnload);

        // Hide the scene and queue it for deletion
        if (currentScene != null)
        {
            currentScene.Visible = false;
            // Only queue free after we've disconnected signals and cleaned references
            currentScene.QueueFree();
        }

        // Show main menu
        if (_mainMenuScene != null)
        {
            ShowScene(_mainMenuScene);
            
            // Publish system is returning to main menu
            _eventBus.PublishSystemModeChanged(SystemMode.MainMenu);
        }
    }

    public override void _ExitTree()
    {
        // Disconnect menu signals
        if (_mainMenuScene is MainMenu.MainMenuScene mainMenu)
        {
            mainMenu.MenuOptionSelected -= OnMenuOptionSelected;
        }

        // Disconnect scene signals
        foreach (var kvp in _loadedScenes)
        {
            var scene = kvp.Value;
            if (scene.HasSignal("SceneUnloadRequested"))
            {
                scene.Disconnect("SceneUnloadRequested", new Callable(this, nameof(HandleSceneUnloadRequest)));
            }
        }
        
        // Publish system shutdown
        _eventBus.PublishSystemShutdown();
        
        // Clean up any remaining scenes
        foreach (var scene in _loadedScenes.Values)
        {
            if (scene.IsInsideTree())
            {
                scene.QueueFree();
            }
        }
        _loadedScenes.Clear();
        
        if (_mainMenuScene != null)
        {
            _mainMenuScene.QueueFree();
        }
        
        // Clear references
        _processManager = null;
        _slotManager = null;
        _mainContent = null;
        _mainMenuScene = null;
    }
}