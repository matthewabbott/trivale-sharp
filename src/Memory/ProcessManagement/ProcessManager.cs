// src/Memory/ProcessManagement/ProcessManager.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Trivale.Memory.SlotManagement;
using Trivale.OS.MainMenu.Processes;

namespace Trivale.Memory.ProcessManagement;

/// <summary>
/// Manages process lifecycle and coordinates with the slot system. Acts as the
/// central authority for process creation, loading, and cleanup.
/// 
/// The ProcessManager owns the relationship between processes and slots, delegating
/// slot management to the SlotManager while maintaining the process-to-slot mapping
/// and handling resource cleanup.
/// </summary>
public partial class ProcessManager : Node, IProcessManager
{
    public event Action<string, string> ProcessStarted;
    public event Action<string> ProcessEnded;
    public event Action<string> ProcessStateChanged;
   
    private readonly Dictionary<string, IProcess> _processes = new();
    private readonly ISlotManager _slotManager;
    private readonly Dictionary<string, string> _processToSlot = new();
   
    public ProcessManager(ISlotManager slotManager)
    {
        _slotManager = slotManager;
    }

    public string CreateProcess(string processType, Dictionary<string, object> initParams = null)
    {
        var processId = $"{processType.ToLower()}_{DateTime.Now.Ticks}";
        
        IProcess newProcess = processType switch
        {
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
        _processes[processId] = newProcess;
        
        GD.Print($"Created process: {processId}");
        return processId;
    }

    public bool StartProcess(string processId, out string slotId)
    {
        slotId = null;
        if (!_processes.TryGetValue(processId, out var process))
        {
            GD.PrintErr($"Process not found: {processId}");
            return false;
        }
        
        // Try to load into a slot
        if (!_slotManager.TryLoadProcessIntoSlot(process, out slotId))
        {
            GD.PrintErr("Failed to load process into any slot.");
            return false;
        }
        
        _processToSlot[processId] = slotId;
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
        }
        
        process.Cleanup();
        _processes.Remove(processId);
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
        ProcessStateChanged?.Invoke(processId);
    }
}