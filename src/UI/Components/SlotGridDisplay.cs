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
    private RichTextLabel _displayLabel;
    
    public void Initialize(SlotGridSystem slotSystem)
    {
        _slotSystem = slotSystem;
        _slotSystem.SlotStateChanged += OnSlotStateChanged;
    }
    
    public override void _Ready()
    {
        // Use RichTextLabel instead of Label to support BBCode formatting
        var richTextLabel = new RichTextLabel
        {
            Text = "INITIALIZING...",
            BbcodeEnabled = true,
            FitContent = true,
            AutowrapMode = TextServer.AutowrapMode.Off,
            CustomMinimumSize = new Vector2(500, 0)
        };
        
        // Set monospace font for ASCII art
        var font = new SystemFont();
        font.FontNames = new string[] { "JetBrainsMono-Regular", "Consolas", "Courier New" };
        richTextLabel.AddThemeFontOverride("normal_font", font);
        
        AddChild(richTextLabel);
        _displayLabel = richTextLabel;
    }
    
    private void OnSlotStateChanged(string slotId, bool isActive, bool isUnlocked, string loadedText)
    {
        UpdateDisplay();
    }
    
    // Resource usage thresholds
    private const float LOW_USAGE = 0.3f;     // 0-30% - Green
    private const float MEDIUM_USAGE = 0.7f;  // 31-70% - Yellow
    // 71-100% - Red
    
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
                
                // Add resource info to the active root slot
                string resourceInfo = "";
                if (slot.IsActive)
                {
                    string memColor = GetResourceColor(slot.MemoryUsage);
                    string cpuColor = GetResourceColor(slot.CpuUsage);
                    resourceInfo = $" MEM:[color={memColor}]{slot.MemoryUsage:F1}[/color] CPU:[color={cpuColor}]{slot.CpuUsage:F1}[/color]";
                }
                
                display.AppendLine($"└── {slotSymbol} [{slot.LoadedText.PadRight(10)}]{resourceInfo}");
                
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
                
                // Add resource info to active slots
                string resourceInfo = "";
                if (childSlot.IsActive)
                {
                    string memColor = GetResourceColor(childSlot.MemoryUsage);
                    string cpuColor = GetResourceColor(childSlot.CpuUsage);
                    resourceInfo = $" MEM:[color={memColor}]{childSlot.MemoryUsage:F1}[/color] CPU:[color={cpuColor}]{childSlot.CpuUsage:F1}[/color]";
                }
                
                display.AppendLine($"    {branchChar}── {slotSymbol} [{childSlot.LoadedText.PadRight(10)}]{resourceInfo}");
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
    
    private string GetResourceColor(float usage)
    {
        if (usage <= LOW_USAGE)
            return "#00FF00"; // Green for low usage
        else if (usage <= MEDIUM_USAGE)
            return "#FFFF00"; // Yellow for medium usage
        else
            return "#FF0000"; // Red for high usage
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