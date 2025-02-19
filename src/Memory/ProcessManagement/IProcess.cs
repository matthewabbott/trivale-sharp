// src/Memory/ProcessManagement/IProcess.cs
using System;
using System.Collections.Generic;

namespace Trivale.Memory.ProcessManagement;

/// <summary>
/// Represents a process that can be loaded into a slot.
/// This is the base interface that all runnable processes must implement.
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

// src/Memory/ProcessManagement/BaseProcess.cs
namespace Trivale.Memory.ProcessManagement;

/// <summary>
/// Base implementation of IProcess that handles common functionality.
/// </summary>
public abstract class BaseProcess : IProcess
{
    public string Id { get; }
    public abstract string Type { get; }
    public virtual Dictionary<string, float> ResourceRequirements => new();
    public bool IsComplete { get; protected set; }
    
    protected Dictionary<string, object> State { get; private set; }
    
    public event Action<Dictionary<string, object>> StateChanged;
    
    protected BaseProcess(string id)
    {
        Id = id;
        State = new Dictionary<string, object>();
    }
    
    public virtual void Initialize(Dictionary<string, object> initialState)
    {
        State = initialState ?? new Dictionary<string, object>();
        OnInitialize();
    }
    
    public virtual void Update(float delta)
    {
        OnUpdate(delta);
    }
    
    public virtual void Cleanup()
    {
        OnCleanup();
    }
    
    public Dictionary<string, object> GetState() => new(State);
    
    protected virtual void OnInitialize() { }
    protected virtual void OnUpdate(float delta) { }
    protected virtual void OnCleanup() { }
    
    protected void EmitStateChanged()
    {
        StateChanged?.Invoke(GetState());
    }
}