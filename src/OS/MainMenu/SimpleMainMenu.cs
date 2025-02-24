// src/OS/MainMenu/SimpleMainMenu.cs
using Godot;
using Trivale.Memory;
using Trivale.UI.Components;
using Trivale.OS;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;

namespace Trivale.OS;

/// <summary>
/// Main menu system that coordinates process creation, scene loading, and UI management.
/// 
/// Architecture:
/// - ProcessManager: Handles process lifecycle (creation, loading, unloading)
/// - SlotManager: Manages memory slots and their states
/// - SceneOrchestrator: Manages scene lifecycle and coordinates with process system
/// - UI Components: Display slot states and handle user interaction
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
/// - SceneOrchestrator: For scene lifecycle management
/// </summary>
public partial class SimpleMainMenu : Control
{
	private SlotGridSystem _slotSystem;
	private Button _cardGameButton;
	private Button _debugButton;
	private Control _mainContent;
	private VBoxContainer _buttonContainer;
	
	// Process and slot management
	private IProcessManager _processManager;
	private ISlotManager _slotManager;
	private SceneOrchestrator _sceneOrchestrator;
	
	private const string CardGameScenePath = "res://Scenes/MainMenu/CardGameScene.tscn";
	private const string DebugScenePath = "res://Scenes/MainMenu/DebugScene.tscn";

	public override void _Ready()
	{
		CustomMinimumSize = new Vector2(800, 600);

		// Create managers
		_slotManager = new SlotManager(2, 2);  // 2x2 grid of slots
		_processManager = new ProcessManager(_slotManager);

		// Create and initialize scene orchestrator
		_sceneOrchestrator = new SceneOrchestrator();
		AddChild(_sceneOrchestrator);

		SetLayout();
		
		// Initialize orchestrator with references it needs
		_sceneOrchestrator.Initialize(_processManager, _slotManager, _mainContent);
		_sceneOrchestrator.SceneUnloaded += OnSceneUnloaded;

		ConnectSignals();
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
		_buttonContainer = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(250, 0),
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,  // Allow expansion
			SizeFlagsVertical = SizeFlags.Fill,
		};
		_buttonContainer.AddThemeConstantOverride("separation", 20);  // More space between buttons
		_mainContent.AddChild(_buttonContainer);

		// Add a title/header
		var titleLabel = new Label
		{
			Text = "NETRUNNER OS",
			HorizontalAlignment = HorizontalAlignment.Center,
			CustomMinimumSize = new Vector2(0, 40)
		};
		_buttonContainer.AddChild(titleLabel);

		// Add some space before buttons
		var spacer = new Control { CustomMinimumSize = new Vector2(0, 20) };
		_buttonContainer.AddChild(spacer);

		// Create button with style
		_cardGameButton = CreateStyledButton("CARD GAME", Colors.Green);
		_buttonContainer.AddChild(_cardGameButton);

		_debugButton = CreateStyledButton("DEBUG SANDBOX", Colors.Orange);
		_buttonContainer.AddChild(_debugButton);
		
		// Add spacer at the bottom too to help with centering
		var bottomSpacer = new Control 
		{ 
			SizeFlagsVertical = SizeFlags.Expand 
		};
		_buttonContainer.AddChild(bottomSpacer);
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

	private void ConnectSignals()
	{
		_cardGameButton.Pressed += () => OnSceneButtonPressed(CardGameScenePath, "CardGame");
		_debugButton.Pressed += () => OnSceneButtonPressed(DebugScenePath, "Debug");
	}

	private void OnSceneButtonPressed(string scenePath, string processType)
	{
		if (!_sceneOrchestrator.LoadScene(processType, scenePath))
		{
			GD.PrintErr($"Failed to load scene {scenePath}");
			return;
		}
		
		// Hide menu buttons while scene is active
		if (_buttonContainer != null)
		{
			_buttonContainer.Visible = false;
		}
	}

	private void OnSceneUnloaded()
	{
		// Restore menu state
		if (_buttonContainer != null)
		{
			_buttonContainer.Visible = true;
		}
	}

	public override void _ExitTree()
	{
		if (_sceneOrchestrator != null)
		{
			_sceneOrchestrator.SceneUnloaded -= OnSceneUnloaded;
		}
		
		base._ExitTree();
	}
}
