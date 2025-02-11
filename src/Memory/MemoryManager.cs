// src/Memory/MemoryManager.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Trivale.Memory;

public class MemoryManager : IMemoryManager
{
    public int MaxSlots => _slots.Count;
    public IReadOnlyList<IMemorySlot> Slots => _slots.AsReadOnly();
    public float TotalMemory { get; }
    public float TotalCpu { get; }
    public float AvailableMemory => TotalMemory - _slots.Sum(s => s.MemoryUsage);
    public float AvailableCpu => TotalCpu - _slots.Sum(s => s.CpuUsage);

    private readonly List<IMemorySlot> _slots;
    private readonly int _gridWidth;
    private readonly int _gridHeight;
    private readonly float _slotMemory;  // Memory per slot
    private readonly float _slotCpu;     // CPU per slot

    public MemoryManager(int gridWidth = 2, int gridHeight = 2, float totalMemory = 8.0f, float totalCpu = 10.0f)
    {
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
        TotalMemory = totalMemory;
        TotalCpu = totalCpu;

        // Calculate per-slot resources
        _slotMemory = totalMemory / (gridWidth * gridHeight);
        _slotCpu = totalCpu / (gridWidth * gridHeight);

        // Initialize slot grid
        _slots = CreateSlotGrid();
    }

    public bool TryAllocateSlot(IProcess process, out IMemorySlot slot)
    {
        slot = null;
        
        // Validate overall resource requirements
        if (!ValidateResourceRequirements(process.ResourceRequirements))
            return false;

        // Find first available slot that can handle the process
        slot = _slots.FirstOrDefault(s => s.CanLoadProcess(process));
        if (slot == null)
            return false;

        try
        {
            slot.LoadProcess(process);
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load process {process.Id}: {e.Message}");
            slot = null;
            return false;
        }
    }

    public void DeallocateSlot(string slotId)
    {
        var slot = GetSlot(slotId);
        if (slot == null)
            return;

        try
        {
            slot.UnloadProcess();
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error deallocating slot {slotId}: {e.Message}");
            // Let the exception propagate - caller should handle cleanup
            throw;
        }
    }

    public IMemorySlot GetSlot(string slotId)
    {
        return _slots.FirstOrDefault(s => s.Id == slotId);
    }

    public IMemorySlot GetSlotAt(int x, int y)
    {
        if (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight)
            return null;

        int index = y * _gridWidth + x;
        return index < _slots.Count ? _slots[index] : null;
    }

    public bool ValidateResourceRequirements(Dictionary<string, float> requirements)
    {
        if (requirements == null)
            return false;

        // Check memory requirement
        if (requirements.TryGetValue("MEM", out float memReq))
        {
            if (memReq > AvailableMemory)
                return false;
        }

        // Check CPU requirement
        if (requirements.TryGetValue("CPU", out float cpuReq))
        {
            if (cpuReq > AvailableCpu)
                return false;
        }

        return true;
    }

    private List<IMemorySlot> CreateSlotGrid()
    {
        var slots = new List<IMemorySlot>();

        for (int y = 0; y < _gridHeight; y++)
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                var position = new Vector2(x, y);
                var slot = new MemorySlot(
                    id: $"SLOT_{x}_{y}",
                    position: position,
                    maxMemory: _slotMemory,
                    maxCpu: _slotCpu
                );
                slots.Add(slot);
            }
        }

        return slots;
    }

    // Helper methods for getting slot information
    public int GetSlotX(IMemorySlot slot) => (int)slot.Position.X;
    public int GetSlotY(IMemorySlot slot) => (int)slot.Position.Y;
    public bool IsSlotPositionValid(int x, int y) => x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight;
    public float GetTotalUsedMemory() => _slots.Sum(s => s.MemoryUsage);
    public float GetTotalUsedCpu() => _slots.Sum(s => s.CpuUsage);
}