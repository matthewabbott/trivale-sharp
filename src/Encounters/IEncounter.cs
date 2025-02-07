// src/Encounters/IEncounter.cs
using System;
using System.Collections.Generic;

namespace Trivale.Encounters;

public interface IEncounter
{
	string Id { get; }
	string Type { get; }
	Dictionary<string, float> ResourceRequirements { get; }
	bool IsComplete { get; }
	
	void Initialize(Dictionary<string, object> initialState);
	void Update(float delta);
	void Cleanup();
	
	// State persistence
	Dictionary<string, object> GetState();
	void RestoreState(Dictionary<string, object> state);
	
	// Events
	event Action<Dictionary<string, object>> StateChanged;
	event Action<string> EncounterEvent;
}

// src/Encounters/EncounterManager.cs
using Godot;
using System.Collections.Generic;

namespace Trivale.Encounters;

public partial class EncounterManager : Node
{
	private Dictionary<string, IEncounter> _activeEncounters = new();
	private Dictionary<string, float> _availableResources = new();
	
	// Events
	[Signal]
	public delegate void EncounterStateChangedEventHandler(string encounterId, Dictionary<string, object> state);
	
	[Signal]
	public delegate void EncounterEventEventHandler(string encounterId, string eventType);
	
	public bool StartEncounter(IEncounter encounter, Dictionary<string, object> initialState = null)
	{
		if (!ValidateResources(encounter.ResourceRequirements))
			return false;
			
		encounter.Initialize(initialState ?? new Dictionary<string, object>());
		_activeEncounters[encounter.Id] = encounter;
		
		// Hook up events
		encounter.StateChanged += (state) => EmitSignal(SignalName.EncounterStateChanged, encounter.Id, state);
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
	public override void _Process(double delta)
	{
		foreach (var encounter in _activeEncounters.Values)
		{
			encounter.Update((float)delta);
		}
	}
}

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
