// src/UI/Components/MemoryGridView.cs
using Godot;
using System.Collections.Generic;
using System.Linq;
using Trivale.Memory;
using Trivale.OS;

namespace Trivale.UI.Components;

public partial class MemoryGridView : VBoxContainer
{
    private VBoxContainer _slotsContainer;
    private ProcessManager _processManager;
    private Dictionary<string, MemorySlotDisplay> _slotDisplays = new();
    
    [Signal]
    public delegate void MemorySlotSelectedEventHandler(string slotId);
    
    public bool ShowAdditionalSlots { get; set; } = false;
    
    public override void _Ready()
    {
        SetupLayout();
        
        // Set up periodic updates
        var timer = new Timer { WaitTime = 0.5, Autostart = true };
        timer.Timeout += UpdateDisplay;
        AddChild(timer);
    }
    
    public void Initialize(ProcessManager processManager)
    {
        _processManager = processManager;
        UpdateDisplay();
    }
    
    private void SetupLayout()
    {
        // Title label at top
        var label = new Label 
        { 
            Text = "SYSTEM MEMORY",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        AddChild(label);
        
        // Container for memory slots
        _slotsContainer = new VBoxContainer
        {
            SizeFlagsVertical = SizeFlags.Fill,
            Theme = UIThemeManager.Instance.CreateTheme()
        };
        AddChild(_slotsContainer);
        
        // Set this container to pass through mouse events
        MouseFilter = MouseFilterEnum.Pass;
    }
    
    public void UpdateDisplay()
    {
        if (_processManager == null || _slotsContainer == null) return;
        
        var slots = _processManager.GetAllSlots().ToList();
        
        // Remove displays for slots that no longer exist
        var toRemove = new List<string>();
        foreach (var kvp in _slotDisplays)
        {
            if (!slots.Any(s => s.Id == kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }
        
        foreach (var id in toRemove)
        {
            _slotDisplays[id].QueueFree();
            _slotDisplays.Remove(id);
        }
        
        // Create or update displays for current slots
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            
            // Skip additional slots if not showing them
            if (i > 0 && !ShowAdditionalSlots) continue;
            
            if (!_slotDisplays.TryGetValue(slot.Id, out var display))
            {
                // Create new display
                display = new MemorySlotDisplay();
                display.SlotSelected += (slotId) => 
                    EmitSignal(SignalName.MemorySlotSelected, slotId);
                
                // Set margin for indentation of additional slots
                if (i > 0)
                {
                    display.AddThemeConstantOverride("margin_left", 20);
                }
                
                _slotDisplays[slot.Id] = display;
                _slotsContainer.AddChild(display);
            }
            
            display.UpdateSlot(slot, i == 0); // Pass isSlot0 flag
        }
    }
}