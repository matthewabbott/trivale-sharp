// src/OS/SceneOrchestrator.cs
using Godot;
using System.Collections.Generic;
using System.Linq;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;
using Trivale.OS.Events;
using Trivale.OS.MainMenu;

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
    private Control _mainMenuScene;
    private SystemEventBus _eventBus;
    
    [Signal]
    public delegate void SceneUnloadedEventHandler(bool returningToMainMenu);

    public void Initialize(IProcessManager processManager, ISlotManager slotManager, Control mainContent)
    {
        _processManager = processManager;
        _slotManager = slotManager;
        _mainContent = mainContent;
        _eventBus = SystemEventBus.Instance;

        // Initialize main menu as a special process
        InitializeMainMenu();
        
        // Publish system started event
        _eventBus.PublishSystemStarted();
        _eventBus.PublishSystemModeChanged(SystemMode.MainMenu);
    }

    /// <summary>
    /// Initializes the main menu as a special process with a fixed ID.
    /// This process is loaded into slot 0 and serves as the root of the MEM slot tree.
    /// </summary>
    private void InitializeMainMenu()
    {
        // Create a special process for main menu with fixed ID
        var mainMenuProcessId = "mainmenu";
        
        // Create the main menu process
        mainMenuProcessId = _processManager.CreateProcess("MainMenu", null, mainMenuProcessId);
        
        if (mainMenuProcessId == null)
        {
            GD.PrintErr("Failed to create main menu process");
            return;
        }
        
        // Load the main menu scene
        var menuScene = LoadSceneInstance("res://Scenes/MainMenu/MainMenuScene.tscn");
        if (menuScene != null)
        {
            // Store it in loaded scenes
            _loadedScenes[mainMenuProcessId] = menuScene;
            
            // Store process ID in metadata
            menuScene.SetMeta("ProcessId", mainMenuProcessId);
            
            // Set orchestrator if it's an orchestratable scene
            if (menuScene is IOrchestratableScene orchestratable)
            {
                orchestratable.SetOrchestrator(this);
            }
            
            // Connect its signals
            if (menuScene is MainMenu.MainMenuScene mainMenu)
            {
                mainMenu.MenuOptionSelected += OnMenuOptionSelected;
            }
            
            // Show the main menu
            _mainContent.AddChild(menuScene);
            ShowScene(mainMenuProcessId);
            
            // Try to load it into slot 0 if possible
            if (_slotManager != null)
            {
                var rootSlotId = "slot_0_0";
                // Make sure slot 0 exists and is unlocked
                _slotManager.UnlockSlot(rootSlotId);
                
                // Also unlock another slot for testing
                _slotManager.UnlockSlot("slot_0_1");
                
                // Start the process in the slot
                bool started = _processManager.StartProcess(mainMenuProcessId, out _);
                GD.Print($"Main menu process started in slot: {started}");
            }
            
            // Publish that main menu was loaded
            _eventBus.PublishSceneLoaded("MainMenuScene");
            _eventBus.PublishSystemModeChanged(SystemMode.MainMenu);
        }
    }

    private void OnMenuOptionSelected(string scenePath, string processType)
    {
        LoadScene(processType, scenePath);
    }

    /// <summary>
    /// Shows a scene for a specific process ID.
    /// Hides all other scenes in the main content area.
    /// </summary>
    /// <param name="processId">The ID of the process whose scene should be shown</param>
    public void ShowScene(string processId)
    {
        if (!_loadedScenes.ContainsKey(processId))
        {
            GD.PrintErr($"Scene for process {processId} not found");
            return;
        }
        
        // Hide all scenes
        foreach (var scene in _loadedScenes.Values)
        {
            scene.Visible = false;
        }
        
        // Show requested scene
        _loadedScenes[processId].Visible = true;
        _activeProcessId = processId;
        
        // Publish system mode changed based on process type
        var process = _processManager.GetProcess(processId);
        if (process != null)
        {
            SystemMode mode = process.Type switch
            {
                "MainMenu" => SystemMode.MainMenu,
                "CardGame" => SystemMode.GameSession,
                "Debug" => SystemMode.Debug,
                "Settings" => SystemMode.Settings,
                _ => SystemMode.MainMenu
            };
            _eventBus.PublishSystemModeChanged(mode);
        }
    }
    
    /// <summary>
    /// Shows a specific Control node in the main content area.
    /// Hides all other children of the main content.
    /// </summary>
    /// <param name="scene">The Control node to show</param>
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
    
    /// <summary>
    /// Returns to the main menu without unloading other scenes.
    /// This allows the player to return to the menu while keeping their game state.
    /// </summary>
    public void ReturnToMainMenu()
    {
        var mainMenuProcessId = _loadedScenes.Keys
            .FirstOrDefault(id => _processManager.GetProcess(id)?.Type == "MainMenu");
            
        if (!string.IsNullOrEmpty(mainMenuProcessId))
        {
            ShowScene(mainMenuProcessId);
            GD.Print("Returned to main menu");
        }
        else if (_mainMenuScene != null)
        {
            ShowScene(_mainMenuScene);
            GD.Print("Returned to main menu (using cached scene)");
        }
        else
        {
            GD.PrintErr("Main menu scene not found");
        }
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

    public override void _ExitTree()
    {
        // Disconnect menu signals
        if (_mainMenuScene is MainMenu.MainMenuScene mainMenu)
        {
            mainMenu.MenuOptionSelected -= OnMenuOptionSelected;
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
        _activeProcessId = null;
    }
}