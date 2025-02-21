// src/Memory/SlotManagement/SlotManager.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Trivale.Memory.ProcessManagement;

namespace Trivale.Memory.SlotManagement;

public partial class SlotManager : Node, ISlotManager
{
    public event Action<string, SlotStatus> SlotStatusChanged;
    public event Action<string> SlotUnlocked;
    public event Action<string> SlotLocked;
    
    private readonly Dictionary<string, ISlot> _slots = new();
    private readonly int _gridWidth;
    private readonly int _gridHeight;
    private readonly float _totalMemory;
    private readonly float _totalCpu;
    
    public SlotManager(int gridWidth = 2, int gridHeight = 2, float totalMemory = 8.0f, float totalCpu = 10.0f)
    {
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
        _totalMemory = totalMemory;
        _totalCpu = totalCpu;
        
        InitializeSlots();
    }
    
    private void InitializeSlots()
    {
        float slotMemory = _totalMemory / (_gridWidth * _gridHeight);
        float slotCpu = _totalCpu / (_gridWidth * _gridHeight);
        
        for (int y = 0; y < _gridHeight; y++)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                string id = $"slot_{x}_{y}";
                bool isFirstSlot = (x == 0 && y == 0);
                
                var slot = new Slot(
                    id: id,
                    position: new Vector2I(x, y),
                    maxMemory: slotMemory,
                    maxCpu: slotCpu,
                    startUnlocked: isFirstSlot
                );
                
                _slots[id] = slot;
            }
        }
    }
    
    public bool TryLoadProcessIntoSlot(IProcess process, out string slotId)
    {
        slotId = null;
        
        if (!CanAllocateProcess(process))
            return false;
            
        var slot = _slots.Values.FirstOrDefault(s => s.CanLoadProcess(process));
        if (slot == null)
            return false;
            
        try
        {
            slot.LoadProcess(process);
            slotId = slot.Id;
            SlotStatusChanged?.Invoke(slot.Id, slot.Status);
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load process: {e.Message}");
            return false;
        }
    }
    
    public void FreeSlot(string slotId)
    {
        if (!_slots.TryGetValue(slotId, out var slot))
            return;
            
        try
        {
            slot.UnloadProcess();
            SlotStatusChanged?.Invoke(slotId, slot.Status);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error freeing slot {slotId}: {e.Message}");
            throw;
        }
    }
    
    public bool UnlockSlot(string slotId)
    {
        if (!_slots.TryGetValue(slotId, out var slot))
            return false;
            
        if (slot is Slot mutableSlot)
        {
            mutableSlot.Unlock();
            SlotUnlocked?.Invoke(slotId);
            SlotStatusChanged?.Invoke(slotId, slot.Status);
            return true;
        }
        
        return false;
    }
    
    // New method to lock a slot
    public bool LockSlot(string slotId)
    {
        if (!_slots.TryGetValue(slotId, out var slot))
            return false;
            
        // Can only lock empty slots
        if (slot.Status != SlotStatus.Empty)
            return false;
            
        if (slot is Slot mutableSlot)
        {
            // Call the new Lock method we'll add to the Slot class
            if (mutableSlot.Lock())
            {
                SlotLocked?.Invoke(slotId);
                SlotStatusChanged?.Invoke(slotId, mutableSlot.Status);
                return true;
            }
        }
        
        return false;
    }
    
    public IReadOnlyList<ISlot> GetAllSlots() => _slots.Values.ToList();
    
    public ISlot GetSlot(string slotId) => 
        _slots.TryGetValue(slotId, out var slot) ? slot : null;
    
    public bool CanAllocateProcess(IProcess process)
    {
        if (process?.ResourceRequirements == null)
            return false;
            
        if (!process.ResourceRequirements.TryGetValue("MEM", out float memReq) ||
            !process.ResourceRequirements.TryGetValue("CPU", out float cpuReq))
        {
            return false;
        }
        
        return memReq <= GetAvailableMemory() && cpuReq <= GetAvailableCpu();
    }
    
    public float GetAvailableMemory() =>
        _totalMemory - _slots.Values.Sum(s => s.MemoryUsage);
        
    public float GetAvailableCpu() =>
        _totalCpu - _slots.Values.Sum(s => s.CpuUsage);
        
    // New method to create additional slots
    public string CreateNewSlot(bool startUnlocked = false)
    {
        // Generate a new position that's not currently used
        Vector2I position = FindNextAvailablePosition();
        
        // Calculate resources for the new slot
        float slotMemory = _totalMemory / (_slots.Count + 1); // Divide available memory
        float slotCpu = _totalCpu / (_slots.Count + 1);       // Divide available CPU
        
        // Create a unique ID
        string id = $"slot_{position.X}_{position.Y}";
        
        // Create the new slot
        var slot = new Slot(
            id: id,
            position: position,
            maxMemory: slotMemory,
            maxCpu: slotCpu,
            startUnlocked: startUnlocked
        );
        
        // Add to our collection
        _slots[id] = slot;
        
        // Emit appropriate event
        if (startUnlocked)
        {
            SlotUnlocked?.Invoke(id);
        }
        
        SlotStatusChanged?.Invoke(id, slot.Status);
        
        return id;
    }
    
    private Vector2I FindNextAvailablePosition()
    {
        // First try to fill in any "holes" in the grid
        for (int y = 0; y < 100; y++) // Reasonable limit to prevent infinite loop
        {
            for (int x = 0; x < 100; x++)
            {
                string testId = $"slot_{x}_{y}";
                if (!_slots.ContainsKey(testId))
                {
                    return new Vector2I(x, y);
                }
            }
        }
        
        // If we somehow get here, just append to the end
        return new Vector2I(0, _slots.Count);
    }
}