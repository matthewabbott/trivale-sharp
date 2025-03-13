// src/Memory/ProcessManagement/BaseProcess.cs
using System;
using System.Collections.Generic;

namespace Trivale.Memory.ProcessManagement;

/// <summary>
/// Base implementation of IProcess that handles common functionality.
/// </summary>
public abstract class BaseProcess : IProcess
{
    public string Id { get; }
    public abstract string Type { get; }
    public virtual Dictionary<string, float> ResourceRequirements => new Dictionary<string, float>();
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
        // Replace the entire state if provided, otherwise keep current
        if (initialState != null)
        {
            State = new Dictionary<string, object>(initialState);
        }
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
    
    public Dictionary<string, object> GetState() => new Dictionary<string, object>(State);
    
    protected virtual void OnInitialize() { }
    protected virtual void OnUpdate(float delta) { }
    protected virtual void OnCleanup() { }
    
    protected void EmitStateChanged()
    {
        StateChanged?.Invoke(GetState());
    }
}