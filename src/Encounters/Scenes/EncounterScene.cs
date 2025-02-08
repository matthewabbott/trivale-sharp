// src/Encounters/Scenes/EncounterScene.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Generic;

namespace Trivale.Encounters.Scenes;

/// <summary>
/// Base class for managing the scene tree representation of an encounter.
/// This handles the Godot-specific aspects of encounter visualization and interaction.
/// </summary>
public partial class EncounterScene : Node
{
    protected IEncounter Encounter { get; private set; }
    
    [Signal]
    public delegate void EncounterClosedEventHandler(string encounterId);
    
    public virtual void Initialize(IEncounter encounter)
    {
        GD.Print($"Initializing scene for encounter: {encounter.Id}");
        Encounter = encounter;
        
        // Hook up standard events
        Encounter.StateChanged += OnEncounterStateChanged;
        Encounter.EncounterEvent += OnEncounterEvent;
        
        // Initialize the encounter
        Encounter.Initialize(null);
    }
    
    public override void _ExitTree()
    {
        if (Encounter != null)
        {
            Encounter.StateChanged -= OnEncounterStateChanged;
            Encounter.EncounterEvent -= OnEncounterEvent;
            Encounter.Cleanup();
        }
    }
    
    protected virtual void OnEncounterStateChanged(Dictionary<string, object> state)
    {
        GD.Print($"Encounter {Encounter.Id} state changed");
    }
    
    protected virtual void OnEncounterEvent(string eventType)
    {
        GD.Print($"Encounter {Encounter.Id} event: {eventType}");
    }
    
    public virtual void Close()
    {
        EmitSignal(SignalName.EncounterClosed, Encounter.Id);
        QueueFree();
    }
}