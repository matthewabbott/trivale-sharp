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
        SetupLayout();
        ConnectSignals();
        UpdateMemSlotUI(true, "");
    }

    private void SetupLayout()
    {
        // Main horizontal container
        var mainContainer = new HBoxContainer
        {
            AnchorsPreset = (int)LayoutPreset.FullRect
        };
        AddChild(mainContainer);

        // Left panel (MEM slots)
        var leftPanel = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(200, 0)
        };
        mainContainer.AddChild(leftPanel);

        // MEM header
        var memHeader = new Label { Text = "MEM" };
        leftPanel.AddChild(memHeader);

        // MEM slot display
        _memSlotDisplay = new Label 
        { 
            Text = "└── □ [          ]",
            Theme = new Theme() // We'll set custom font here later
        };
        leftPanel.AddChild(_memSlotDisplay);

        // Center panel (main content)
        _mainContent = new Control
        {
            SizeFlagsHorizontal = SizeFlags.Expand
        };
        mainContainer.AddChild(_mainContent);

        // Menu buttons container
        var buttonContainer = new VBoxContainer
        {
            AnchorsPreset = (int)LayoutPreset.Center
        };
        _mainContent.AddChild(buttonContainer);

        // Buttons
        _cardGameButton = new Button { Text = "CARD GAME PLACEHOLDER" };
        buttonContainer.AddChild(_cardGameButton);

        _debugButton = new Button { Text = "DEBUG SANDBOX" };
        buttonContainer.AddChild(_debugButton);

        // Viewport container for loaded scenes
        _viewportContainer = new Control
        {
            AnchorsPreset = (int)LayoutPreset.FullRect,
            Visible = false // Hidden until scene is loaded
        };
        _mainContent.AddChild(_viewportContainer);

        // Right panel (resources)
        _resourcePanel = new Panel
        {
            CustomMinimumSize = new Vector2(150, 0)
        };
        var resourceLabel = new Label { Text = "Resources:\nMEM\nHealth\netc." };
        _resourcePanel.AddChild(resourceLabel);
        mainContainer.AddChild(_resourcePanel);
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