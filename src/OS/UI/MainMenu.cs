// src/OS/UI/MainMenu.cs
using Godot;
using System;
using Trivale.UI.Components;

namespace Trivale.OS.UI;

public partial class MainMenu : Control
{
    [Signal]
    public delegate void OptionSelectedEventHandler(string option);
    
    private VBoxContainer _mainContainer;
    private TerminalButton _playButton;
    private TerminalButton _debugButton;
    
    public override void _Ready()
    {
        GD.Print("[MainMenu] _Ready called");
        SetupLayout();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouse mouseEvent)
        {
            GD.Print($"[MainMenu] _GuiInput: {mouseEvent.GetType()} at {mouseEvent.Position}");
        }
        else
        {
            GD.Print($"[MainMenu] _GuiInput: {@event}");
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouse mouseEvent)
        {
            GD.Print($"[MainMenu] _UnhandledInput: {mouseEvent.GetType()} at {mouseEvent.Position}");
        }
        else
        {
            GD.Print($"[MainMenu] _UnhandledInput: {@event}");
        }
    }
    
    private void SetupLayout()
    {
        // Center container for the menu
        var centerContainer = new CenterContainer
        {
            AnchorsPreset = (int)LayoutPreset.FullRect
        };
        AddChild(centerContainer);
        
        // Main container for buttons
        _mainContainer = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(400, 300)
        };
        centerContainer.AddChild(_mainContainer);
        
        // Title
        var titleLabel = new Label
        {
            Text = "NETRUNNER OS v1.0",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _mainContainer.AddChild(titleLabel);
        
        // Add some spacing
        _mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 40) });
        
        // Play button
        _playButton = new TerminalButton
        {
            IconText = AsciiStyle.CreateBox(20, 6, "PLAY")[0],
            ButtonText = "CARD GAME"
        };
        _playButton.Pressed += () => EmitSignal(SignalName.OptionSelected, "CardGame");
        _mainContainer.AddChild(_playButton);
        
        // Debug button
        _debugButton = new TerminalButton
        {
            IconText = AsciiStyle.CreateBox(20, 6, "DEBUG")[0],
            ButtonText = "TEST/DEBUG"
        };
        _debugButton.Pressed += () => EmitSignal(SignalName.OptionSelected, "Debug");
        _mainContainer.AddChild(_debugButton);
    }
}