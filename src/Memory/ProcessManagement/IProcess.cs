// src/Memory/ProcessManagement/IProcess.cs
using System;
using System.Collections.Generic;

namespace Trivale.Memory.ProcessManagement;

/// <summary>
/// Core unit of execution in the memory system. A process represents any program
/// that can be loaded into a memory slot, with defined resource requirements and
/// state management capabilities.
/// 
/// Processes are slot-agnostic and can be loaded into any slot meeting their
/// resource requirements. They handle their own state preservation and restoration.
/// </summary>
public interface IProcess
{
    /// <summary>Unique identifier for this process</summary>
    string Id { get; }
    
    /// <summary>Type of process (e.g., "CardGame", "Debug", etc.)</summary>
    string Type { get; }
    
    /// <summary>Resource requirements for running this process</summary>
    Dictionary<string, float> ResourceRequirements { get; }
    
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
}