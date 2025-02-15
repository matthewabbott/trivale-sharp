// src/OS/MainMenu/SimpleMainMenu.cs
using Godot;
using System;

namespace Trivale.OS;

public partial class SimpleMainMenu : Control
{
	private Label _memSlotDisplay;
	private Button _cardGameButton;
	private Button _debugButton;
	private Control _viewportContainer;
	private Panel _resourcePanel;
	private Control _mainContent;
	
	private const string CardGameScenePath = "res://Scenes/MainMenu/CardGameScene.tscn";
	private const string DebugScenePath = "res://Scenes/MainMenu/DebugScene.tscn";

	public override void _Ready()
	{
		// Set up the root Control node (this node) to fill the window
		LayoutMode = 1;  // Important: Use anchors
		AnchorsPreset = (int)LayoutPreset.FullRect;
		GrowHorizontal = GrowDirection.Both;
		GrowVertical = GrowDirection.Both;

		SetupLayout();
		ConnectSignals();
		UpdateMemSlotUI(true, "");
	}

	private void SetupLayout()
	{
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
			Theme = new Theme() // We'll customize this later
		};
		marginContainer.AddChild(mainContainer);

		// Left panel (MEM slots) with panel background
		var leftPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(200, 0),
			SizeFlagsVertical = SizeFlags.Fill
		};
		var leftStyle = new StyleBoxFlat
		{
			BgColor = new Color(0, 0.05f, 0, 0.9f),  // Very dark green
			BorderColor = new Color(0, 1, 0),         // Bright green
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1
		};
		leftPanel.AddThemeStyleboxOverride("panel", leftStyle);
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

		// MEM slot display
		_memSlotDisplay = new Label 
		{ 
			Text = "└── □ [          ]",
			CustomMinimumSize = new Vector2(0, 100),
			Theme = new Theme() // We'll set custom font here later
		};
		leftContent.AddChild(_memSlotDisplay);

		// Center panel (main content) with panel background
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

		// Menu buttons container
		var buttonContainer = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(300, 0)
		};
		buttonContainer.AddThemeConstantOverride("separation", 10);
		_mainContent.AddChild(buttonContainer);

		// Buttons
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

		// Viewport container for loaded scenes
		_viewportContainer = new PanelContainer
		{
			Visible = false, // Hidden until scene is loaded
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill
		};
		_mainContent.AddChild(_viewportContainer);

		// Right panel (resources)
		var rightPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(150, 0),
			SizeFlagsVertical = SizeFlags.Fill
		};
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

	private void ConnectSignals()
	{
		_cardGameButton.Pressed += () => OnSceneButtonPressed(CardGameScenePath, "CARD GAME");
		_debugButton.Pressed += () => OnSceneButtonPressed(DebugScenePath, "DEBUG");
	}

	private void OnSceneButtonPressed(string scenePath, string displayName)
	{
		UpdateMemSlotUI(false, displayName);
		LoadSceneInViewport(scenePath);
	}

	private void UpdateMemSlotUI(bool isEmpty, string loadedText)
	{
		if (isEmpty)
		{
			_memSlotDisplay.Text = "└── □ [          ]";
		}
		else
		{
			_memSlotDisplay.Text = 
				$"└── ■ [LOADED: {loadedText}]\n" +
				"    ├── □ [          ]\n" +
				"    ├── □ [          ]\n" +
				"    └── □ [          ]";
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
			_viewportContainer.AddChild(instance);
			_viewportContainer.Visible = true;
		}
		else
		{
			GD.PrintErr($"Failed to load scene: {scenePath}");
		}
	}
}
