// src/Tests/EncounterTestScene.cs
using Godot;
using System;
using Trivale.Encounters;
using Trivale.Game;
using Trivale.Memory;

namespace Trivale.Tests;

public partial class EncounterTestScene : Node
{
    private ProcessManager _processManager;
    private Button _createGameButton;
    private SpinBox _playerCountSpinner;
    private SpinBox _handSizeSpinner;
    
    public override void _Ready()
    {
        GD.Print("EncounterTestScene._Ready called");
        
        // Create and add process manager
        _processManager = new ProcessManager();
        AddChild(_processManager);
        GD.Print("Created ProcessManager");
        
        SetupControls();
        GD.Print("Setup complete");
    }
    
    private void SetupControls()
    {
        var controlPanel = new Control
        {
            Position = new Vector2(200, 20),  // Moved to avoid overlap with other UI
            CustomMinimumSize = new Vector2(200, 150)
        };
        AddChild(controlPanel);
        GD.Print("Added control panel");
        
        var container = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(200, 150)
        };
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
        
        GD.Print("Controls set up - button should be visible");
    }
    
    private void CreateNewGame()
    {
        GD.Print("CreateNewGame called");
        
        var config = new GameConfiguration
        {
            NumPlayers = (int)_playerCountSpinner.Value,
            HandSize = (int)_handSizeSpinner.Value
        };
        
        var processName = $"game_{DateTime.Now.Ticks}";
        var cardGame = new CardGameEncounter(processName, config);
        
        GD.Print($"Created process: {processName}");
        
        if (_processManager.StartProcess(cardGame))
        {
            GD.Print($"Successfully started process: {processName}");
        }
        else
        {
            GD.PrintErr($"Failed to start process: {processName}");
        }
    }
}