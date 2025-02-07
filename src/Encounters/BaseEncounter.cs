// src/Encounters/BaseEncounter.cs
using System;
using System.Collections.Generic;

namespace Trivale.Encounters;

public abstract class BaseEncounter : IEncounter
{
    public string Id { get; }
    public abstract string Type { get; }
    public virtual Dictionary<string, float> ResourceRequirements => new();
    public bool IsComplete { get; protected set; }
    
    protected Dictionary<string, object> State { get; private set; }
    
    public event Action<Dictionary<string, object>> StateChanged;
    public event Action<string> EncounterEvent;
    
    protected BaseEncounter(string id)
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
    
    public virtual void RestoreState(Dictionary<string, object> state)
    {
        State = new Dictionary<string, object>(state);
        OnStateRestored();
    }
    
    protected virtual void OnInitialize() { }
    protected virtual void OnUpdate(float delta) { }
    protected virtual void OnCleanup() { }
    protected virtual void OnStateRestored() { }
    
    protected void EmitStateChanged()
    {
        StateChanged?.Invoke(GetState());
    }
    
    protected void EmitEncounterEvent(string eventType)
    {
        EncounterEvent?.Invoke(eventType);
    }
}