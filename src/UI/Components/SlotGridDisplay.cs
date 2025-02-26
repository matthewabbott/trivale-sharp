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
        
        // Clear existing content except the text label
        foreach (var child in GetChildren())
        {
            if (child != _displayLabel) // Keep the display label
            {
                child.QueueFree();
            }
        }
        
        // Get all slots
        var slots = _slotSystem.GetAllSlots().ToList();
        
        // Show ASCII representation in the label
        var displayText = new StringBuilder();
        if (slots.Any(s => s.Value.ParentSlotId != null))
        {
            DisplayHierarchicalTree(displayText, slots);
        }
        else
        {
            DisplayTraditionalTree(displayText, slots);
        }
        
        _displayLabel.Text = displayText.ToString().TrimEnd();
        
        // Create a container for the interactive buttons that will be displayed below the text
        var buttonContainer = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.Fill,
            SizeFlagsVertical = SizeFlags.Fill
        };
        AddChild(buttonContainer);
        
        // Add a small spacer to separate the text display from the buttons
        var spacer = new Control { CustomMinimumSize = new Vector2(0, 10) };
        buttonContainer.AddChild(spacer);
        
        // Now add clickable slot visualizations for unlocked slots
        foreach (var (slotId, state) in slots)
        {
            if (state.IsUnlocked)
            {
                // Create an indented container for this slot entry
                var slotEntry = new HBoxContainer
                {
                    SizeFlagsHorizontal = SizeFlags.Fill
                };
                
                // Add indentation based on hierarchy
                var indentation = new Control
                {
                    CustomMinimumSize = new Vector2(20, 0) // Adjust this value for desired indentation
                };
                slotEntry.AddChild(indentation);
                
                // Add the button in an indented position
                var buttonBox = new VBoxContainer
                {
                    SizeFlagsHorizontal = SizeFlags.Fill
                };
                
                // Add a label that shows the hierarchy
                var hierarchyLabel = new Label
                {
                    Text = GetHierarchyPrefix(slotId, slots),
                    SizeFlagsHorizontal = SizeFlags.Fill
                };
                buttonBox.AddChild(hierarchyLabel);
                
                // Create the actual button
                var slotButton = CreateSlotButton(slotId, state);
                buttonBox.AddChild(slotButton);
                
                slotEntry.AddChild(buttonBox);
                buttonContainer.AddChild(slotEntry);
                
                // Add a small spacer between entries
                var entrySpacer = new Control { CustomMinimumSize = new Vector2(0, 5) };
                buttonContainer.AddChild(entrySpacer);
            }
        }
    }

    // Helper to determine the hierarchy prefix for display
    private string GetHierarchyPrefix(string slotId, List<KeyValuePair<string, SlotState>> slots)
    {
        var state = slots.FirstOrDefault(s => s.Key == slotId).Value;
        
        if (state.ParentSlotId != null)
        {
            // This is a child slot
            return "│   ";
        }
        else if (slots.Any(s => s.Value.ParentSlotId == slotId))
        {
            // This is a parent slot with children
            return "├── ";
        }
        else
        {
            // This is a standalone slot
            return "└── ";
        }
    }

    private Button CreateSlotButton(string slotId, SlotState state)
    {
        var button = new Button
        {
            Text = GetButtonText(state),
            CustomMinimumSize = new Vector2(180, 30),
            SizeFlagsHorizontal = SizeFlags.Fill
        };
        button.AddThemeConstantOverride("text_alignment", (int)HorizontalAlignment.Center);
        
        // Style the button based on slot state
        var style = new StyleBoxFlat();
        
        if (state.IsActive)
        {
            style.BgColor = new Color(0, 0.3f, 0, 0.7f); // Green for active
            style.BorderColor = new Color(0, 0.5f, 0, 1.0f);
            style.BorderWidthBottom = 1;
            style.BorderWidthLeft = 1;
            style.BorderWidthRight = 1;
            style.BorderWidthTop = 1;
        }
        else if (state.IsUnlocked)
        {
            style.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.7f); // Dark for unlocked
            style.BorderColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
            style.BorderWidthBottom = 1;
            style.BorderWidthLeft = 1;
            style.BorderWidthRight = 1;
            style.BorderWidthTop = 1;
        }
        
        button.AddThemeStyleboxOverride("normal", style);
        
        // Connect button press to slot selection in the SlotGridSystem
        button.Pressed += () => OnSlotButtonPressed(slotId);
        
        return button;
    }

    // Helper to get appropriate button text
    private string GetButtonText(SlotState state)
    {
        if (state.IsActive)
        {
            return $"[■] {state.LoadedText}";
        }
        else if (state.IsUnlocked)
        {
            return "[□] Empty Slot";
        }
        else
        {
            return "[⚿] Locked";
        }
        
    }    

    // Add a handler for button presses
    private void OnSlotButtonPressed(string slotId)
    {
        // Forward to the SlotGridSystem to handle slot selection
        _slotSystem.SelectSlot(slotId);
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
        
        // Clear references
        _slotSystem = null;
    }
}