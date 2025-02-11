// src/Memory/IMemorySlot.cs
using Godot;
using System.Collections.Generic;

namespace Trivale.Memory;

/// <summary>
/// Represents a single memory slot in the system that can contain and run a process.
/// Memory slots are the fundamental unit of resource management and process isolation.
/// </summary>
public interface IMemorySlot
{
    /// <summary>Unique identifier for this memory slot</summary>
    string Id { get; }
    
    /// <summary>Current status of this slot</summary>
    SlotStatus Status { get; }
    
    /// <summary>Position in the UI grid</summary>
    Vector2 Position { get; }
    
    /// <summary>Current memory usage (0.0 to 1.0)</summary>
    float MemoryUsage { get; }
    
    /// <summary>Current CPU usage (0.0 to 1.0)</summary>
    float CpuUsage { get; }
    
    /// <summary>Currently loaded process, if any</summary>
    IProcess CurrentProcess { get; }
    
    /// <summary>Whether the slot can be interacted with</summary>
    bool IsInteractable { get; }
    
    /// <summary>
    /// Checks if a process can be loaded into this slot based on resource requirements
    /// and current system state.
    /// </summary>
    bool CanLoadProcess(IProcess process);
    
    /// <summary>
    /// Loads a process into this slot. May throw if requirements aren't met.
    /// </summary>
    void LoadProcess(IProcess process);
    
    /// <summary>
    /// Unloads the current process and frees resources.
    /// </summary>
    void UnloadProcess();
    
    /// <summary>
    /// Gets the current state of the slot and its process.
    /// </summary>
    Dictionary<string, object> GetState();
    
    /// <summary>
    /// Suspends the current process if one is loaded.
    /// </summary>
    void Suspend();
    
    /// <summary>
    /// Resumes the current process if one is suspended.
    /// </summary>
    void Resume();
}