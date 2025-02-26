// src/UI/Components/SlotGridDisplay.cs
// Temporary ASCII-based visualization of the slot system
// TODO: collision detection, thumbnails, event handlers, ui feedback mechanisms,
    // ability to manipulate individual slots. There's a lot to do here.
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
            SizeFlagsHorizontal = SizeFlags.Fill,
            SizeFlagsVertical = SizeFlags.Fill,
            AutowrapMode = TextServer.AutowrapMode.Word,
            CustomMinimumSize = new Vector2(250, 0)
        };
        
        // Set monospace font for ASCII art
        var font = new SystemFont();
        font.FontNames = new string[] { "JetBrainsMono-Regular", "Consolas", "Courier New" };
        richTextLabel.AddThemeFontOverride("normal_font", font);
        
        AddChild(richTextLabel);
        _displayLabel = richTextLabel;
    }

    private Control CreateSlotVisual(string slotId, SlotState state)
    {
        var container = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(200, 0),
            SizeFlagsHorizontal = SizeFlags.Fill
        };
        
        // Create a button for slot selection
        var button = new Button
        {
            Text = state.IsActive 
                ? $"{GetSlotSymbol(state)} [{state.LoadedText}]" 
                : $"{GetSlotSymbol(state)} [EMPTY]",
            SizeFlagsHorizontal = SizeFlags.Fill
        };
        
        // Style the button based on state
        var style = new StyleBoxFlat();
        
        if (state.IsActive)
        {
            style.BgColor = new Color(0, 0.3f, 0, 0.7f); // Green for active
        }
        else if (state.IsUnlocked)
        {
            style.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.7f); // Dark for unlocked
        }
        else
        {
            style.BgColor = new Color(0.2f, 0, 0, 0.5f); // Red for locked
            button.Disabled = true;
        }
        
        button.AddThemeStyleboxOverride("normal", style);
        
        // Connect the button's pressed signal
        button.Pressed += () => OnSlotButtonPressed(slotId);
        
        container.AddChild(button);
        
        return container;
    }

    private void OnSlotButtonPressed(string slotId)
    {
        if (_slotSystem != null)
        {
            _slotSystem.SelectSlot(slotId);
        }
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
        
        // Check if we have any slots with parent-child relationships
        bool hasParentChildRelationships = slots.Any(s => s.Value.ParentSlotId != null);
        
        if (hasParentChildRelationships)
        {
            // Use hierarchy-based display
            DisplayHierarchicalTree(display, slots);
        }
        else
        {
            // Use traditional active-root display
            DisplayTraditionalTree(display, slots);
        }
        
        // If we somehow didn't display anything, show a message
        if (display.Length == 0)
        {
            display.AppendLine("└── □ [NO SLOTS AVAILABLE]");
        }
        
        _displayLabel.Text = display.ToString().TrimEnd();
    }
    
    private void DisplayHierarchicalTree(StringBuilder display, List<KeyValuePair<string, SlotState>> slots)
    {
        // Find all root slots (those without parents)
        var rootSlots = slots.Where(s => s.Value.ParentSlotId == null && s.Value.IsUnlocked).ToList();
        
        // Process each root slot
        for (int i = 0; i < rootSlots.Count; i++)
        {
            bool isLastRoot = (i == rootSlots.Count - 1);
            var (rootId, rootSlot) = rootSlots[i];
            
            // Display the root slot
            string rootSymbol = GetSlotSymbol(rootSlot);
            string rootPrefix = isLastRoot ? "└── " : "├── ";
            
            // Add resource info to root if active - simplified version
            string resourceInfo = "";
            if (rootSlot.IsActive)
            {
                // Just add an indicator dot with appropriate color
                string color = GetResourceColor(rootSlot.MemoryUsage);
                resourceInfo = $" [color={color}]●[/color]";
            }
            
            display.AppendLine($"{rootPrefix}{rootSymbol} [{rootSlot.LoadedText.PadRight(10)}]{resourceInfo}");
            
            // Display children of this root
            string childIndent = isLastRoot ? "    " : "│   ";
            DisplayChildrenRecursive(display, slots, rootId, childIndent);
        }
        
        // If we have any locked slots, display them last
        var lockedSlots = slots.Where(s => !s.Value.IsUnlocked).ToList();
        if (lockedSlots.Count > 0)
        {
            // Only add a separator if we displayed any roots above
            if (rootSlots.Count > 0)
            {
                display.AppendLine();
                display.AppendLine("Locked Slots:");
            }
            
            for (int i = 0; i < lockedSlots.Count; i++)
            {
                bool isLastLocked = (i == lockedSlots.Count - 1);
                var (lockedId, lockedSlot) = lockedSlots[i];
                
                string slotSymbol = GetSlotSymbol(lockedSlot);
                string prefix = isLastLocked ? "└── " : "├── ";
                
                display.AppendLine($"{prefix}{slotSymbol} [[color=#777777]LOCKED    [/color]]");
            }
        }
    }
    
    private void DisplayChildrenRecursive(StringBuilder display, List<KeyValuePair<string, SlotState>> slots, 
        string parentId, string indent)
    {
        // Find all direct children of this parent
        var children = slots.Where(s => s.Value.ParentSlotId == parentId && s.Value.IsUnlocked).ToList();
        
        // Process each child
        for (int i = 0; i < children.Count; i++)
        {
            bool isLastChild = (i == children.Count - 1);
            var (childId, childSlot) = children[i];
            
            // Display the child
            string childSymbol = GetSlotSymbol(childSlot);
            string childPrefix = isLastChild ? "└── " : "├── ";
            
            // Add resource info if active - simplified version
            string resourceInfo = "";
            if (childSlot.IsActive)
            {
                // Just add an indicator dot with appropriate color
                string color = GetResourceColor(childSlot.MemoryUsage);
                resourceInfo = $" [color={color}]●[/color]";
            }
            
            display.AppendLine($"{indent}{childPrefix}{childSymbol} [{childSlot.LoadedText.PadRight(10)}]{resourceInfo}");
            
            // Recursively display this child's children
            string nextIndent = indent + (isLastChild ? "    " : "│   ");
            DisplayChildrenRecursive(display, slots, childId, nextIndent);
        }
    }
    
    private void DisplayTraditionalTree(StringBuilder display, List<KeyValuePair<string, SlotState>> slots)
    {
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
                
                // Add resource info to the active root slot - simplified version
                string resourceInfo = "";
                if (slot.IsActive)
                {
                    // Just add an indicator dot with appropriate color
                    string color = GetResourceColor(slot.MemoryUsage);
                    resourceInfo = $" [color={color}]●[/color]";
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
                
                // Add resource info to active slots - simplified version
                string resourceInfo = "";
                if (childSlot.IsActive)
                {
                    // Just add an indicator dot with appropriate color
                    string color = GetResourceColor(childSlot.MemoryUsage);
                    resourceInfo = $" [color={color}]●[/color]";
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
                
                display.AppendLine($"    {branchChar}── {slotSymbol} [[color=#777777]LOCKED    [/color]]");
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