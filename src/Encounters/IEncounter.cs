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
