// src/UI/Components/SlotGridSystem.cs
using Godot;
using System.Collections.Generic;
using System.Linq;
using Trivale.Memory.SlotManagement;

namespace Trivale.UI.Components;

/// <summary>
/// UI representation of the slot system that manages slot grid visualization
/// and user interaction. Observes SlotManager events to update its display
/// and maintains its own simplified state for UI purposes.
/// 
/// Decouples the slot management system from its visual representation.
/// </summary>
public struct SlotState
{
    public bool IsActive;
    public bool IsUnlocked;
    public string LoadedText;
    public Vector2I GridPosition;
    // Added for future hierarchy support:
    public string ParentSlotId;  // null means root/no parent
    // Resource tracking:
    public float MemoryUsage;
    public float CpuUsage;
}

public partial class SlotGridSystem : Control
{
    private Dictionary<string, SlotState> _slots = new();
    private ISlotManager _slotManager;
    
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
            GridPosition = gridPosition,
            ParentSlotId = null,  // We'll set this when implementing full hierarchy
            MemoryUsage = slot.MemoryUsage,
            CpuUsage = slot.CpuUsage
        };
        
        EmitSignal(SignalName.SlotStateChanged, slot.Id, isActive, isUnlocked, loadedText);
    }
    
    private Vector2I GetGridPositionFromId(string slotId)
    {
        // Parse position from id (format: slot_x_y)
        var parts = slotId.Split('_');
        if (parts.Length >= 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
        {
            return new Vector2I(x, y);
        }
        return Vector2I.Zero;
    }

    // Get slots in display order
    private IEnumerable<KeyValuePair<string, SlotState>> GetDisplayOrder()
    {
        // First, any active slots
        var activeSlots = _slots.Where(kvp => kvp.Value.IsActive);
        
        // Then, unlocked but inactive slots, ordered by grid position
        var inactiveSlots = _slots.Where(kvp => !kvp.Value.IsActive && kvp.Value.IsUnlocked)
            .OrderBy(kvp => kvp.Value.GridPosition.Y)
            .ThenBy(kvp => kvp.Value.GridPosition.X);
            
        // Finally, any locked slots
        var lockedSlots = _slots.Where(kvp => !kvp.Value.IsUnlocked)
            .OrderBy(kvp => kvp.Value.GridPosition.Y)
            .ThenBy(kvp => kvp.Value.GridPosition.X);

        return activeSlots.Concat(inactiveSlots).Concat(lockedSlots);
    }

    // Set parent-child relationship between slots
    public void SetSlotParent(string childSlotId, string parentSlotId)
    {
        if (!_slots.ContainsKey(childSlotId) || !_slots.ContainsKey(parentSlotId))
            return;
            
        var childSlot = _slots[childSlotId];
        childSlot.ParentSlotId = parentSlotId;
        _slots[childSlotId] = childSlot;
        
        // Emit signal to update the display
        EmitSignal(SignalName.SlotStateChanged, childSlotId, childSlot.IsActive, childSlot.IsUnlocked, childSlot.LoadedText);
    }
    
    // Clear parent relationship
    public void ClearSlotParent(string slotId)
    {
        if (!_slots.ContainsKey(slotId))
            return;
            
        var slot = _slots[slotId];
        slot.ParentSlotId = null;
        _slots[slotId] = slot;
        
        // Emit signal to update the display
        EmitSignal(SignalName.SlotStateChanged, slotId, slot.IsActive, slot.IsUnlocked, slot.LoadedText);
    }
    
    // Get all child slots for a given parent
    public IEnumerable<KeyValuePair<string, SlotState>> GetChildSlots(string parentSlotId)
    {
        return _slots.Where(kvp => kvp.Value.ParentSlotId == parentSlotId);
    }
    
    // Check if a slot has any children
    public bool HasChildren(string slotId)
    {
        return _slots.Any(kvp => kvp.Value.ParentSlotId == slotId);
    }
    
    // Public accessors
    public IEnumerable<KeyValuePair<string, SlotState>> GetAllSlots() => GetDisplayOrder();
    
    public bool IsSlotActive(string slotId) => 
        _slots.ContainsKey(slotId) && _slots[slotId].IsActive;
    
    public bool IsSlotUnlocked(string slotId) => 
        _slots.ContainsKey(slotId) && _slots[slotId].IsUnlocked;
    
    public SlotState? GetSlotState(string slotId) => 
        _slots.ContainsKey(slotId) ? _slots[slotId] : null;
}