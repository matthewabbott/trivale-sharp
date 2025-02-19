// src/OS/MainMenu/SimpleMainMenu.cs
using Godot;
using Trivale.Memory;
using Trivale.UI.Components;
using Trivale.OS.MainMenu.Processes;
using Trivale.Memory.ProcessManagement;

namespace Trivale.OS;

/// <summary>
/// Main menu system that manages processes, memory slots, and scene loading.
/// 
/// Process/Scene Lifecycle:
/// 1. Button Press -> Creates process -> Loads into memory slot -> Loads scene
/// 2. Scene signals unload -> Process unloaded -> Memory slot cleared -> UI restored
/// 
/// System Responsibilities:
/// - Process Management: Creating, loading, and unloading processes
/// - Memory Management: Managing the memory slot state and resources
/// - Scene Management: Loading scenes and handling their unload requests
/// - UI State: Maintaining menu state and MEM slot visualization
/// 
/// Loaded Scene Contracts:
/// - Scenes must implement SceneUnloadRequested signal
/// - Scenes should not manage their own unloading
/// - Scenes can expect proper cleanup when signaling unload
/// 
/// Memory/Process Management:
/// - All process and memory management is centralized here
/// - Processes are created/destroyed with their corresponding scenes
/// - Memory slot state is maintained and visualized
/// </summary>
public partial class SimpleMainMenu : Control
{
	private SlotGridSystem _slotSystem;
	private Button _cardGameButton;
	private Button _debugButton;
	private Control _viewportContainer;
	private Control _mainContent;
	
	// Process management
	private IProcess _currentProcess;
	
	private const string CardGameScenePath = "res://Scenes/MainMenu/CardGameScene.tscn";
	private const string DebugScenePath = "res://Scenes/MainMenu/DebugScene.tscn";

	public override void _Ready()
	{
		CustomMinimumSize = new Vector2(800, 600);
		SetLayout();
		ConnectSignals();
	}

	private void SetLayout()
	{
		// Set up the root Control node to fill the window
		LayoutMode = 1;  // Use anchors
		AnchorsPreset = (int)LayoutPreset.FullRect;
		GrowHorizontal = GrowDirection.Both;
		GrowVertical = GrowDirection.Both;

		// Main container with margins
		var marginContainer = new MarginContainer();
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
			GrowVertical = GrowDirection.Both
		};
		marginContainer.AddChild(mainContainer);

		// Left panel (Slot Grid) with panel background
		var leftPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(200, 0),
			SizeFlagsVertical = SizeFlags.Fill
		};
		mainContainer.AddChild(leftPanel);

		var leftContent = new VBoxContainer();
		leftPanel.AddChild(leftContent);

		// MEM header
		var memHeader = new Label { Text = "MEM" };
		leftContent.AddChild(memHeader);

		// Slot grid system
		_slotSystem = new SlotGridSystem();
		leftContent.AddChild(_slotSystem);

		// Slot grid display
		var slotDisplay = new SlotGridDisplay();
		slotDisplay.Initialize(_slotSystem);  // Pass the reference directly
		leftContent.AddChild(slotDisplay);

		// Center panel (main content)
		var centerPanel = new PanelContainer
		{
			SizeFlagsHorizontal = SizeFlags.Expand,
			SizeFlagsVertical = SizeFlags.Fill
		};
		mainContainer.AddChild(centerPanel);

		_mainContent = new MarginContainer();
		_mainContent.AddThemeConstantOverride("margin_left", 10);
		_mainContent.AddThemeConstantOverride("margin_right", 10);
		_mainContent.AddThemeConstantOverride("margin_top", 10);
		_mainContent.AddThemeConstantOverride("margin_bottom", 10);
		centerPanel.AddChild(_mainContent);

		SetupMainMenuButtons();
		SetupViewportContainer();
	}

	private void SetupMainMenuButtons()
	{
		var menuContainer = new CenterContainer
		{
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill
		};
		_mainContent.AddChild(menuContainer);

		var buttonContainer = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(300, 0)
		};
		menuContainer.AddChild(buttonContainer);

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
		// Set slot 0 state
		_slotSystem.SetSlotState(0, true, displayName);

		// Create and load new process
		IProcess newProcess = displayName switch
		{
			"CARD GAME" => new CardGameMenuProcess($"menu_cardgame_{System.DateTime.Now.Ticks}"),
			"DEBUG" => new DebugMenuProcess($"menu_debug_{System.DateTime.Now.Ticks}"),
			_ => null
		};

		if (newProcess != null)
		{
			_currentProcess = newProcess;
			LoadSceneInViewport(scenePath);
			GD.Print($"Loaded process {newProcess.Id} of type {newProcess.Type}");
		}
		else
		{
			GD.PrintErr($"Failed to create process for {displayName}");
			_slotSystem.SetSlotState(0, false, "");
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
		_currentProcess = null;
		_slotSystem.SetSlotState(0, false, "");

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
}
