// src/Encounters/EncounterManager.cs
using Godot;
using System.Collections.Generic;
using Trivale.Encounters.Scenes;

namespace Trivale.Encounters;

public partial class EncounterManager : Node
{
    private Dictionary<string, (IEncounter encounter, EncounterScene scene)> _activeEncounters = new();
    private Dictionary<string, float> _availableResources = new()
    {
        // Add some default resources
        ["MEM"] = 10.0f,
        ["CPU"] = 10.0f
    };
    
    [Signal]
    public delegate void EncounterStateChangedEventHandler(string encounterId);
    
    [Signal]
    public delegate void EncounterEventEventHandler(string encounterId, string eventType);
    
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
        
        // Create the appropriate scene manager based on encounter type
        EncounterScene scene = encounter.Type switch
        {
            "CardGame" => new CardEncounterScene(),
            _ => throw new System.ArgumentException($"Unknown encounter type: {encounter.Type}")
        };
        
        // Add scene to tree and initialize it
        AddChild(scene);
        scene.Initialize(encounter);
        
        // Store the encounter and its scene
        _activeEncounters[encounter.Id] = (encounter, scene);
        
        // Hook up scene events
        scene.EncounterClosed += OnEncounterClosed;
        
        GD.Print($"Encounter {encounter.Id} started successfully");
        return true;
    }
    
    private void OnEncounterClosed(string encounterId)
    {
        if (_activeEncounters.TryGetValue(encounterId, out var encounterData))
        {
            encounterData.encounter.Cleanup();
            _activeEncounters.Remove(encounterId);
            GD.Print($"Encounter removed: {encounterId}");
        }
    }
    
    public IEncounter GetEncounter(string encounterId)
    {
        return _activeEncounters.TryGetValue(encounterId, out var encounterData) ? encounterData.encounter : null;
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
        foreach (var (encounter, _) in _activeEncounters.Values)
        {
            encounter.Update((float)delta);
        }
    }
    
    public override void _ExitTree()
    {
        // Clean up all active encounters
        foreach (var (_, scene) in _activeEncounters.Values)
        {
            scene.Close();
        }
        _activeEncounters.Clear();
    }
}