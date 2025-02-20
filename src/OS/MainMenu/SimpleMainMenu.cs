// src/OS/MainMenu/SimpleMainMenu.cs
using Godot;
using Trivale.Memory;
using Trivale.UI.Components;
using Trivale.OS.MainMenu;
using Trivale.OS.MainMenu.Processes;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;

namespace Trivale.OS;

/// <summary>
/// Main menu system that coordinates process creation, scene loading, and UI management.
/// 
/// Architecture:
/// - ProcessManager: Handles process lifecycle (creation, loading, unloading)
/// - SlotManager: Manages memory slots and their states
/// - UI Components: Display slot states and handle user interaction
/// 
/// Process/Scene Lifecycle:
/// 1. Button Press -> ProcessManager creates process -> SlotManager allocates slot -> Scene loads
/// 2. Scene signals unload -> ProcessManager unloads process -> SlotManager frees slot -> UI restored
/// 
/// System Responsibilities:
/// - Scene Management: Loading scenes and handling their unload requests
/// - UI Coordination: Managing menu state and viewport
/// - Event Handling: Connecting UI actions to ProcessManager operations
/// 
/// Loaded Scene Contracts:
/// - Scenes must implement SceneUnloadRequested signal
/// - Scenes should not manage their own unloading
/// - Scenes can expect proper cleanup when signaling unload
/// 
/// Dependencies:
/// - IProcessManager: For process lifecycle management
/// - ISlotManager: For slot state management
/// - SlotGridSystem: For UI representation of slots
/// </summary>
public partial class SimpleMainMenu : Control
{
	private SlotGridSystem _slotSystem;
	private Button _cardGameButton;
	private Button _debugButton;
	private Control _viewportContainer;
	private Control _mainContent;
	
	// Process and slot management
	private IProcessManager _processManager;
	private ISlotManager _slotManager;
	
	private const string CardGameScenePath = "res://Scenes/MainMenu/CardGameScene.tscn";
	private const string DebugScenePath = "res://Scenes/MainMenu/DebugScene.tscn";

	public override void _Ready()
	{
		CustomMinimumSize = new Vector2(800, 600);

		// Create managers
		_slotManager = new SlotManager(2, 2);  // 2x2 grid of slots
		_processManager = new ProcessManager(_slotManager);

		// Subscribe to manager events
		_processManager.ProcessStarted += OnProcessStarted;
		_processManager.ProcessEnded += OnProcessEnded;
		_slotManager.SlotStatusChanged += OnSlotStatusChanged;

		SetLayout();
		ConnectSignals();
	}

	public override void _ExitTree()
	{
		base._ExitTree();

		// Unsubscribe from events
		if (_processManager != null)
		{
			_processManager.ProcessStarted -= OnProcessStarted;
			_processManager.ProcessEnded -= OnProcessEnded;
		}

		if (_slotManager != null)
		{
			_slotManager.SlotStatusChanged -= OnSlotStatusChanged;
		}
	}

	private StyleBoxFlat CreatePanelStyle()
	{
		return new StyleBoxFlat
		{
			BgColor = new Color(0, 0.05f, 0, 0.9f),  // Very dark green
			BorderColor = new Color(0, 1, 0),         // Bright green
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1
		};
	}

	private void SetLayout()
	{
		// Set up the root Control node to fill the window
		LayoutMode = 1;  // Use anchors
		AnchorsPreset = (int)LayoutPreset.FullRect;
		GrowHorizontal = GrowDirection.Both;
		GrowVertical = GrowDirection.Both;

		// Main container with margins
		var marginContainer = new MarginContainer
		{
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
			GrowHorizontal = GrowDirection.Both,
			GrowVertical = GrowDirection.Both
		};
		marginContainer.AddThemeConstantOverride("margin_left", 20);
		marginContainer.AddThemeConstantOverride("margin_right", 20);
		marginContainer.AddThemeConstantOverride("margin_top", 20);
		marginContainer.AddThemeConstantOverride("margin_bottom", 20);
		AddChild(marginContainer);

		// Main horizontal container
		var mainContainer = new HBoxContainer
		{
			AnchorsPreset = (int)LayoutPreset.FullRect,
			GrowHorizontal = GrowDirection.Both,
			GrowVertical = GrowDirection.Both,
			Theme = new Theme()
		};
		marginContainer.AddChild(mainContainer);

		// Left panel (MEM slots) with panel background
		var leftPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(200, 0),
			SizeFlagsVertical = SizeFlags.Fill
		};
		leftPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		mainContainer.AddChild(leftPanel);

		var leftContent = new VBoxContainer();
		leftPanel.AddChild(leftContent);

		// MEM header
		var memHeader = new Label
		{
			Text = "MEM",
			CustomMinimumSize = new Vector2(0, 30)
		};
		leftContent.AddChild(memHeader);

		// Slot grid system
		_slotSystem = new SlotGridSystem();
		_slotSystem.Initialize(_slotManager);
		leftContent.AddChild(_slotSystem);

		// Slot grid display
		var slotDisplay = new SlotGridDisplay();
		slotDisplay.Initialize(_slotSystem);
		leftContent.AddChild(slotDisplay);

		// Center panel (main content) with panel background
		var centerPanel = new PanelContainer
		{
			SizeFlagsHorizontal = SizeFlags.Expand,
			SizeFlagsVertical = SizeFlags.Fill
		};
		centerPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		mainContainer.AddChild(centerPanel);

		_mainContent = new MarginContainer();
		_mainContent.AddThemeConstantOverride("margin_left", 10);
		_mainContent.AddThemeConstantOverride("margin_right", 10);
		_mainContent.AddThemeConstantOverride("margin_top", 10);
		_mainContent.AddThemeConstantOverride("margin_bottom", 10);
		centerPanel.AddChild(_mainContent);

		SetupMainMenuButtons();
		SetupViewportContainer();

		// Right panel (resources)
		var rightPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(150, 0),
			SizeFlagsVertical = SizeFlags.Fill
		};
		rightPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		mainContainer.AddChild(rightPanel);

		var rightContent = new VBoxContainer();
		rightPanel.AddChild(rightContent);

		var resourceHeader = new Label
		{
			Text = "Resources:",
			CustomMinimumSize = new Vector2(0, 30)
		};
		rightContent.AddChild(resourceHeader);

		var resourceLabel = new Label { Text = "MEM\nHealth\netc." };
		rightContent.AddChild(resourceLabel);
	}

	private void SetupMainMenuButtons()
	{
		var buttonContainer = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(300, 0),
			SizeFlagsHorizontal = 0,  // Don't expand horizontally
			SizeFlagsVertical = SizeFlags.Fill
		};
		buttonContainer.AddThemeConstantOverride("separation", 10);
		_mainContent.AddChild(buttonContainer);

		_cardGameButton = new Button
		{
			Text = "CARD GAME PLACEHOLDER",
			CustomMinimumSize = new Vector2(0, 40)
		};
		buttonContainer.AddChild(_cardGameButton);

		_debugButton = new Button
		{
			Text = "DEBUG SANDBOX",
			CustomMinimumSize = new Vector2(0, 40)
		};
		buttonContainer.AddChild(_debugButton);
	}

	private void SetupViewportContainer()
	{
		_viewportContainer = new Control
		{
			Visible = false,
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill
		};
		_mainContent.AddChild(_viewportContainer);
	}

	private void ConnectSignals()
	{
		_cardGameButton.Pressed += () => OnSceneButtonPressed(CardGameScenePath, "CARD GAME");
		_debugButton.Pressed += () => OnSceneButtonPressed(DebugScenePath, "DEBUG");
	}

	private void OnSceneButtonPressed(string scenePath, string displayName)
	{
		var processType = displayName switch
		{
			"CARD GAME" => "CardGame",
			"DEBUG" => "Debug",
			_ => null
		};
		
		if (processType == null) return;

		var processId = _processManager.CreateProcess(processType);
		if (processId != null && _processManager.StartProcess(processId, out var slotId))
		{
			LoadSceneInViewport(scenePath);
			GD.Print($"Loaded process {processId} of type {processType} in slot {slotId}");
		}
		else
		{
			GD.PrintErr($"Failed to create/start process for {displayName}");
		}
	}

	private void LoadSceneInViewport(string scenePath)
	{
		// Clear existing content
		foreach (Node child in _viewportContainer.GetChildren())
		{
			child.QueueFree();
		}

		// Load new scene
		var sceneResource = ResourceLoader.Load<PackedScene>(scenePath);
		if (sceneResource != null)
		{
			var instance = sceneResource.Instantiate();
			
			// Initialize scene with managers if it's DebugScene
			if (instance is DebugScene debugScene)
			{
				debugScene.Initialize(_processManager, _slotManager);
			}
			
			// Connect to the scene's unload signal
			if (instance.HasSignal("SceneUnloadRequested"))
			{
				instance.Connect("SceneUnloadRequested", new Callable(this, nameof(HandleSceneUnloadRequest)));
			}
			
			_viewportContainer.AddChild(instance);
			_viewportContainer.Visible = true;

			// Hide menu buttons when showing scene
			foreach (var child in _mainContent.GetChildren())
			{
				if (child != _viewportContainer && child is Control control)
				{
					control.Visible = false;
				}
			}
		}
		else
		{
			GD.PrintErr($"Failed to load scene: {scenePath}");
		}
	}

	private void HandleSceneUnloadRequest()
	{
		// Find the active process and unload it
		foreach (var slot in _slotManager.GetAllSlots())
		{
			if (slot.Status == SlotStatus.Active && slot.CurrentProcess != null)
			{
				_processManager.UnloadProcess(slot.CurrentProcess.Id);
				break;
			}
		}

		// Clear viewport
		foreach (Node child in _viewportContainer.GetChildren())
		{
			child.QueueFree();
		}
		_viewportContainer.Visible = false;

		// Show menu buttons again
		foreach (var child in _mainContent.GetChildren())
		{
			if (child != _viewportContainer && child is Control control)
			{
				control.Visible = true;
			}
		}
	}

	private void OnProcessStarted(string processId, string slotId)
	{
		GD.Print($"Process {processId} started in slot {slotId}");
	}

	private void OnProcessEnded(string processId)
	{
		GD.Print($"Process {processId} ended");
	}

	private void OnSlotStatusChanged(string slotId, SlotStatus status)
	{
		GD.Print($"Slot {slotId} changed status to {status}");
	}
}
