// src/UI/Components/SlotGridDisplay.cs
// Temporary ASCII-based visualization of the slot system
using Godot;
using System.Text;

namespace Trivale.UI.Components;

public partial class SlotGridDisplay : Control
{
    private SlotGridSystem _slotSystem;
    private Label _displayLabel;
    
    public override void _Ready()
    {
        _displayLabel = new Label
        {
            Text = "INITIALIZING...",
            HorizontalAlignment = HorizontalAlignment.Left
        };
        AddChild(_displayLabel);
        
        // Find and connect to slot system
        // Note: The SlotGridSystem node needs a unique name set in the scene
        _slotSystem = GetNode<SlotGridSystem>("%SlotGridSystem");
        if (_slotSystem != null)
        {
            _slotSystem.SlotStateChanged += OnSlotStateChanged;
        }
        else
        {
            GD.PrintErr("SlotGridDisplay: Could not find SlotGridSystem node");
        }
    }
    
    private void OnSlotStateChanged(int slotIndex, bool isActive, bool isUnlocked, string loadedText)
    {
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (_slotSystem == null) return;
        
        // Build display string
        var display = new StringBuilder();
        
        foreach (var (index, slot) in _slotSystem.GetAllSlots())
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