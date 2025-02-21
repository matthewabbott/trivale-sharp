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
        var slots = _slotSystem.GetAllSlots().ToList();
        
        // Find the first active slot to use as the root of our tree
        var firstActiveSlot = slots.FirstOrDefault(s => s.Value.IsActive);
        bool hasActiveSlot = firstActiveSlot.Key != null;
        
        // First pass: find active root slot
        for (int i = 0; i < slots.Count; i++)
        {
            var (slotId, slot) = slots[i];
            
            // If this is the active slot, display it as the root
            if (hasActiveSlot && slotId == firstActiveSlot.Key)
            {
                string slotSymbol = GetSlotSymbol(slot);
                display.AppendLine($"└── {slotSymbol} [{slot.LoadedText.PadRight(10)}]");
                
                // Second pass: display the other slots as children
                DisplayChildSlots(display, slots, i);
                break; // We found our root, exit the loop
            }
            // If no active slot, show the first unlocked slot as root
            else if (!hasActiveSlot && i == 0 && slot.IsUnlocked)
            {
                string slotSymbol = GetSlotSymbol(slot);
                display.AppendLine($"└── {slotSymbol} [{slot.LoadedText.PadRight(10)}]");
                
                // Display the other slots as children
                DisplayChildSlots(display, slots, i);
                break;
            }
        }
        
        // If we somehow didn't display anything (all slots locked?), show a message
        if (display.Length == 0)
        {
            display.AppendLine("└── □ [NO SLOTS AVAILABLE]");
        }
        
        _displayLabel.Text = display.ToString().TrimEnd();
    }

    private void DisplayChildSlots(StringBuilder display, List<KeyValuePair<string, SlotState>> slots, int rootIndex)
    {
        // Track different sections
        bool hasAddedUnlockedSlots = false;
        bool hasAddedLockedSlots = false;
        
        // First, display unlocked slots
        for (int j = 0; j < slots.Count; j++)
        {
            // Skip the root slot
            if (j == rootIndex) continue;
            
            var (childSlotId, childSlot) = slots[j];
            
            // Only display unlocked slots
            if (childSlot.IsUnlocked)
            {
                string slotSymbol = GetSlotSymbol(childSlot);
                
                // Count how many more unlocked slots follow this one (for branch character)
                int remainingUnlocked = CountRemainingUnlocked(slots, j);
                bool isLast = remainingUnlocked == 0;
                
                // If we'll show locked slots and this is the last unlocked slot, it's not really last
                if (isLast && HasLockedSlotsToShow(slots))
                    isLast = false;
                
                string branchChar = isLast ? "└" : "├";
                
                display.AppendLine($"    {branchChar}── {slotSymbol} [{childSlot.LoadedText.PadRight(10)}]");
                hasAddedUnlockedSlots = true;
            }
        }
        
        // Second, display locked slots if we have any
        for (int j = 0; j < slots.Count; j++)
        {
            var (childSlotId, childSlot) = slots[j];
            
            // Only display locked slots
            if (!childSlot.IsUnlocked)
            {
                string slotSymbol = GetSlotSymbol(childSlot);
                
                // Count how many more locked slots follow this one
                int remainingLocked = CountRemainingLocked(slots, j);
                bool isLast = remainingLocked == 0;
                
                string branchChar = isLast ? "└" : "├";
                
                display.AppendLine($"    {branchChar}── {slotSymbol} [LOCKED    ]");
                hasAddedLockedSlots = true;
            }
        }
    }
    
    private int CountRemainingUnlocked(List<KeyValuePair<string, SlotState>> slots, int currentIndex)
    {
        int count = 0;
        for (int i = currentIndex + 1; i < slots.Count; i++)
        {
            if (slots[i].Value.IsUnlocked)
                count++;
        }
        return count;
    }
    
    private int CountRemainingLocked(List<KeyValuePair<string, SlotState>> slots, int currentIndex)
    {
        int count = 0;
        for (int i = currentIndex + 1; i < slots.Count; i++)
        {
            if (!slots[i].Value.IsUnlocked)
                count++;
        }
        return count;
    }
    
    private bool HasLockedSlotsToShow(List<KeyValuePair<string, SlotState>> slots)
    {
        return slots.Any(s => !s.Value.IsUnlocked);
    }

    private bool IsLastVisibleSlot(List<KeyValuePair<string, SlotState>> slots, int currentIndex)
    {
        // Check if this is the last unlocked slot in the list
        for (int i = currentIndex + 1; i < slots.Count; i++)
        {
            if (slots[i].Value.IsUnlocked)
            {
                return false; // Found another unlocked slot after this one
            }
        }
        return true; // This is the last unlocked slot
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