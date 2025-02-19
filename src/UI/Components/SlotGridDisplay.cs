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
        
        // Set monospace font for ASCII art
        var font = new SystemFont();
        font.FontNames = new string[] { "JetBrainsMono-Regular", "Consolas", "Courier New" };
        _displayLabel.AddThemeFontOverride("font", font);
        
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
        
        var display = new StringBuilder();
        
        // Sort slots by their grid position for consistent display
        var slots = _slotSystem.GetAllSlots()
            .OrderBy(kvp => kvp.Value.GridPosition.Y)
            .ThenBy(kvp => kvp.Value.GridPosition.X)
            .ToList();

        // Find first active slot for tree structure
        var firstActiveSlot = slots.FirstOrDefault(s => s.Value.IsActive);
        bool hasActiveSlot = firstActiveSlot.Key != null;
        
        foreach (var (slotId, slot) in slots)
        {
            string slotSymbol;
            if (!slot.IsUnlocked)
            {
                slotSymbol = "⚿"; // Locked slot
            }
            else if (slot.IsActive)
            {
                slotSymbol = "■"; // Active slot
            }
            else
            {
                slotSymbol = "□"; // Empty slot
            }
            
            // Handle tree structure for active processes
            if (hasActiveSlot)
            {
                if (slotId == firstActiveSlot.Key)
                {
                    // Root of the tree
                    display.AppendLine($"└── {slotSymbol} [{slot.LoadedText.PadRight(10)}]");
                }
                else
                {
                    // Branches (show only if unlocked)
                    if (slot.IsUnlocked)
                    {
                        bool isLast = slots.IndexOf((slotId, slot)) == slots.Count - 1;
                        string branch = isLast ? "└" : "├";
                        display.AppendLine($"    {branch}── {slotSymbol} [{slot.LoadedText.PadRight(10)}]");
                    }
                }
            }
            else
            {
                // No active process, flat display
                display.AppendLine($"└── {slotSymbol} [{slot.LoadedText.PadRight(10)}]");
            }
        }
        
        _displayLabel.Text = display.ToString().TrimEnd();
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