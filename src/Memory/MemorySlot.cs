// src/Memory/MemorySlot.cs
using Godot;
using System;
using System.Collections.Generic;

namespace Trivale.Memory;

public class MemorySlot : IMemorySlot
{
    public string Id { get; }
    public SlotStatus Status { get; private set; }
    public Vector2 Position { get; }
    public float MemoryUsage { get; private set; }
    public float CpuUsage { get; private set; }
    public IProcess CurrentProcess { get; private set; }
    public bool IsInteractable => Status != SlotStatus.Locked && Status != SlotStatus.Loading;

    private readonly float _maxMemory;
    private readonly float _maxCpu;
    private Dictionary<string, object> _savedState;
    
    public MemorySlot(string id, Vector2 position, float maxMemory = 1.0f, float maxCpu = 1.0f)
    {
        Id = id;
        Position = position;
        _maxMemory = maxMemory;
        _maxCpu = maxCpu;
        Status = SlotStatus.Empty;
        _savedState = new Dictionary<string, object>();
    }
    
    public bool CanLoadProcess(IProcess process)
    {
        if (process == null)
            return false;
            
        if (Status != SlotStatus.Empty && Status != SlotStatus.Corrupted)
            return false;
            
        // Validate resource requirements
        if (!process.ResourceRequirements.TryGetValue("MEM", out float memReq) ||
            !process.ResourceRequirements.TryGetValue("CPU", out float cpuReq))
        {
            return false;
        }
        
        return memReq <= _maxMemory && cpuReq <= _maxCpu;
    }
    
    public void LoadProcess(IProcess process)
    {
        if (!CanLoadProcess(process))
            throw new InvalidOperationException($"Cannot load process {process?.Id} into slot {Id}");
            
        try
        {
            Status = SlotStatus.Loading;
            
            // Set up process
            CurrentProcess = process;
            UpdateResourceUsage();
            
            // Initialize with saved state if available
            if (_savedState.Count > 0)
            {
                process.Initialize(_savedState);
                _savedState.Clear();
            }
            else
            {
                process.Initialize(null);
            }
            
            Status = SlotStatus.Active;
            GD.Print($"Process {process.Id} loaded into slot {Id}");
        }
        catch (Exception e)
        {
            Status = SlotStatus.Corrupted;
            CurrentProcess = null;
            GD.PrintErr($"Failed to load process {process?.Id} into slot {Id}: {e.Message}");
            throw;
        }
    }
    
    public void UnloadProcess()
    {
        if (CurrentProcess == null)
            return;
            
        try
        {
            // Save state before unloading
            _savedState = CurrentProcess.GetState();
            
            CurrentProcess.Cleanup();
            CurrentProcess = null;
            MemoryUsage = 0;
            CpuUsage = 0;
            Status = SlotStatus.Empty;
        }
        catch (Exception e)
        {
            Status = SlotStatus.Corrupted;
            GD.PrintErr($"Error unloading process from slot {Id}: {e.Message}");
            throw;
        }
    }
    
    public Dictionary<string, object> GetState()
    {
        var state = new Dictionary<string, object>
        {
            ["status"] = Status,
            ["memoryUsage"] = MemoryUsage,
            ["cpuUsage"] = CpuUsage
        };
        
        if (CurrentProcess != null)
        {
            state["processState"] = CurrentProcess.GetState();
        }
        else if (_savedState.Count > 0)
        {
            state["savedState"] = _savedState;
        }
        
        return state;
    }
    
    public void Suspend()
    {
        if (Status != SlotStatus.Active || CurrentProcess == null)
            return;
            
        _savedState = CurrentProcess.GetState();
        Status = SlotStatus.Suspended;
        UpdateResourceUsage(cpuMultiplier: 0.1f); // Minimal CPU usage while suspended
    }
    
    public void Resume()
    {
        if (Status != SlotStatus.Suspended || CurrentProcess == null)
            return;
            
        CurrentProcess.Initialize(_savedState);
        _savedState.Clear();
        Status = SlotStatus.Active;
        UpdateResourceUsage();
    }
    
    private void UpdateResourceUsage(float memMultiplier = 1.0f, float cpuMultiplier = 1.0f)
    {
        if (CurrentProcess?.ResourceRequirements == null)
        {
            MemoryUsage = 0;
            CpuUsage = 0;
            return;
        }
        
        if (CurrentProcess.ResourceRequirements.TryGetValue("MEM", out float memReq))
        {
            MemoryUsage = memReq * memMultiplier;
        }
        
        if (CurrentProcess.ResourceRequirements.TryGetValue("CPU", out float cpuReq))
        {
            CpuUsage = cpuReq * cpuMultiplier;
        }
    }
}