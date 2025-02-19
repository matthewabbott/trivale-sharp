// src/UI/Components/SlotGridSystem.cs
// NOTE: ON NOTICE: we might want to delete this file or repurpose it as part of the slotmanagement refactor
using Godot;
using System.Collections.Generic;

namespace Trivale.UI.Components;

// Represents the state of a single slot in the grid
public struct SlotState
{
    public bool IsActive;
    public bool IsUnlocked;
    public string LoadedText;
    public Vector2I GridPosition;  // Position in the slot grid
}

// Manages the state of a grid of slots
public partial class SlotGridSystem : Control
{
    private Dictionary<int, SlotState> _slots = new();
    private int _rows = 2;
    private int _columns = 2;
    
    // Pass individual slot state components (Can't send the state itself in a signal)
    [Signal]
    public delegate void SlotStateChangedEventHandler(int slotIndex, bool isActive, bool isUnlocked, string loadedText);
    
    public override void _Ready()
    {
        InitializeSlots();
    }
    
    private void InitializeSlots()
    {
        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _columns; col++)
            {
                int index = row * _columns + col;
                _slots[index] = new SlotState
                {
                    IsActive = false,
                    IsUnlocked = index == 0, // Only first slot starts unlocked
                    LoadedText = "",
                    GridPosition = new Vector2I(col, row)
                };
            }
        }
        
        EmitSlotUpdates();
    }
    
    public void SetSlotState(int slotIndex, bool isActive, string loadedText = "")
    {
        if (!_slots.ContainsKey(slotIndex)) return;
        
        var state = _slots[slotIndex];
        state.IsActive = isActive;
        state.LoadedText = loadedText;
        _slots[slotIndex] = state;
        
        EmitSignal(SignalName.SlotStateChanged, slotIndex, state.IsActive, state.IsUnlocked, state.LoadedText);
    }
    
    public void UnlockSlot(int slotIndex)
    {
        if (!_slots.ContainsKey(slotIndex)) return;
        
        var state = _slots[slotIndex];
        state.IsUnlocked = true;
        _slots[slotIndex] = state;
        
        EmitSignal(SignalName.SlotStateChanged, slotIndex, state.IsActive, state.IsUnlocked, state.LoadedText);
    }
    
    private void EmitSlotUpdates()
    {
        foreach (var kvp in _slots)
        {
            var state = kvp.Value;
            EmitSignal(SignalName.SlotStateChanged, kvp.Key, state.IsActive, state.IsUnlocked, state.LoadedText);
        }
    }
    
    // Public accessors
    public bool IsSlotActive(int slotIndex) => 
        _slots.ContainsKey(slotIndex) && _slots[slotIndex].IsActive;
    
    public bool IsSlotUnlocked(int slotIndex) => 
        _slots.ContainsKey(slotIndex) && _slots[slotIndex].IsUnlocked;
    
    public SlotState? GetSlotState(int slotIndex) => 
        _slots.ContainsKey(slotIndex) ? _slots[slotIndex] : null;
    
    public IEnumerable<KeyValuePair<int, SlotState>> GetAllSlots() => _slots;
}