// src/Memory/ProcessManagement/ProcessSlotRegistry.cs
using System;
using System.Collections.Generic;
using Godot;

namespace Trivale.Memory.ProcessManagement;

/// <summary>
/// Maintains a registry of which process is loaded into which memory slot.
/// Acts as the central source of truth for process-slot mappings.
/// </summary>
public class ProcessSlotRegistry
{
    private Dictionary<string, string> _processToSlot = new();
    private Dictionary<string, string> _slotToProcess = new();
    
    // Event raised when a process is mapped to or unmapped from a slot
    public event Action<string, string> ProcessSlotMappingChanged; // processId, slotId
    
    // Event raised when the active process changes
    public event Action<string> ActiveProcessChanged; // processId
    
    // The currently active process
    public string ActiveProcessId { get; private set; }
    
    /// <summary>
    /// Registers a process as being loaded in a specific slot.
    /// </summary>
    public void RegisterProcessSlot(string processId, string slotId)
    {
        if (string.IsNullOrEmpty(processId) || string.IsNullOrEmpty(slotId))
            return;
            
        _processToSlot[processId] = slotId;
        _slotToProcess[slotId] = processId;
        
        ProcessSlotMappingChanged?.Invoke(processId, slotId);
        GD.Print($"Process {processId} registered in slot {slotId}");
    }
    
    /// <summary>
    /// Unregisters a process, removing it from any associated slot.
    /// </summary>
    public void UnregisterProcess(string processId)
    {
        if (string.IsNullOrEmpty(processId))
            return;
            
        if (_processToSlot.TryGetValue(processId, out var slotId))
        {
            _processToSlot.Remove(processId);
            _slotToProcess.Remove(slotId);
            
            ProcessSlotMappingChanged?.Invoke(processId, null);
            GD.Print($"Process {processId} unregistered from slot {slotId}");
            
            // If this was the active process, clear that reference
            if (processId == ActiveProcessId)
            {
                SetActiveProcess(null);
            }
        }
    }

    /// <summary>
    /// Sets a process as the currently active one.
    /// </summary>
    public void SetActiveProcess(string processId)
    {
        if (ActiveProcessId == processId)
            return;
            
        ActiveProcessId = processId;
        ActiveProcessChanged?.Invoke(processId);
        GD.Print($"Active process set to: {processId ?? "none"}");
    }
    
    /// <summary>
    /// Gets the slot ID where a process is loaded.
    /// </summary>
    public string GetSlotForProcess(string processId) => 
        string.IsNullOrEmpty(processId) ? null : 
            _processToSlot.TryGetValue(processId, out var slotId) ? slotId : null;
    
    /// <summary>
    /// Gets the process ID loaded in a slot.
    /// </summary>
    public string GetProcessForSlot(string slotId) => 
        string.IsNullOrEmpty(slotId) ? null : 
            _slotToProcess.TryGetValue(slotId, out var processId) ? processId : null;
            
    /// <summary>
    /// Checks if a slot has a process loaded.
    /// </summary>
    public bool IsSlotOccupied(string slotId) => 
        !string.IsNullOrEmpty(slotId) && _slotToProcess.ContainsKey(slotId);
}