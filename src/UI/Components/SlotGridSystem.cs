// src/UI/Components/SlotGridSystem.cs
using Godot;
using System.Collections.Generic;
using System.Linq;
using Trivale.Memory.SlotManagement;
using Trivale.Memory.ProcessManagement;
using Trivale.OS.Events;

namespace Trivale.UI.Components;

/// <summary>
/// UI representation of the slot system that manages slot grid visualization
/// and user interaction. Observes SlotManager events via the SystemEventBus
/// to update its display and maintains its own simplified state for UI purposes.
/// 
/// Decouples the slot management system from its visual representation.
/// </summary>
public struct SlotState
{
    public bool IsActive;
    public bool IsUnlocked;
    public string LoadedText;
    public Vector2I GridPosition;
    public string ParentSlotId;  // null means root/no parent
    public float MemoryUsage;
    public float CpuUsage;
}

public partial class SlotGridSystem : Control
{
    private Dictionary<string, SlotState> _slots = new();
    private ISlotManager _slotManager;
    private ProcessSlotRegistry _registry;
    private SystemEventBus _eventBus;
    
    [Signal]
    public delegate void SlotStateChangedEventHandler(string slotId, bool isActive, bool isUnlocked, string loadedText);
    
    [Signal]
    public delegate void SlotSelectedEventHandler(string slotId, string processId);
    
    public void Initialize(ISlotManager slotManager, ProcessSlotRegistry registry)
    {
        _slotManager = slotManager;
        _registry = registry;
        _eventBus = SystemEventBus.Instance;
        
        // Subscribe to registry events
        _registry.ProcessSlotMappingChanged += OnProcessSlotMappingChanged;
        _registry.ActiveProcessChanged += OnActiveProcessChanged;
        
        // Use event bus to monitor slot changes
        _eventBus.SlotStatusChanged += OnSlotStatusChanged;
        _eventBus.SlotUnlocked += OnSlotUnlocked;
        _eventBus.SlotLocked += OnSlotLocked;
        _eventBus.SlotParentChanged += OnSlotParentChanged;
        _eventBus.SlotResourcesChanged += OnSlotResourcesChanged;
        
        // Also hook up legacy events for backward compatibility
        _slotManager.SlotStatusChanged += OnLegacySlotStatusChanged;
        _slotManager.SlotUnlocked += OnLegacySlotUnlocked;
        
        // Initialize UI state from slot manager
        foreach (var slot in _slotManager.GetAllSlots())
        {
            UpdateSlotState(slot);
        }
    }
    
    private void OnActiveProcessChanged(string newActiveProcessId)
    {
        foreach (var slotId in _slots.Keys.ToList())
        {
            var slotState = _slots[slotId];
            var processId = _registry.GetProcessForSlot(slotId);
            slotState.IsActive = processId == newActiveProcessId;
            _slots[slotId] = slotState;
            EmitSignal(SignalName.SlotStateChanged, slotId, slotState.IsActive, 
                slotState.IsUnlocked, slotState.LoadedText);
        }
    }
    
    private void OnProcessSlotMappingChanged(string processId, string slotId)
    {
        if (!string.IsNullOrEmpty(slotId) && _slots.TryGetValue(slotId, out var slotState))
        {
            var process = _registry.GetProcessForSlot(slotId);
            slotState.LoadedText = process?.Type ?? "EMPTY";
            slotState.IsActive = process?.Id == _registry.ActiveProcessId;
            _slots[slotId] = slotState;
            EmitSignal(SignalName.SlotStateChanged, slotId, slotState.IsActive, 
                slotState.IsUnlocked, slotState.LoadedText);
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
    
private void OnSlotLocked(string slotId)
    {
        var slot = _slotManager.GetAllSlots().FirstOrDefault(s => s.Id == slotId);
        if (slot != null)
        {
            UpdateSlotState(slot);
        }
    }
    
    private void OnSlotParentChanged(string childSlotId, string parentSlotId)
    {
        if (_slots.TryGetValue(childSlotId, out var childState))
        {
            childState.ParentSlotId = parentSlotId;
            _slots[childSlotId] = childState;
            
            EmitSignal(SignalName.SlotStateChanged, childSlotId, childState.IsActive, 
                childState.IsUnlocked, childState.LoadedText);
        }
    }
    
    private void OnSlotResourcesChanged(string slotId, float memory, float cpu)
    {
        if (_slots.TryGetValue(slotId, out var state))
        {
            state.MemoryUsage = memory;
            state.CpuUsage = cpu;
            _slots[slotId] = state;
            
            // No need to emit signal for just resources changing
            // unless you want a specific resource-changed signal
        }
    }
    
    // Legacy event handlers (will be removed eventually)
    private void OnLegacySlotStatusChanged(string slotId, SlotStatus status)
    {
        // Forward to event bus handler - eventually we'll remove this
        OnSlotStatusChanged(slotId, status);
    }
    
    private void OnLegacySlotUnlocked(string slotId)
    {
        // Forward to event bus handler - eventually we'll remove this
        OnSlotUnlocked(slotId);
    }

    private void UpdateSlotState(ISlot slot)
    {
        var gridPosition = GetGridPositionFromId(slot.Id);
        var processId = slot.CurrentProcess?.Id;
        var isActive = processId != null && processId == _registry.ActiveProcessId;
        var isUnlocked = slot.Status != SlotStatus.Locked;
        var loadedText = slot.CurrentProcess?.Type ?? "EMPTY";
        
        // Look for existing parent relationship
        string parentSlotId = null;
        if (_slots.TryGetValue(slot.Id, out var existingState))
        {
            parentSlotId = existingState.ParentSlotId;
        }
        
        _slots[slot.Id] = new SlotState
        {
            IsActive = isActive,
            IsUnlocked = isUnlocked,
            LoadedText = loadedText,
            GridPosition = gridPosition,
            ParentSlotId = parentSlotId,
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

    // Add method to handle slot selection
    public void SelectSlot(string slotId)
    {
        if (!_slots.TryGetValue(slotId, out var state) || !state.IsUnlocked)
            return;
            
        string processId = _registry.GetProcessForSlot(slotId);
        
        // Emit signal even if processId is null (empty slot)
        GD.Print($"SlotGridSystem selected slot {slotId} with process {processId ?? "none"}");
        EmitSignal(SignalName.SlotSelected, slotId, processId);
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
        
        // Publish event to event bus
        _eventBus.PublishSlotParentChanged(childSlotId, parentSlotId);
        
        // Also emit signal for direct subscribers
        EmitSignal(SignalName.SlotStateChanged, childSlotId, childSlot.IsActive, childSlot.IsUnlocked, childSlot.LoadedText);
    }
    
    // Clear parent relationship
    public void ClearSlotParent(string slotId)
    {
        if (!_slots.ContainsKey(slotId))
            return;
            
        var slot = _slots[slotId];
        
        // Only publish if there was actually a parent
        if (slot.ParentSlotId != null)
        {
            slot.ParentSlotId = null;
            _slots[slotId] = slot;
            
            // Publish event
            _eventBus.PublishSlotParentChanged(slotId, null);
            
            // Emit signal
            EmitSignal(SignalName.SlotStateChanged, slotId, slot.IsActive, slot.IsUnlocked, slot.LoadedText);
        }
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
        
    public override void _ExitTree()
    {
        // Unsubscribe from events
        if (_eventBus != null)
        {
            _eventBus.SlotStatusChanged -= OnSlotStatusChanged;
            _eventBus.SlotUnlocked -= OnSlotUnlocked;
            _eventBus.SlotLocked -= OnSlotLocked;
            _eventBus.SlotParentChanged -= OnSlotParentChanged;
            _eventBus.SlotResourcesChanged -= OnSlotResourcesChanged;
        }
        
        // Unsubscribe from registry events
        if (_registry != null)
        {
            _registry.ProcessSlotMappingChanged -= OnProcessSlotMappingChanged;
            _registry.ActiveProcessChanged -= OnActiveProcessChanged;
        }
        
        // Unsubscribe from legacy events
        if (_slotManager != null)
        {
            _slotManager.SlotStatusChanged -= OnLegacySlotStatusChanged;
            _slotManager.SlotUnlocked -= OnLegacySlotUnlocked;
        }
        
        // Clear references
        _slotManager = null;
        _registry = null;
        _eventBus = null;
        _slots.Clear();
        
        base._ExitTree();
    }
}