// src/UI/Components/SlotGridDisplay.cs
// Temporary ASCII-based visualization of the slot system
using Godot;
using System.Text;
using System.Linq;

namespace Trivale.UI.Components;

public partial class SlotGridDisplay : Control
{
    private SlotGridSystem _slotSystem;
    private Label _displayLabel;
    
    public void Initialize(SlotGridSystem slotSystem)
    {
        _slotSystem = slotSystem;
        _slotSystem.SlotStateChanged += OnSlotStateChanged;
    }
    
    public override void _Ready()
    {
        _displayLabel = new Label
        {
            Text = "INITIALIZING...",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        AddChild(_displayLabel);
    }
    
    private void OnSlotStateChanged(string slotId, bool isActive, bool isUnlocked, string loadedText)
    {
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (_slotSystem == null)
        {
            _displayLabel.Text = "└── □ [ERROR: NO SLOT SYSTEM]";
            return;
        }
        
        // Build display string
        var display = new StringBuilder();
        
        // Sort slots by their grid position for consistent display
        var sortedSlots = _slotSystem.GetAllSlots()
            .OrderBy(kvp => kvp.Value.GridPosition.Y)
            .ThenBy(kvp => kvp.Value.GridPosition.X);

        foreach (var (slotId, slot) in sortedSlots)
        {
            if (!slot.IsUnlocked) continue;
            
            // Add indentation for sub-slots
            if (slot.GridPosition.Y > 0) 
            {
                display.Append("    ");
            }
            
            // Create the slot visualization
            display.Append("└── ");
            display.Append(slot.IsActive ? "■" : "□");
            display.Append(" [");
            display.Append(slot.LoadedText.PadRight(10));
            display.AppendLine("]");
        }
        
        _displayLabel.Text = display.ToString();
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        
        if (_slotSystem != null)
        {
            _slotSystem.SlotStateChanged -= OnSlotStateChanged;
        }
    }
}
