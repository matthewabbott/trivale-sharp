// src/UI/Components/SlotGridDisplay.cs
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
	
	public void Initialize(SlotGridSystem slotSystem)
	{
		_slotSystem = slotSystem;
		_slotSystem.SlotStateChanged += OnSlotStateChanged;
	}
	
	public override void _Ready()
	{
		// Set up a container for our slot display
		AddChild(new VBoxContainer
		{
			Name = "SlotContainer",
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill
		});
		
		UpdateDisplay();
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
			return;
		}
		
		// Get the container or create it if needed
		var container = GetNodeOrNull<VBoxContainer>("SlotContainer");
		if (container == null)
		{
			container = new VBoxContainer
			{
				Name = "SlotContainer",
				SizeFlagsHorizontal = SizeFlags.Fill,
				SizeFlagsVertical = SizeFlags.Fill
			};
			AddChild(container);
		}
		
		// Clear existing content
		foreach (var child in container.GetChildren())
		{
			child.QueueFree();
		}
		
		// Get all slots
		var slots = _slotSystem.GetAllSlots().ToList();
		
		// Title
		var titleLabel = new Label
		{
			Text = "MEM SLOTS",
			SizeFlagsHorizontal = SizeFlags.Fill
		};
		container.AddChild(titleLabel);
		
		// Add a separator
		container.AddChild(new HSeparator());
		
		// Find the "mainmenu" slot for special treatment
		var mainMenuSlot = slots.FirstOrDefault(s => 
			s.Key.Contains("slot_0_0") || (s.Value.IsActive && s.Value.LoadedText == "MainMenu"));
			
		bool hasMainMenu = mainMenuSlot.Key != null;
		
		// If we have a main menu slot, display it first with special treatment
		if (hasMainMenu)
		{
			// Create a custom container just for the main menu
			var mainMenuContainer = new VBoxContainer
			{
				SizeFlagsHorizontal = SizeFlags.Fill
			};
			container.AddChild(mainMenuContainer);
			
			// Add a label for the system root
			var systemRootLabel = new Label
			{
				Text = "SYSTEM ROOT:",
				SizeFlagsHorizontal = SizeFlags.Fill
			};
			mainMenuContainer.AddChild(systemRootLabel);
			
			// Create special main menu button
			var mainMenuButton = CreateSlotButton(mainMenuSlot.Key, mainMenuSlot.Value, "└── ", true);
			mainMenuContainer.AddChild(mainMenuButton);
			
			// Add spacer
			mainMenuContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
			
			// Add separator between main menu and other slots
			container.AddChild(new HSeparator());
		}
		
		// Other processes label
		var processesLabel = new Label
		{
			Text = "PROCESSES:",
			SizeFlagsHorizontal = SizeFlags.Fill
		};
		container.AddChild(processesLabel);
		
		// If we have parent-child relationships, display as hierarchy
		if (slots.Any(s => s.Value.ParentSlotId != null))
		{
			CreateHierarchicalLayout(container, slots, mainMenuSlot.Key);
		}
		else
		{
			CreateFlatLayout(container, slots, mainMenuSlot.Key);
		}
	}
	
	private void CreateHierarchicalLayout(VBoxContainer container, List<KeyValuePair<string, SlotState>> slots, string mainMenuSlotId)
	{
		// Find all root slots (those without parents) except the main menu
		var rootSlots = slots
			.Where(s => s.Value.ParentSlotId == null && s.Key != mainMenuSlotId)
			.OrderBy(s => !s.Value.IsActive)
			.ToList();
		
		foreach (var (rootId, rootState) in rootSlots)
		{
			// Create a root slot button
			var rootButton = CreateSlotButton(rootId, rootState, "└── ");
			container.AddChild(rootButton);
			
			// Add child slots if any
			var childSlots = slots.Where(s => s.Value.ParentSlotId == rootId).ToList();
			for (int i = 0; i < childSlots.Count; i++)
			{
				bool isLast = (i == childSlots.Count - 1);
				var (childId, childState) = childSlots[i];
				
				var prefix = isLast ? "    └── " : "    ├── ";
				var childButton = CreateSlotButton(childId, childState, prefix);
				container.AddChild(childButton);
			}
			
			// Add spacer between root slot groups
			container.AddChild(new Control { CustomMinimumSize = new Vector2(0, 5) });
		}
	}
	
	private void CreateFlatLayout(VBoxContainer container, List<KeyValuePair<string, SlotState>> slots, string mainMenuSlotId)
	{
		var filteredSlots = slots.Where(s => s.Key != mainMenuSlotId).ToList();
		
		for (int i = 0; i < filteredSlots.Count; i++)
		{
			var (slotId, state) = filteredSlots[i];
			bool isLast = i == filteredSlots.Count - 1;
			var prefix = isLast ? "└── " : "├── ";
			var slotButton = CreateSlotButton(slotId, state, prefix);
			container.AddChild(slotButton);
		}
	}
	
	private Control CreateSlotButton(string slotId, SlotState state, string prefix, bool isMainMenu = false)
	{
		// Create a horizontal container for button + resource indicator
		var buttonContainer = new HBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.Fill
		};
		
		// Create the main button
		var button = new Button
		{
			Text = $"{prefix}{GetSlotSymbol(state)} [{GetSlotText(state)}]",
			CustomMinimumSize = new Vector2(200, 30),
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,
			Disabled = !state.IsUnlocked
		};
		
		// Use TextAlignment to left align
		button.AddThemeConstantOverride("text_alignment", (int)HorizontalAlignment.Left);
		
		// Style the button based on slot state
		var style = new StyleBoxFlat();
		
		if (isMainMenu)
		{
			// Special styling for main menu
			style.BgColor = new Color(0, 0.2f, 0.4f, 0.8f); // Blue-ish for main menu
			style.BorderColor = new Color(0, 0.4f, 0.8f, 1.0f);
			style.BorderWidthBottom = 2;
			style.BorderWidthLeft = 2;
			style.BorderWidthRight = 2;
			style.BorderWidthTop = 2;
		}
		else if (state.IsActive)
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
		else
		{
			style.BgColor = new Color(0.2f, 0, 0, 0.5f); // Red for locked
			style.BorderColor = new Color(0.4f, 0, 0, 0.7f);
			style.BorderWidthBottom = 1;
			style.BorderWidthLeft = 1;
			style.BorderWidthRight = 1;
			style.BorderWidthTop = 1;
		}
		
		button.AddThemeStyleboxOverride("normal", style);
		
		// Hover style
		var hoverStyle = style.Duplicate() as StyleBoxFlat;
		if (hoverStyle != null)
		{
			if (isMainMenu)
			{
				hoverStyle.BgColor = new Color(0, 0.3f, 0.5f, 0.9f);
			}
			else if (state.IsActive)
			{
				hoverStyle.BgColor = new Color(0, 0.4f, 0, 0.8f);
			}
			else if (state.IsUnlocked)
			{
				hoverStyle.BgColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
			}
			button.AddThemeStyleboxOverride("hover", hoverStyle);
		}
		
		// Connect button press to slot selection in the SlotGridSystem
		button.Pressed += () => OnSlotButtonPressed(slotId);
		
		buttonContainer.AddChild(button);
		
		// Add resource indicator if active
		if (state.IsActive)
		{
			var resourceColor = GetResourceColor(state.MemoryUsage);
			var resourceIndicator = new ColorRect
			{
				Color = resourceColor,
				CustomMinimumSize = new Vector2(10, 10)
			};

			var indicatorContainer = new CenterContainer
			{
				CustomMinimumSize = new Vector2(20, 30), // Match button height
				SizeFlagsVertical = SizeFlags.Fill
			};
			indicatorContainer.AddChild(resourceIndicator);
			buttonContainer.AddChild(indicatorContainer);
		}
		
		return buttonContainer;
	}
	
	private string GetSlotText(SlotState state)
	{
		if (!string.IsNullOrEmpty(state.LoadedText))
		{
			return state.LoadedText.PadRight(10);
		}
		else if (state.IsUnlocked)
		{
			return "EMPTY".PadRight(10);
		}
		else
		{
			return "LOCKED".PadRight(10);
		}
	}
	
	private string GetSlotSymbol(SlotState slot)
	{
		if (!slot.IsUnlocked) return "⚿";  // Locked
		return slot.IsActive ? "■" : "□";   // Active/Inactive
	}
	
	private Color GetResourceColor(float usage)
	{
		if (usage <= LOW_USAGE)
			return new Color(0, 1, 0); // Green for low usage
		else if (usage <= MEDIUM_USAGE)
			return new Color(1, 1, 0); // Yellow for medium usage
		else
			return new Color(1, 0, 0); // Red for high usage
	}
	
	// Add a handler for button presses
	private void OnSlotButtonPressed(string slotId)
	{
		// Forward to the SlotGridSystem to handle slot selection
		_slotSystem.SelectSlot(slotId);
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
