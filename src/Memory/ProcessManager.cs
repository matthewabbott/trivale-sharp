// src/Memory/ProcessManager.cs
using Godot;
using System;
using System.Collections.Generic;
using Trivale.Encounters.Scenes;

namespace Trivale.Memory;

public partial class ProcessManager : Node
{
    private IMemoryManager _memoryManager;
    private Dictionary<string, ProcessSceneInfo> _activeProcesses = new();
    
    private struct ProcessSceneInfo
    {
        public IProcess Process;
        public ProcessScene Scene;
        public IMemorySlot Slot;
    }
    
    [Signal]
    public delegate void ProcessStateChangedEventHandler(string processId);
    
    [Signal]
    public delegate void ProcessEventEventHandler(string processId, string eventType);
    
    public override void _Ready()
    {
        _memoryManager = new MemoryManager();  // Could inject this later if needed
    }
    
    public bool StartProcess(IProcess process, Dictionary<string, object> initialState = null)
    {
        GD.Print($"Starting process: {process.Id}");
        
        if (!_memoryManager.TryAllocateSlot(process, out var slot))
        {
            GD.PrintErr("Failed to allocate memory slot:");
            GD.PrintErr($"  Required: {string.Join(", ", process.ResourceRequirements)}");
            GD.PrintErr($"  Available Memory: {_memoryManager.AvailableMemory}");
            GD.PrintErr($"  Available CPU: {_memoryManager.AvailableCpu}");
            return false;
        }
        
        GD.Print("Memory slot allocated");
        
        // Create the appropriate scene manager based on process type
        ProcessScene scene = process.Type switch
        {
            "CardGame" => new CardProcessScene(),
            _ => throw new ArgumentException($"Unknown process type: {process.Type}")
        };
        
        // Add scene to tree and initialize it
        AddChild(scene);
        scene.Initialize(process);
        
        // Store process info
        _activeProcesses[process.Id] = new ProcessSceneInfo
        {
            Process = process,
            Scene = scene,
            Slot = slot
        };
        
        // Hook up scene events
        scene.ProcessClosed += OnProcessClosed;
        
        GD.Print($"Process {process.Id} started successfully");
        return true;
    }
    
    private void OnProcessClosed(string processId)
    {
        if (_activeProcesses.TryGetValue(processId, out var processInfo))
        {
            processInfo.Process.Cleanup();
            _memoryManager.DeallocateSlot(processInfo.Slot.Id);
            _activeProcesses.Remove(processId);
            GD.Print($"Process cleaned up: {processId}");
        }
    }
    
    public IProcess GetProcess(string processId)
    {
        return _activeProcesses.TryGetValue(processId, out var info) ? info.Process : null;
    }
    
    public IMemorySlot GetProcessSlot(string processId)
    {
        return _activeProcesses.TryGetValue(processId, out var info) ? info.Slot : null;
    }
    
    public ProcessScene GetProcessScene(string processId)
    {
        return _activeProcesses.TryGetValue(processId, out var info) ? info.Scene : null;
    }
    
    public IReadOnlyList<IMemorySlot> GetAllSlots() => _memoryManager.Slots;
    
    public override void _Process(double delta)
    {
        foreach (var (processId, info) in _activeProcesses)
        {
            info.Process.Update((float)delta);
        }
    }
    
    public override void _ExitTree()
    {
        // Clean up all active processes
        foreach (var (processId, info) in _activeProcesses)
        {
            info.Scene.Close();
        }
        _activeProcesses.Clear();
    }
}
