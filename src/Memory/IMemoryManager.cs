// src/Memory/IMemoryManager.cs
using System.Collections.Generic;

namespace Trivale.Memory;

/// <summary>
/// Manages the system's memory slots and handles resource allocation.
/// </summary>
public interface IMemoryManager
{
    /// <summary>Maximum number of slots available</summary>
    int MaxSlots { get; }
    
    /// <summary>List of all memory slots</summary>
    IReadOnlyList<IMemorySlot> Slots { get; }
    
    /// <summary>Total system memory (1.0 = 100%)</summary>
    float TotalMemory { get; }
    
    /// <summary>Total CPU capacity (1.0 = 100%)</summary>
    float TotalCpu { get; }
    
    /// <summary>Currently available memory</summary>
    float AvailableMemory { get; }
    
    /// <summary>Currently available CPU</summary>
    float AvailableCpu { get; }
    
    /// <summary>
    /// Tries to find a suitable slot and allocate resources for a process.
    /// </summary>
    bool TryAllocateSlot(IProcess process, out IMemorySlot slot);
    
    /// <summary>
    /// Deallocates a slot and frees its resources.
    /// </summary>
    void DeallocateSlot(string slotId);
    
    /// <summary>
    /// Gets a slot by its ID.
    /// </summary>
    IMemorySlot GetSlot(string slotId);
    
    /// <summary>
    /// Gets a slot by its grid position.
    /// </summary>
    IMemorySlot GetSlotAt(int x, int y);
    
    /// <summary>
    /// Validates that a process's resource requirements can be met.
    /// </summary>
    bool ValidateResourceRequirements(Dictionary<string, float> requirements);
}
