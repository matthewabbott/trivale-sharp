
// src/Encounters/EncounterManager.cs
using Godot;
using System.Collections.Generic;

namespace Trivale.Encounters;

public partial class EncounterManager : Node
{
    private Dictionary<string, IEncounter> _activeEncounters = new();
    private Dictionary<string, float> _availableResources = new();
    
    private const string SIGNAL_STATE_CHANGED = "encounter_state_changed";
    private const string SIGNAL_EVENT = "encounter_event";
    
    [Signal]
    public delegate void EncounterStateChangedEventHandler(string encounterId);
    
    [Signal]
    public delegate void EncounterEventEventHandler(string encounterId, string eventType);
    
    public bool StartEncounter(IEncounter encounter, Dictionary<string, object> initialState = null)
    {
        if (!ValidateResources(encounter.ResourceRequirements))
            return false;
            
        encounter.Initialize(initialState ?? new Dictionary<string, object>());
        _activeEncounters[encounter.Id] = encounter;
        
        // Hook up events
        encounter.StateChanged += (state) => EmitSignal(SignalName.EncounterStateChanged, encounter.Id);
        encounter.EncounterEvent += (eventType) => EmitSignal(SignalName.EncounterEvent, encounter.Id, eventType);
        
        return true;
    }
    
    public void EndEncounter(string encounterId)
    {
        if (_activeEncounters.TryGetValue(encounterId, out var encounter))
        {
            encounter.Cleanup();
            _activeEncounters.Remove(encounterId);
        }
    }
    
    private bool ValidateResources(Dictionary<string, float> requirements)
    {
        foreach (var (resource, amount) in requirements)
        {
            if (!_availableResources.ContainsKey(resource) || 
                _availableResources[resource] < amount)
                return false;
        }
        return true;
    }
    
    // Called every frame
    public IEncounter GetEncounter(string encounterId)
    {
        return _activeEncounters.TryGetValue(encounterId, out var encounter) ? encounter : null;
    }
    
    public override void _Process(double delta)
    {
        foreach (var encounter in _activeEncounters.Values)
        {
            encounter.Update((float)delta);
        }
    }
}