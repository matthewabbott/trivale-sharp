// src/OS/SceneOrchestrator.cs
using Godot;
using System.Collections.Generic;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;

namespace Trivale.OS;

/// <summary>
/// Centralizes scene management and coordinates with ProcessManager while keeping
/// UI and process logic separate. Handles scene lifecycle including loading,
/// unloading, and state management.
/// 
/// Key responsibilities:
/// - Loads and unloads scenes
/// - Manages scene-to-process relationships
/// - Coordinates cleanup between process and scene systems
/// - Maintains references to loaded scenes
/// </summary>
public partial class SceneOrchestrator : Node
{
    private Dictionary<string, Node> _loadedScenes = new();
    private IProcessManager _processManager;
    private ISlotManager _slotManager;
    private Control _mainContent;
    
    [Signal]
    public delegate void SceneUnloadedEventHandler();

    public void Initialize(IProcessManager processManager, ISlotManager slotManager, Control mainContent)
    {
        _processManager = processManager;
        _slotManager = slotManager;
        _mainContent = mainContent;
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

        // Initialize if it's a DebugScene
        if (scene is MainMenu.DebugScene debugScene)
        {
            debugScene.Initialize(_processManager, _slotManager);
        }

        // Connect to scene's unload signal
        if (scene.HasSignal("SceneUnloadRequested"))
        {
            scene.Connect("SceneUnloadRequested", new Callable(this, nameof(HandleSceneUnloadRequest)));
        }

        // Show the scene
        ShowScene(scene);
        return true;
    }

    private Node LoadSceneInstance(string scenePath)
    {
        var sceneResource = ResourceLoader.Load<PackedScene>(scenePath);
        if (sceneResource == null)
        {
            GD.PrintErr($"Failed to load scene: {scenePath}");
            return null;
        }

        var instance = sceneResource.Instantiate();
        
        // Set up Control nodes properly
        if (instance is Control control)
        {
            control.SizeFlagsHorizontal = Control.SizeFlags.Fill;
            control.SizeFlagsVertical = Control.SizeFlags.Fill;
            control.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
            control.GrowHorizontal = Control.GrowDirection.Both;
            control.GrowVertical = Control.GrowDirection.Both;
        }

        return instance;
    }

    private void ShowScene(Node scene)
    {
        // Clear existing content
        foreach (Node child in _mainContent.GetChildren())
        {
            child.QueueFree();
        }

        _mainContent.AddChild(scene);
    }

    private void HandleSceneUnloadRequest()
    {
        // Find the process that owns this scene
        string processToUnload = null;
        foreach (var kvp in _loadedScenes)
        {
            if (kvp.Value.IsInsideTree())
            {
                processToUnload = kvp.Key;
                break;
            }
        }

        if (processToUnload != null)
        {
            // Unload process first
            _processManager.UnloadProcess(processToUnload);
            
            // Then clean up scene
            if (_loadedScenes.TryGetValue(processToUnload, out var scene))
            {
                _loadedScenes.Remove(processToUnload);
                scene.QueueFree();
            }
        }

        EmitSignal(SignalName.SceneUnloaded);
    }

    public override void _ExitTree()
    {
        // Clean up any remaining scenes
        foreach (var scene in _loadedScenes.Values)
        {
            if (scene.IsInsideTree())
            {
                scene.QueueFree();
            }
        }
        _loadedScenes.Clear();
    }
}