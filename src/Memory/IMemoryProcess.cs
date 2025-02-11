// src/Memory/IProcess.cs
using System;
using System.Collections.Generic;

namespace Trivale.Memory;

/// <summary>
/// Represents a process that can be loaded into a memory slot.
/// This is the base interface that all runnable processes must implement.
/// </summary>
public interface IProcess
{
    /// <summary>Unique identifier for this process</summary>
    string Id { get; }
    
    /// <summary>Type of process (e.g., "CardGame", "Market", etc.)</summary>
    string Type { get; }
    
    /// <summary>Resource requirements for running this process</summary>
    Dictionary<string, float> ResourceRequirements { get; }
    
    /// <summary>Memory slot this process is loaded into, if any</summary>
    IMemorySlot Slot { get; }
    
    /// <summary>Whether the process has completed its task</summary>
    bool IsComplete { get; }
    
    /// <summary>
    /// Initializes the process with the given state.
    /// </summary>
    void Initialize(Dictionary<string, object> state);
    
    /// <summary>
    /// Gets the current process state for saving/restoration.
    /// </summary>
    Dictionary<string, object> GetState();
    
    /// <summary>
    /// Called when the process should update its state.
    /// </summary>
    void Update(float delta);
    
    /// <summary>
    /// Cleans up resources when the process is unloaded.
    /// </summary>
    void Cleanup();
    
    /// <summary>Called when process state has changed</summary>
    event Action<Dictionary<string, object>> StateChanged;
    
    /// <summary>Called for process-specific events</summary>
    event Action<string> ProcessEvent;
}