// src/Memory/BaseProcess.cs
using System;
using System.Collections.Generic;

namespace Trivale.Memory;

public abstract class BaseProcess : IProcess
{
    public string Id { get; }
    public abstract string Type { get; }
    public virtual Dictionary<string, float> ResourceRequirements => new();
    public bool IsComplete { get; protected set; }
    public IMemorySlot Slot { get; set; }  // Added for IProcess implementation
    
    protected Dictionary<string, object> State { get; private set; }
    
    public event Action<Dictionary<string, object>> StateChanged;
    public event Action<string> ProcessEvent;  // Renamed from EncounterEvent
    
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
    
    protected void EmitProcessEvent(string eventType)  // Renamed from EmitEncounterEvent
    {
        ProcessEvent?.Invoke(eventType);
    }
}
