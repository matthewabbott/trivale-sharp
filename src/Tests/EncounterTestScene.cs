// src/Tests/EncounterTestScene.cs

using Godot;
using System;
using Trivale.Encounters;
using Trivale.OS;

namespace Trivale.Tests;

public partial class EncounterTestScene : Node
{
    private SystemDesktop _desktop;
    private WindowManager _windowManager;
    private EncounterManager _encounterManager;
    private Button _createGameButton;
    private SpinBox _playerCountSpinner;
    private SpinBox _handSizeSpinner;
    
    public override void _Ready()
    {
        _desktop = GetParent<SystemDesktop>();
        if (_desktop == null)
        {
            GD.PrintErr("EncounterTestScene must be a child of SystemDesktop");
            return;
        }
        
        _windowManager = _desktop.GetNode<WindowManager>("WindowLayer");
        if (_windowManager == null)
        {
            GD.PrintErr("Could not find WindowManager");
            return;
        }
        
        // Create and add encounter manager
        _encounterManager = new EncounterManager();
        AddChild(_encounterManager);
        
        SetupControls();
    }
    
    private void SetupControls()
    {
        var controlPanel = new Control
        {
            Position = new Vector2(20, 20)
        };
        AddChild(controlPanel);
        
        var container = new VBoxContainer();
        controlPanel.AddChild(container);
        
        // Player count control
        var playerCountContainer = new HBoxContainer();
        container.AddChild(playerCountContainer);
        
        playerCountContainer.AddChild(new Label { Text = "Players: " });
        _playerCountSpinner = new SpinBox
        {
            MinValue = 2,
            MaxValue = 8,
            Value = 4,
            CustomMinimumSize = new Vector2(100, 0)
        };
        playerCountContainer.AddChild(_playerCountSpinner);
        
        // Hand size control
        var handSizeContainer = new HBoxContainer();
        container.AddChild(handSizeContainer);
        
        handSizeContainer.AddChild(new Label { Text = "Hand Size: " });
        _handSizeSpinner = new SpinBox
        {
            MinValue = 1,
            MaxValue = 13,
            Value = 5,
            CustomMinimumSize = new Vector2(100, 0)
        };
        handSizeContainer.AddChild(_handSizeSpinner);
        
        // Add some spacing
        container.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
        
        // Create game button
        _createGameButton = new Button
        {
            Text = "Start New Game",
            CustomMinimumSize = new Vector2(150, 30)
        };
        _createGameButton.Pressed += CreateNewGame;
        container.AddChild(_createGameButton);
    }
    
    private void CreateNewGame()
    {
        var config = new GameConfiguration
        {
            NumPlayers = (int)_playerCountSpinner.Value,
            HandSize = (int)_handSizeSpinner.Value
        };
        
        var encounterName = $"puzzle_{DateTime.Now.Ticks}";
        var encounter = new PuzzleCardEncounter(encounterName, config);
        
        if (_encounterManager.StartEncounter(encounter))
        {
            GD.Print($"Started encounter: {encounterName}");
            
            // Get the windows from the encounter and add them to the window manager
            var playerWindow = (encounter as PuzzleCardEncounter)?.GetWindow("hand");
            var tableWindow = (encounter as PuzzleCardEncounter)?.GetWindow("table");
            
            if (playerWindow != null)
            {
                _windowManager.AddWindow(playerWindow);
            }
            
            if (tableWindow != null)
            {
                _windowManager.AddWindow(tableWindow);
            }
        }
        else
        {
            GD.PrintErr($"Failed to start encounter: {encounterName}");
        }
    }
}