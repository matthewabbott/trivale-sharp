// src/Memory/ProcessManagement/ProcessManager.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Trivale.Memory.SlotManagement;
using Trivale.OS.Events;
using Trivale.OS.MainMenu.Processes;

namespace Trivale.Memory.ProcessManagement;

/// <summary>
/// Manages process lifecycle and coordinates with the slot system. Acts as the
/// central authority for process creation, loading, and cleanup.
/// 
/// The ProcessManager owns the relationship between processes and slots, delegating
/// slot management to the SlotManager while maintaining the process-to-slot mapping
/// and handling resource cleanup.
/// 
/// This version is decoupled from UI and uses the SystemEventBus to communicate
/// state changes, rather than direct event callbacks.
/// </summary>
public partial class ProcessManager : Node, IProcessManager
{
    // These events are kept for backward compatibility but should be replaced with the event bus
    public event Action<string, string> ProcessStarted;
    public event Action<string> ProcessEnded;
    public event Action<string> ProcessStateChanged;
   
    private readonly Dictionary<string, IProcess> _processes = new();
    private readonly ISlotManager _slotManager;
    private readonly Dictionary<string, string> _processToSlot = new();
    private readonly SystemEventBus _eventBus;
    private readonly ProcessSlotRegistry _registry;
   
    public ProcessManager(ISlotManager slotManager, ProcessSlotRegistry registry)
    {
        _slotManager = slotManager;
        _registry = registry;
        _eventBus = SystemEventBus.Instance;
    }

    public string CreateProcess(string processType, Dictionary<string, object> initParams = null, string specificId = null)
    {
        var processId = specificId ?? $"{processType.ToLower()}_{DateTime.Now.Ticks}";
        
        IProcess newProcess = processType switch
        {
            "MainMenu" => new MainMenuProcess(processId),
            "CardGame" => new CardGameMenuProcess(processId),
            "Debug" => new DebugMenuProcess(processId),
            _ => null
        };
        
        if (newProcess == null)
        {
            GD.PrintErr($"Unknown process type: {processType}");
            return null;
        }

        // Hook up state change events from the process
        newProcess.StateChanged += (state) => OnProcessStateChanged(processId, state);
        
        // Initialize the process with provided state or empty state
        newProcess.Initialize(initParams ?? new Dictionary<string, object>());
        
        _processes[processId] = newProcess;
        
        GD.Print($"Created process: {processId}");
        
        // Publish event through the bus
        _eventBus.PublishProcessCreated(processId);
        
        return processId;
    }

    public bool StartProcess(string processId, out string slotId)
    {
        return StartProcess(processId, null, out slotId);
    }

    public bool StartProcess(string processId, string preferredSlotId, out string slotId)
    {
        slotId = null;
        if (!_processes.TryGetValue(processId, out var process))
        {
            GD.PrintErr($"Process not found: {processId}");
            return false;
        }
        
        // Try to load into specific slot if provided
        if (!string.IsNullOrEmpty(preferredSlotId))
        {
            var slot = _slotManager.GetSlot(preferredSlotId);
            if (slot != null && slot.CanLoadProcess(process))
            {
                try
                {
                    slot.LoadProcess(process);
                    slotId = preferredSlotId;
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to load process into preferred slot: {ex.Message}");
                    return false;
                }
            }
            else
            {
                GD.PrintErr($"Preferred slot {preferredSlotId} cannot load this process");
                return false;
            }
        }
        // Otherwise try to load into any available slot
        else if (!_slotManager.TryLoadProcessIntoSlot(process, out slotId))
        {
            GD.PrintErr("Failed to load process into any slot.");
            return false;
        }
        
        _processToSlot[processId] = slotId;
        
        // Register the process-slot mapping in the registry
        _registry.RegisterProcessSlot(processId, slotId);
        
        // Publish event through the bus
        _eventBus.PublishProcessStarted(processId, slotId);
        
        // Legacy event invocation
        ProcessStarted?.Invoke(processId, slotId);
        
        return true;
    }

    public bool UnloadProcess(string processId)
    {
        if (!_processes.TryGetValue(processId, out var process))
        {
            GD.PrintErr($"Process not found: {processId}");
            return false;
        }
        
        if (_processToSlot.TryGetValue(processId, out var slotId))
        {
            _slotManager.FreeSlot(slotId);
            _processToSlot.Remove(processId);
            
            // Unregister from the registry
            _registry.UnregisterProcess(processId);
        }
        
        process.Cleanup();
        _processes.Remove(processId);
        
        // Publish event through the bus
        _eventBus.PublishProcessEnded(processId);
        
        // Legacy event invocation
        ProcessEnded?.Invoke(processId);
        
        return true;
    }
   
    public IProcess GetProcess(string processId)
    {
        return _processes.TryGetValue(processId, out var process) ? process : null;
    }
   
    public IReadOnlyList<string> GetActiveProcessIds()
    {
        return _processes.Keys.ToList();
    }
   
    private void OnProcessStateChanged(string processId, Dictionary<string, object> newState)
    {
        // Publish state change through the bus
        _eventBus.PublishProcessStateChanged(processId, newState);
        
        // Legacy event invocation
        ProcessStateChanged?.Invoke(processId);
    }
    
    public override void _ExitTree()
    {
        // Clean up all processes
        foreach (var processId in GetActiveProcessIds().ToList())
        {
            UnloadProcess(processId);
        }
        
        base._ExitTree();
    }
    
    // Add a helper method to get the slot ID for a process
    public string GetProcessSlotId(string processId)
    {
        return _processToSlot.TryGetValue(processId, out var slotId) ? slotId : null;
    }
}