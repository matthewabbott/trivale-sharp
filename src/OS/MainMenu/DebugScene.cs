// src/OS/MainMenu/DebugScene.cs

using Godot;
using System.Linq;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;
using Trivale.UI.Components;
using System.Collections.Generic;

namespace Trivale.OS.MainMenu;

/// <summary>
/// Debug sandbox scene demonstrating MEM slot system integration.
/// This scene serves as a reference implementation for how to:
/// 1. Set up process and slot management in a scene
/// 2. Handle process lifecycle (create, load, unload)
/// 3. Display and update slot states
/// 4. Clean up properly when exiting
/// 
/// Integration Pattern:
/// 1. Create slot and process managers at scene start
/// 2. Hook up UI controls to process operations
/// 3. Listen to manager events for state updates
/// 4. Clean up processes on scene exit
/// </summary>
public partial class DebugScene : Control, IOrchestratableScene
{
	[Signal]
	public delegate void SceneUnloadRequestedEventHandler();
	
	private IProcessManager _processManager;
	private ISlotManager _slotManager;
	private Label _statusLabel;
	private HBoxContainer _buttonContainer;
	private Button _createProcessButton;
	private Button _unloadProcessButton;
	private Button _addSlotButton;     // New button for adding slots
	private Button _removeSlotButton;  // New button for removing slots
	private SlotGridSystem _slotGridSystem;
	private SlotGridDisplay _slotDisplay;
	private SceneOrchestrator _orchestrator;
	private List<string> _createdProcessIds = new List<string>();
	
	public void Initialize(IProcessManager processManager, ISlotManager slotManager)
	{
		_processManager = processManager;
		_slotManager = slotManager;
	}

	public override void _Ready()
	{
		// Configure the root Control to fill its parent viewport
		LayoutMode = 1; // Use anchors
		AnchorsPreset = (int)LayoutPreset.FullRect;
		GrowHorizontal = GrowDirection.Both;
		GrowVertical = GrowDirection.Both;

		SetupUI();
		ConnectSignals();
	}

	public void SetOrchestrator(SceneOrchestrator orchestrator)
	{
		_orchestrator = orchestrator;
	}

	public string GetProcessId()
	{
		return HasMeta("ProcessId") ? (string)GetMeta("ProcessId") : null;
	}
	
	private Button _createNewSlotButton; // New button

	private Button _setParentButton;    // New button
	
	private void SetupUI()
	{
		// Main vertical layout
		var layout = new VBoxContainer
		{
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
			GrowHorizontal = GrowDirection.Both,
			GrowVertical = GrowDirection.Both,
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill
		};
		AddChild(layout);

		// Add margins to respect parent container's padding
		var marginContainer = new MarginContainer
		{
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill
		};
		marginContainer.AddThemeConstantOverride("margin_left", 10);
		marginContainer.AddThemeConstantOverride("margin_right", 10);
		marginContainer.AddThemeConstantOverride("margin_top", 10);
		marginContainer.AddThemeConstantOverride("margin_bottom", 10);
		layout.AddChild(marginContainer);

		// Create a VBoxContainer inside the margin container for the actual content
		var contentLayout = new VBoxContainer
		{
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill
		};
		marginContainer.AddChild(contentLayout);

		// Status label at top
		_statusLabel = new Label { 
			Text = "Process Management Test",
			HorizontalAlignment = HorizontalAlignment.Center
		};
		contentLayout.AddChild(_statusLabel);

		// Button container - ensure it stays within bounds
		_buttonContainer = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(0, 40),
			SizeFlagsHorizontal = SizeFlags.ShrinkCenter, // This centers the container
		};
		_buttonContainer.AddThemeConstantOverride("separation", 10); // Space between buttons
		contentLayout.AddChild(_buttonContainer);
		
		// Create a second button container for less important buttons
		var secondaryButtonContainer = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(0, 40),
			SizeFlagsHorizontal = SizeFlags.ShrinkCenter
		};
		contentLayout.AddChild(secondaryButtonContainer);
		
		// Create a third button container for advanced features
		var advancedButtonContainer = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(0, 40),
			SizeFlagsHorizontal = SizeFlags.ShrinkCenter
		};
		contentLayout.AddChild(advancedButtonContainer);
		
		// Main control buttons with proper styling
		_createProcessButton = CreateStyledButton("Load Debug Process", Colors.Green);
		_unloadProcessButton = CreateStyledButton("Unload Process", Colors.Red);
		var returnButton = CreateStyledButton("Return to Menu", Colors.White);
		
		_buttonContainer.AddChild(_createProcessButton);
		_buttonContainer.AddChild(_unloadProcessButton);
		_buttonContainer.AddChild(returnButton);
		
		// Slot management buttons in second row
		_addSlotButton = CreateStyledButton("Unlock Slot", Colors.Blue);
		_removeSlotButton = CreateStyledButton("Lock Slot", Colors.Orange);
		_createNewSlotButton = CreateStyledButton("Create New Slot", Colors.Purple);
		
		secondaryButtonContainer.AddChild(_addSlotButton);
		secondaryButtonContainer.AddChild(_removeSlotButton);
		secondaryButtonContainer.AddChild(_createNewSlotButton);
		
		// Advanced buttons in third row
		_setParentButton = CreateStyledButton("Set Parent/Child", new Color(0.5f, 0.0f, 0.5f)); // Purple
		
		advancedButtonContainer.AddChild(_setParentButton);

		// Create a container for scene switching tests
		var sceneSwitchContainer = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(0, 40),
			SizeFlagsHorizontal = SizeFlags.ShrinkCenter
		};
		contentLayout.AddChild(sceneSwitchContainer);
		
		// Add process creation buttons for different process types
		var createCardGameButton = CreateStyledButton("New Card Game", Colors.Green);
		var createDebugButton = CreateStyledButton("New Debug Process", Colors.Purple);
		var switchButton = CreateStyledButton("Switch to First Slot", Colors.Blue);
		
		sceneSwitchContainer.AddChild(createCardGameButton);
		sceneSwitchContainer.AddChild(createDebugButton);
		sceneSwitchContainer.AddChild(switchButton);
		
		// Connect the buttons
		createCardGameButton.Pressed += OnCreateCardGamePressed;
		createDebugButton.Pressed += OnCreateDebugProcessPressed;
		switchButton.Pressed += OnSwitchToFirstSlotPressed;
		
		// Connect signals
		_createProcessButton.Pressed += OnCreateProcessPressed;
		_unloadProcessButton.Pressed += OnUnloadProcessPressed;
		_addSlotButton.Pressed += OnAddSlotPressed;
		_removeSlotButton.Pressed += OnRemoveSlotPressed;
		_createNewSlotButton.Pressed += OnCreateNewSlotPressed;
		_setParentButton.Pressed += OnSetParentPressed;
		returnButton.Pressed += OnReturnPressed;
	}
	
	private Button CreateStyledButton(string text, Color color)
	{
		var button = new Button
		{
			Text = text,
			CustomMinimumSize = new Vector2(150, 30)
		};
		
		var normalStyle = new StyleBoxFlat
		{
			BgColor = new Color(color, 0.2f),
			BorderColor = new Color(color, 0.8f),
			BorderWidthBottom = 1,
			BorderWidthLeft = 1,
			BorderWidthRight = 1,
			BorderWidthTop = 1,
			ContentMarginLeft = 10,
			ContentMarginRight = 10
		};
		button.AddThemeStyleboxOverride("normal", normalStyle);
		
		var hoverStyle = normalStyle.Duplicate() as StyleBoxFlat;
		if (hoverStyle != null)
		{
			hoverStyle.BgColor = new Color(color, 0.3f);
		}
		button.AddThemeStyleboxOverride("hover", hoverStyle);
		
		var pressedStyle = normalStyle.Duplicate() as StyleBoxFlat;
		if (pressedStyle != null)
		{
			pressedStyle.BgColor = new Color(color, 0.4f);
		}
		button.AddThemeStyleboxOverride("pressed", pressedStyle);
		
		return button;
	}

	private void ConnectSignals()
	{
		// Connect Process Events with named methods (not lambdas)
		_processManager.ProcessStarted += OnProcessStarted;
		_processManager.ProcessEnded += OnProcessEnded;
			
		// Connect Slot Events with named methods (not lambdas)
		_slotManager.SlotStatusChanged += OnSlotStatusChanged;
		_slotManager.SlotUnlocked += OnSlotUnlocked;
	}

	// Named event handlers for ProcessManager events
	private void OnProcessStarted(string processId, string slotId)
	{
		_statusLabel.Text = $"Started process {processId} in slot {slotId}";
		// ProcessManager will automatically unlock additional slots
	}

	private void OnProcessEnded(string processId)
	{
		_statusLabel.Text = $"Ended process {processId}";
		UpdateUI();
	}

	// Named event handlers for SlotManager events
	private void OnSlotStatusChanged(string slotId, SlotStatus status)
	{
		UpdateUI();
	}

	private void OnSlotUnlocked(string slotId)
	{
		_statusLabel.Text = $"Slot {slotId} unlocked";
		UpdateUI();
	}
	
	private void UpdateUI()
	{
		// Update button states based on current system state
		var hasActiveProcess = _processManager.GetActiveProcessIds().Any();
		_unloadProcessButton.Disabled = !hasActiveProcess;
		
		var hasAvailableSlot = _slotManager.GetAllSlots().Any(s => 
			s.IsUnlocked && s.Status == SlotStatus.Empty);
		_createProcessButton.Disabled = !hasAvailableSlot;
		
		// Update Remove Slot button state - can only remove if there are unlocked slots
		var hasUnlockedSlots = _slotManager.GetAllSlots().Any(s => s.IsUnlocked);
		_removeSlotButton.Disabled = !hasUnlockedSlots;
	}
	
	private void OnCreateProcessPressed()
	{
		var processId = _processManager.CreateProcess("Debug");
		if (processId != null)
		{
			if (_processManager.StartProcess(processId, out _))
			{
				_statusLabel.Text = $"Created and started process {processId}";
				_createdProcessIds.Add(processId); // Track the created process
			}
			else
			{
				_statusLabel.Text = $"Failed to start process {processId}";
			}
		}
		else
		{
			_statusLabel.Text = "Failed to create process";
		}
	}
	
	private void OnUnloadProcessPressed()
	{
		// STEP 5: Process Cleanup Pattern
		var activeProcess = _processManager.GetActiveProcessIds().FirstOrDefault();
		if (activeProcess != null)
		{
			_processManager.UnloadProcess(activeProcess);
		}
	}
	
	private void OnAddSlotPressed()
	{
		// Find the first locked slot
		var slots = _slotManager.GetAllSlots();
		var lockedSlot = slots.FirstOrDefault(s => !s.IsUnlocked);
		
		if (lockedSlot != null)
		{
			_slotManager.UnlockSlot(lockedSlot.Id);
			_statusLabel.Text = $"Unlocked slot {lockedSlot.Id}";
		}
		else
		{
			_statusLabel.Text = "No more locked slots available";
		}
		
		UpdateUI();
	}
	
	private void OnRemoveSlotPressed()
	{
		// Find the last unlocked empty slot
		var slots = _slotManager.GetAllSlots();
		var slotToRemove = slots.LastOrDefault(s => 
			s.IsUnlocked && s.Status == SlotStatus.Empty);
		
		if (slotToRemove != null)
		{
			// We need to cast to SlotManager to access the internal LockSlot method
			// If you don't have such a method, you'll need to add it to your SlotManager class
			if (_slotManager is SlotManager manager)
			{
				if (manager.LockSlot(slotToRemove.Id))
				{
					_statusLabel.Text = $"Locked slot {slotToRemove.Id}";
				}
				else
				{
					_statusLabel.Text = $"Could not lock slot {slotToRemove.Id}";
				}
			}
			else
			{
				_statusLabel.Text = "Cannot access slot manager lock function";
			}
		}
		else
		{
			_statusLabel.Text = "No empty slots to remove";
		}
		
		UpdateUI();
	}
	
	private void OnCreateNewSlotPressed()
	{
		// We need to cast to SlotManager to access the CreateNewSlot method
		if (_slotManager is SlotManager manager)
		{
			// Create a new slot (unlocked or locked)
			bool createUnlocked = true; // Set to false if you want new slots to start locked
			string newSlotId = manager.CreateNewSlot(createUnlocked);
			
			if (!string.IsNullOrEmpty(newSlotId))
			{
				string state = createUnlocked ? "unlocked" : "locked";
				_statusLabel.Text = $"Created new {state} slot {newSlotId}";
			}
			else
			{
				_statusLabel.Text = "Failed to create new slot";
			}
		}
		else
		{
			_statusLabel.Text = "Cannot access slot manager creation function";
		}
		
		UpdateUI();
	}
	
	private void OnSetParentPressed()
	{
		// Find available slots
		var slots = _slotManager.GetAllSlots().ToList();
		
		// Need at least two unlocked slots
		var unlockedSlots = slots.Where(s => s.IsUnlocked).ToList();
		if (unlockedSlots.Count < 2)
		{
			_statusLabel.Text = "Need at least 2 unlocked slots";
			return;
		}
		
		// Set up parent-child relationship between the first two unlocked slots
		var parent = unlockedSlots[0];
		var child = unlockedSlots[1];
		
		// Find the SlotGridSystem to set the parent-child relationship
		var slotGridSystem = FindSlotGridSystem();
		if (slotGridSystem != null)
		{
			slotGridSystem.SetSlotParent(child.Id, parent.Id);
			_statusLabel.Text = $"Set {parent.Id} as parent of {child.Id}";
		}
		else
		{
			_statusLabel.Text = "Could not find SlotGridSystem";
		}
		
		UpdateUI();
	}
	
	// Helper to find the SlotGridSystem in the scene
	private UI.Components.SlotGridSystem FindSlotGridSystem()
	{
		// Try to find the SlotGridSystem in the tree
		var root = GetTree()?.Root;
		if (root == null) return null;
		
		return FindRecursive<UI.Components.SlotGridSystem>(root);
	}
	
	// Helper to recursively find a node of a specific type
	private T FindRecursive<T>(Node parent) where T : class
	{
		foreach (var child in parent.GetChildren())
		{
			if (child is T foundNode)
				return foundNode;
				
			var result = FindRecursive<T>(child);
			if (result != null)
				return result;
		}
		
		return null;
	}

	private void OnReturnPressed()
	{
		// Clean up only the processes that this debug scene created
		if (_processManager != null)
		{
			foreach (var processId in _createdProcessIds.ToList())
			{
				_processManager.UnloadProcess(processId);
				_createdProcessIds.Remove(processId);
			}
		}
		
		if (_orchestrator != null)
		{
			// Get process ID from metadata
			string processId = null;
			if (HasMeta("ProcessId"))
			{
				processId = (string)GetMeta("ProcessId");
			}
			
			// Use direct method call to unload just this scene
			_orchestrator.RequestSceneUnload(processId);
		}
		else
		{
			GD.PrintErr("DebugScene: Orchestrator not set, can't request unload");
		}
	}

	private void OnCreateCardGamePressed()
	{
		var processId = _processManager.CreateProcess("CardGame");
		if (processId != null)
		{
			if (_processManager.StartProcess(processId, out var slotId))
			{
				_statusLabel.Text = $"Created and started CardGame process {processId} in slot {slotId}";
				_createdProcessIds.Add(processId);
			}
			else
			{
				_statusLabel.Text = $"Failed to start CardGame process {processId}";
			}
		}
		else
		{
			_statusLabel.Text = "Failed to create CardGame process";
		}
	}

	private void OnCreateDebugProcessPressed()
	{
		var processId = _processManager.CreateProcess("Debug");
		if (processId != null)
		{
			if (_processManager.StartProcess(processId, out var slotId))
			{
				_statusLabel.Text = $"Created and started Debug process {processId} in slot {slotId}";
				_createdProcessIds.Add(processId);
			}
			else
			{
				_statusLabel.Text = $"Failed to start Debug process {processId}";
			}
		}
		else
		{
			_statusLabel.Text = "Failed to create Debug process";
		}
	}

	private void OnSwitchToFirstSlotPressed()
	{
		// Find the first unlocked slot
		var firstSlot = _slotManager.GetAllSlots().FirstOrDefault(s => s.IsUnlocked);
		if (firstSlot != null)
		{
			_statusLabel.Text = $"Attempting to switch to slot {firstSlot.Id}";
			
			// Use the slot grid system to select this slot
			var slotGridSystem = FindSlotGridSystem();
			if (slotGridSystem != null)
			{
				slotGridSystem.SelectSlot(firstSlot.Id);
				_statusLabel.Text = $"Selected slot {firstSlot.Id}";
			}
			else
			{
				_statusLabel.Text = "Could not find SlotGridSystem";
			}
		}
		else
		{
			_statusLabel.Text = "No unlocked slots found";
		}
	}
	
	public override void _ExitTree()
	{
		// Disconnect all signals
		if (_processManager != null)
		{
			_processManager.ProcessStarted -= OnProcessStarted;
			_processManager.ProcessEnded -= OnProcessEnded;
		}
		
		if (_slotManager != null)
		{
			_slotManager.SlotStatusChanged -= OnSlotStatusChanged;
			_slotManager.SlotUnlocked -= OnSlotUnlocked;
		}
		
		// Disconnect button signals
		if (_createProcessButton != null)
		{
			_createProcessButton.Pressed -= OnCreateProcessPressed;
		}
		
		if (_unloadProcessButton != null)
		{
			_unloadProcessButton.Pressed -= OnUnloadProcessPressed;
		}
		
		if (_addSlotButton != null)
		{
			_addSlotButton.Pressed -= OnAddSlotPressed;
		}
		
		if (_removeSlotButton != null)
		{
			_removeSlotButton.Pressed -= OnRemoveSlotPressed;
		}
		
		if (_createNewSlotButton != null)
		{
			_createNewSlotButton.Pressed -= OnCreateNewSlotPressed;
		}
		
		if (_setParentButton != null)
		{
			_setParentButton.Pressed -= OnSetParentPressed;
		}
		
		// Disconnect scene switching buttons
		var sceneSwitchButtons = GetNodeOrNull("ContentLayout/SceneSwitchContainer");
		if (sceneSwitchButtons != null)
		{
			foreach (var child in sceneSwitchButtons.GetChildren())
			{
				if (child is Button button)
				{
					if (button.Text == "New Card Game")
						button.Pressed -= OnCreateCardGamePressed;
					else if (button.Text == "New Debug Process")
						button.Pressed -= OnCreateDebugProcessPressed;
					else if (button.Text == "Switch to First Slot")
						button.Pressed -= OnSwitchToFirstSlotPressed;
				}
			}
		}

		base._ExitTree();
	}
}
