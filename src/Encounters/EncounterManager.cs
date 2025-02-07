using Godot;
using System.Collections.Generic;

namespace Trivale.Encounters;

public partial class EncounterManager : Node
{
    private Dictionary<string, IEncounter> _activeEncounters = new();
    private Dictionary<string, float> _availableResources = new()
    {
        // Add some default resources
        ["MEM"] = 10.0f,
        ["CPU"] = 10.0f
    };
    
    private const string SIGNAL_STATE_CHANGED = "encounter_state_changed";
    private const string SIGNAL_EVENT = "encounter_event";
    
    [Signal]
    public delegate void EncounterStateChangedEventHandler(string encounterId);
    
    [Signal]
    public delegate void EncounterEventEventHandler(string encounterId, string eventType);
    
    public override void _Ready()
    {
        GD.Print("EncounterManager._Ready called");
    }
    
    public bool StartEncounter(IEncounter encounter, Dictionary<string, object> initialState = null)
    {
        GD.Print($"Starting encounter: {encounter.Id}");
        
        if (!ValidateResources(encounter.ResourceRequirements))
        {
            GD.PrintErr("Failed to validate resources:");
            foreach (var (resource, amount) in encounter.ResourceRequirements)
            {
                var available = _availableResources.GetValueOrDefault(resource, 0f);
                GD.PrintErr($"  {resource}: required={amount}, available={available}");
            }
            return false;
        }
        
        GD.Print("Resources validated");
        encounter.Initialize(initialState ?? new Dictionary<string, object>());
        _activeEncounters[encounter.Id] = encounter;
        
        // Hook up events
        encounter.StateChanged += (state) => EmitSignal(SignalName.EncounterStateChanged, encounter.Id);
        encounter.EncounterEvent += (eventType) => EmitSignal(SignalName.EncounterEvent, encounter.Id, eventType);
        
        GD.Print($"Encounter {encounter.Id} started successfully");
        return true;
    }
    
    public void EndEncounter(string encounterId)
    {
        if (_activeEncounters.TryGetValue(encounterId, out var encounter))
        {
            encounter.Cleanup();
            _activeEncounters.Remove(encounterId);
            GD.Print($"Encounter ended: {encounterId}");
        }
    }
    
    public IEncounter GetEncounter(string encounterId)
    {
        return _activeEncounters.TryGetValue(encounterId, out var encounter) ? encounter : null;
    }
    
    private bool ValidateResources(Dictionary<string, float> requirements)
    {
        foreach (var (resource, amount) in requirements)
        {
            if (!_availableResources.ContainsKey(resource) || 
                _availableResources[resource] < amount)
            {
                GD.PrintErr($"Resource validation failed for {resource}");
                return false;
            }
        }
        return true;
    }
    
    public override void _Process(double delta)
    {
        foreach (var encounter in _activeEncounters.Values)
        {
            encounter.Update((float)delta);
        }
    }
}