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
	
	// Properties for viewport management
	private SubViewportContainer _viewportContainer;
	private SubViewport _subViewport;
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

		// Left panel (MEM slots) with panel background - make it flexible but with min size
		var leftPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(200, 0),
			SizeFlagsVertical = SizeFlags.Fill,
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,
			SizeFlagsStretchRatio = 0.25f  // Take up 25% of available space
		};
		leftPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		mainContainer.AddChild(leftPanel);

		var leftContent = new VBoxContainer
		{
			SizeFlagsVertical = SizeFlags.Fill,
			SizeFlagsHorizontal = SizeFlags.Fill
		};
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

		// Center panel (main content) with panel background - make it the largest
		var centerPanel = new PanelContainer
		{
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill,
			SizeFlagsStretchRatio = 0.5f  // Take up 50% of available space
		};
		centerPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		mainContainer.AddChild(centerPanel);

		_mainContent = new MarginContainer
		{
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill
		};
		_mainContent.AddThemeConstantOverride("margin_left", 10);
		_mainContent.AddThemeConstantOverride("margin_right", 10);
		_mainContent.AddThemeConstantOverride("margin_top", 10);
		_mainContent.AddThemeConstantOverride("margin_bottom", 10);
		centerPanel.AddChild(_mainContent);

		SetupMainMenuButtons();
		SetupViewportContainer();

		// Right panel (resources) - make it flexible but with min size
		var rightPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(200, 0),
			SizeFlagsVertical = SizeFlags.Fill,
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,
			SizeFlagsStretchRatio = 0.25f  // Take up 25% of available space
		};
		rightPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		mainContainer.AddChild(rightPanel);

		// Create the resource panel component
		var resourcePanel = new UI.Components.ResourcePanel();
		resourcePanel.Initialize(_slotManager);
		rightPanel.AddChild(resourcePanel);
	}

	private void SetupMainMenuButtons()
	{
		var buttonContainer = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(250, 0),
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,  // Allow expansion
			SizeFlagsVertical = SizeFlags.Fill,
			// Center alignment is not available, so using standard alignment
		};
		buttonContainer.AddThemeConstantOverride("separation", 20);  // More space between buttons
		_mainContent.AddChild(buttonContainer);

		// Add a title/header
		var titleLabel = new Label
		{
			Text = "NETRUNNER OS",
			HorizontalAlignment = HorizontalAlignment.Center,
			CustomMinimumSize = new Vector2(0, 40)
		};
		buttonContainer.AddChild(titleLabel);

		// Add some space before buttons
		var spacer = new Control { CustomMinimumSize = new Vector2(0, 20) };
		buttonContainer.AddChild(spacer);

		// Create button with style
		_cardGameButton = CreateStyledButton("CARD GAME", Colors.Green);
		buttonContainer.AddChild(_cardGameButton);

		_debugButton = CreateStyledButton("DEBUG SANDBOX", Colors.Orange);
		buttonContainer.AddChild(_debugButton);
		
		// Add spacer at the bottom too to help with centering
		var bottomSpacer = new Control 
		{ 
			SizeFlagsVertical = SizeFlags.Expand 
		};
		buttonContainer.AddChild(bottomSpacer);
	}

	private Button CreateStyledButton(string text, Color accentColor)
	{
		var button = new Button
		{
			Text = text,
			CustomMinimumSize = new Vector2(0, 50),  // Taller buttons
			SizeFlagsHorizontal = SizeFlags.Fill,    // Fill width
			SizeFlagsVertical = SizeFlags.Fill      // Fill height
		};
		
		// Normal state
		var normalStyle = new StyleBoxFlat
		{
			BgColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),  // Dark background
			BorderColor = accentColor,                    // Accent color border
			BorderWidthBottom = 2,
			BorderWidthLeft = 2,
			BorderWidthRight = 2,
			BorderWidthTop = 2,
			ContentMarginLeft = 15,
			ContentMarginRight = 15,
			ContentMarginTop = 10,
			ContentMarginBottom = 10
		};
		button.AddThemeStyleboxOverride("normal", normalStyle);
		
		// Hover state
		var hoverStyle = normalStyle.Duplicate() as StyleBoxFlat;
		if (hoverStyle != null)
		{
			hoverStyle.BgColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);  // Slightly lighter
			hoverStyle.BorderWidthBottom = 3;
			hoverStyle.BorderWidthLeft = 3;
			hoverStyle.BorderWidthRight = 3;
			hoverStyle.BorderWidthTop = 3;
		}
		button.AddThemeStyleboxOverride("hover", hoverStyle);
		
		// Pressed state
		var pressedStyle = normalStyle.Duplicate() as StyleBoxFlat;
		if (pressedStyle != null)
		{
			pressedStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);  // Even lighter when pressed
			pressedStyle.BorderColor = new Color(accentColor, 1.0f);   // Full brightness border
		}
		button.AddThemeStyleboxOverride("pressed", pressedStyle);
		
		return button;
	}

	private void SetupViewportContainer()
	{
		// Create a SubViewportContainer for scene rendering
		_viewportContainer = new SubViewportContainer
		{
			Visible = false,
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill,
			AnchorsPreset = (int)LayoutPreset.FullRect,  // Fill parent container
			GrowHorizontal = GrowDirection.Both,
			GrowVertical = GrowDirection.Both,
			Stretch = true  // Make viewport content stretch to container size
		};
		_mainContent.AddChild(_viewportContainer);
		
		// Create the actual SubViewport that will hold scenes
		_subViewport = new SubViewport
		{
			HandleInputLocally = true,  // Let scenes handle their own input
			RenderTargetUpdateMode = SubViewport.UpdateMode.Always
		};
		_viewportContainer.AddChild(_subViewport);
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
		// Clear existing content from SubViewport
		foreach (Node child in _subViewport.GetChildren())
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
			
			// Add the scene to the SubViewport (not directly to the container)
			_subViewport.AddChild(instance);
			
			// Make the container visible
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
		GD.Print("Handling scene unload request");
		
		// Find the active process and unload it
		foreach (var processId in _processManager.GetActiveProcessIds())
		{
			GD.Print($"Unloading process: {processId}");
			_processManager.UnloadProcess(processId);
		}

		// Clear viewport with deferred call to avoid issues during signal processing
		CallDeferred(nameof(DeferredClearViewport));
	}
	
	private void DeferredClearViewport()
	{
		// Clear viewport content
		foreach (Node child in _subViewport.GetChildren())
		{
			child.QueueFree();
		}
		
		// Hide the viewport container
		_viewportContainer.Visible = false;

		// Show menu buttons again
		foreach (var child in _mainContent.GetChildren())
		{
			if (child != _viewportContainer && child is Control control)
			{
				control.Visible = true;
			}
		}
		
		GD.Print("Viewport cleared, menu buttons restored");
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
