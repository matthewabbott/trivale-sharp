// src/UI/Components/SlotGridDisplay.cs
// Temporary ASCII-based visualization of the slot system
using Godot;
using System.Text;
using System.Linq;
using System.Collections.Generic;

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
        var slots = _slotSystem.GetAllSlots().ToList();  // Now uses our new ordering

        // Track tree rendering state
        bool firstActive = true;
        
        foreach (var (slotId, slot) in slots)
        {
            string slotSymbol = GetSlotSymbol(slot);
            string indent = "";
            string branch = "└";

            if (slot.IsActive && firstActive)
            {
                // Root of our tree
                firstActive = false;
            }
            else if (slot.IsUnlocked)
            {
                // Child branch
                indent = "    ";
                branch = GetIndex(slots, slotId) == slots.Count - 1 ? "└" : "├";
            }

            display.AppendLine($"{indent}{branch}── {slotSymbol} [{slot.LoadedText.PadRight(10)}]");
        }
        
        _displayLabel.Text = display.ToString().TrimEnd();
    }

    private string GetSlotSymbol(SlotState slot)
    {
        if (!slot.IsUnlocked) return "⚿";  // Locked
        return slot.IsActive ? "■" : "□";   // Active/Inactive
    }

    private int GetIndex(List<KeyValuePair<string, SlotState>> slotList, string slotId)
    {
        return slotList.FindIndex(kvp => kvp.Key == slotId);
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