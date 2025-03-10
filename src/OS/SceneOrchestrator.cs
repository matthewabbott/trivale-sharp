// src/OS/SceneOrchestrator.cs
using Godot;
using System.Collections.Generic;
using System.Linq;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;
using Trivale.OS.Events;
using Trivale.OS.MainMenu;
using Trivale.OS.MainMenu.Processes;

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

    private const string MAIN_MENU_TYPE = "MainMenu";
    private const string MAIN_MENU_ID = "mainmenu";

    /// <summary>
    /// Dictionary mapping process IDs to their respective scenes.
    /// This allows us to track which scene belongs to which process.
    /// </summary>
    private Dictionary<string, Control> _loadedScenes = new();
    
    /// <summary>
    /// The currently active process ID. Used to track which scene is displayed.
    /// </summary>
    private string _activeProcessId;
    private IProcessManager _processManager;
    private ISlotManager _slotManager;
    private Control _mainContent;
    private SystemEventBus _eventBus;
    private ProcessSlotRegistry _processSlotRegistry;
    
    [Signal]
    public delegate void SceneUnloadedEventHandler(bool returningToMainMenu);

    /// <summary>
    /// Initializes the orchestrator with required dependencies.
    /// </summary>
    public void Initialize(IProcessManager processManager, ISlotManager slotManager, 
        ProcessSlotRegistry registry, Control mainContent)
    {
        _processManager = processManager;
        _slotManager = slotManager;
        _processSlotRegistry = registry;
        _mainContent = mainContent;
        _eventBus = SystemEventBus.Instance;

        // Subscribe to registry events
        _processSlotRegistry.ActiveProcessChanged += OnActiveProcessChanged;
        
        // Publish system started event
        _eventBus.PublishSystemStarted();
        _eventBus.PublishSystemModeChanged(SystemMode.MainMenu);
    }

    /// <summary>
    /// Responds to active process changes in the registry.
    /// Shows the scene associated with the newly active process.
    /// </summary>
    private void OnActiveProcessChanged(string processId)
    {
        if (string.IsNullOrEmpty(processId))
            return;
            
        // Show the scene associated with this process
        if (_loadedScenes.TryGetValue(processId, out var scene))
        {
            ShowScene(processId);
        }
    }

    /// <summary>
    /// Handles menu option selection from the main menu.
    /// </summary>
    private void OnMenuOptionSelected(string scenePath, string processType)
    {
        LoadScene(processType, scenePath);
    }

    /// <summary>
    /// Responds to slot selection in the SlotGridSystem.
    /// Shows the scene for the process in the selected slot.
    /// </summary>
    public void HandleSlotSelected(string slotId, string processId)
    {
        if (string.IsNullOrEmpty(processId))
            return;
        
        // If we have a scene for this process, show it
        if (_loadedScenes.TryGetValue(processId, out _))
        {
            ShowScene(processId);
        }
    }

    /// <summary>
    /// Shows a scene for a specific process ID.
    /// Hides all other scenes in the main content area.
    /// Sets active process in registry.
    /// </summary>
    /// <param name="processId">The ID of the process whose scene should be shown</param>
    public void ShowScene(string processId)
    {
        if (string.IsNullOrEmpty(processId))
        {
            GD.PrintErr("Invalid process ID (null or empty)");
            return;
        }
        
        var process = _processManager.GetProcess(processId);
        if (process == null)
        {
            GD.PrintErr($"Invalid process id for ShowScene: {processId}");
            return;
        }
        
        string scenePath = null;
        
        // Determine scene path based on process type
        if (process is MainMenuProcess mainMenuProcess)
        {
            // Special handling for MainMenuProcess
            scenePath = mainMenuProcess.ScenePath;
        }
        else 
        {
            // Get scenePath from process state for other processes
            var state = process.GetState();
            if (state.TryGetValue("scenePath", out var path) && path is string pathString)
            {
                scenePath = pathString;
            }
        }
        
        if (string.IsNullOrEmpty(scenePath))
        {
            GD.PrintErr($"No scene path found for process: {processId}");
            return;
        }
        
        // Load and show the scene
        LoadSceneForProcess(scenePath, processId);
        
        // Update active process references
        _activeProcessId = processId;
        _processSlotRegistry.SetActiveProcess(processId);
        
        // Publish event for scene loaded
        _eventBus.PublishSceneLoaded(scenePath);
    }

    /// <summary>
    /// Loads and shows a scene for a specific process.
    /// </summary>
    /// <param name="scenePath">Path to the scene resource</param>
    /// <param name="processId">ID of the associated process</param>
    private void LoadSceneForProcess(string scenePath, string processId)
    {
        // Hide all existing scenes
        foreach (var child in _mainContent.GetChildren())
        {
            if (child is Control control)
            {
                control.Visible = false;
            }
        }
        
        Control scene = null;
        
        // If scene is already loaded, reuse it
        if (_loadedScenes.TryGetValue(processId, out scene) && scene != null && IsInstanceValid(scene))
        {
            // Scene already loaded, just show it
            scene.Visible = true;
            return;
        }
        
        // Clean up old scene if it exists but is invalid
        if (scene != null && !IsInstanceValid(scene))
        {
            _loadedScenes.Remove(processId);
        }
        
        // Load the new scene
        scene = LoadSceneInstance(scenePath);
        if (scene == null)
        {
            GD.PrintErr($"Failed to load scene: {scenePath}");
            return;
        }
        
        // Store process ID in scene metadata
        scene.SetMeta("ProcessId", processId);
        
        // Set orchestrator if scene implements the interface
        if (scene is IOrchestratableScene orchestratable)
        {
            orchestratable.SetOrchestrator(this);
        }
        
        // Add to the content area and cache
        _mainContent.AddChild(scene);
        _loadedScenes[processId] = scene;
        
        // Ensure it's visible
        scene.Visible = true;
    }
    
    /// <summary>
    /// Loads a scene for a specific process type and makes it visible.
    /// Creates a process, starts it in a slot, and associates the scene with it.
    /// </summary>
    /// <param name="processType">The type of process to create</param>
    /// <param name="scenePath">The path to the scene resource</param>
    /// <returns>True if the scene was loaded successfully</returns>
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

        // Initialize scene with relevant data
        InitializeLoadedScene(scene, processId);

        // Show the scene
        ShowScene(processId);
        
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
    
    /// <summary>
    /// Initializes a newly loaded scene with necessary references and data.
    /// Stores the process ID in the scene's metadata and configures scene-specific settings.
    /// </summary>
    /// <param name="scene">The scene to initialize</param>
    /// <param name="processId">The ID of the process associated with this scene</param>
    private void InitializeLoadedScene(Control scene, string processId)
    {
        // Store process ID in scene's metadata
        scene.SetMeta("ProcessId", processId);
        
        // If scene implements our interface, use it
        if (scene is IOrchestratableScene orchestratableScene)
        {
            orchestratableScene.SetOrchestrator(this);
        }
        
        // Additional scene-specific initialization if needed
        if (scene is MainMenu.DebugScene debugScene)
        {
            debugScene.Initialize(_processManager, _slotManager);
        }
    }
    
    /// <summary>
    /// Provides a direct method call for scene unloading requests.
    /// Not a recommended Godot pattern: restricts independence of scenes
    /// In exchange, we get explicit control flow.
    /// </summary>
    /// <param name="processId">The ID of the process to unload. If null, uses the active process.</param>
    public void RequestSceneUnload(string processId = null)
    {
        // If no specific process ID provided, use the active one
        if (string.IsNullOrEmpty(processId))
        {
            processId = _activeProcessId;
        }
        
        // Check if this is the main menu process
        var process = _processManager.GetProcess(processId);
        if (process?.Type == "MainMenu")
        {
            GD.PrintErr("Main menu process is protected from unloading - switching to it instead");
            ReturnToMainMenu(); // Just switch to the main menu instead
            return;
        }
        
        if (string.IsNullOrEmpty(processId) || !_loadedScenes.ContainsKey(processId))
        {
            GD.PrintErr($"Invalid process ID in unload request: {processId}");
            return;
        }

        // Get current scene and extract path before we clear references
        var currentScene = _loadedScenes[processId];
        string scenePath = currentScene?.Name ?? "Unknown";
        bool isReturningToMainMenu = true; // For now, always true

        // Remove from loaded scenes
        _loadedScenes.Remove(processId);
        
        // Clear active reference if this was the active process
        if (_activeProcessId == processId)
        {
            _activeProcessId = null;
        }
        
        // Signal that scene is being unloaded 
        EmitSignal(SignalName.SceneUnloaded, isReturningToMainMenu);
        
        // Publish event through bus
        _eventBus.PublishSceneUnloaded(scenePath, isReturningToMainMenu);
        
        // Clean up the process first while we still have a valid scene reference
        _processManager.UnloadProcess(processId);

        // Hide and queue the scene for deletion as the final step
        if (currentScene != null)
        {
            currentScene.Visible = false;
            currentScene.QueueFree();
        }

        // Return to main menu
        ReturnToMainMenu();
    }

    /// <summary>
    /// Loads a scene instance from a resource path.
    /// Sets up the control properties for proper layout.
    /// </summary>
    /// <param name="scenePath">The path to the scene resource</param>
    /// <returns>The instantiated scene as a Control, or null if loading failed</returns>
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

    /// <summary>
    /// Returns to the main menu without unloading other scenes.
    /// This allows the player to return to the menu while keeping their game state.
    /// </summary>
    public void ReturnToMainMenu()
    {
        var mainMenuProcessId = _loadedScenes.Keys
            .FirstOrDefault(id => _processManager.GetProcess(id)?.Type == MAIN_MENU_TYPE);
            
        if (!string.IsNullOrEmpty(mainMenuProcessId))
        {
            ShowScene(mainMenuProcessId);
            GD.Print("Returned to main menu");
            return;
        }

        GD.PrintErr("Main menu process not found in loaded scenes; attempting recovery");
        string newMainMenuId = _processManager.CreateProcess(MAIN_MENU_TYPE, null, MAIN_MENU_ID);
        string slotId = null;
        if (newMainMenuId != null && _processManager.StartProcess(newMainMenuId, out slotId))
        {
            GD.Print($"Created and started new MainMenu process {newMainMenuId} in slot {slotId}");
            ShowScene(newMainMenuId);
        }
        else
        {
            GD.PrintErr($"Failed to recover MainMenu process. Created ID: {newMainMenuId}, Slot: {slotId ?? "none"}");
        }
    }

    /// <summary>
    /// Creates a preview representation of a process for display in a slot.
    /// </summary>
    /// <param name="processId">ID of the process to preview</param>
    /// <param name="size">Desired size of the preview</param>
    /// <returns>A Control representing the process preview</returns>
    public Control CreateProcessPreview(string processId, Vector2 size)
    {
        // Implementation of process preview creation
        // This can be left as is for now
        return null;
    }

    public override void _ExitTree()
    {
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

        if (_processSlotRegistry != null)
        {
            _processSlotRegistry.ActiveProcessChanged -= OnActiveProcessChanged;
        }
        
        // Clear references
        _processManager = null;
        _slotManager = null;
        _mainContent = null;
        _activeProcessId = null;
        _processSlotRegistry = null;
    }
}