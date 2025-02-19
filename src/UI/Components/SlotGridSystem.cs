// src/UI/Components/SlotGridSystem.cs
using Godot;
using System.Collections.Generic;
using Trivale.Memory.SlotManagement;

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
    private Dictionary<string, SlotState> _slots = new();
    private ISlotManager _slotManager;
    private int _rows = 2;
    private int _columns = 2;
    
    // Pass individual slot state components (Can't send the state itself in a signal)
    [Signal]
    public delegate void SlotStateChangedEventHandler(string slotId, bool isActive, bool isUnlocked, string loadedText);
    
    public void Initialize(ISlotManager slotManager)
    {
        _slotManager = slotManager;
        _slotManager.SlotStatusChanged += OnSlotStatusChanged;
        _slotManager.SlotUnlocked += OnSlotUnlocked;
        
        // Initialize UI state from slot manager
        foreach (var slot in _slotManager.GetAllSlots())
        {
            UpdateSlotState(slot);
        }
    }
    
    private void OnSlotStatusChanged(string slotId, SlotStatus status)
    {
        var slot = _slotManager.GetAllSlots().FirstOrDefault(s => s.Id == slotId);
        if (slot != null)
        {
            UpdateSlotState(slot);
        }
    }
    
    private void OnSlotUnlocked(string slotId)
    {
        var slot = _slotManager.GetAllSlots().FirstOrDefault(s => s.Id == slotId);
        if (slot != null)
        {
            UpdateSlotState(slot);
        }
    }
    
    private void UpdateSlotState(ISlot slot)
    {
        var gridPosition = GetGridPositionFromId(slot.Id);
        var isActive = slot.Status == SlotStatus.Active;
        var isUnlocked = slot.Status != SlotStatus.Locked;
        var loadedText = slot.CurrentProcess?.Type ?? "";
        
        _slots[slot.Id] = new SlotState
        {
            IsActive = isActive,
            IsUnlocked = isUnlocked,
            LoadedText = loadedText,
            GridPosition = gridPosition
        };
        
        EmitSignal(SignalName.SlotStateChanged, slot.Id, isActive, isUnlocked, loadedText);
    }
    
    private Vector2I GetGridPositionFromId(string slotId)
    {
        // Assuming slot IDs are in format "slot_0", "slot_1", etc.
        if (int.TryParse(slotId.Split('_')[1], out int index))
        {
            return new Vector2I(index % _columns, index / _columns);
        }
        return Vector2I.Zero;
    }
    
    // Public accessors for UI components
    public bool IsSlotActive(string slotId) => 
        _slots.ContainsKey(slotId) && _slots[slotId].IsActive;
    
    public bool IsSlotUnlocked(string slotId) => 
        _slots.ContainsKey(slotId) && _slots[slotId].IsUnlocked;
    
    public SlotState? GetSlotState(string slotId) => 
        _slots.ContainsKey(slotId) ? _slots[slotId] : null;
    
    public IEnumerable<KeyValuePair<string, SlotState>> GetAllSlots() => _slots;
}
