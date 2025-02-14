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
        // Make the control fill its container
        AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        
        SetupLayout();
    }
    
    private void SetupLayout()
    {
        // Center container for the menu
        var centerContainer = new CenterContainer
        {
            AnchorsPreset = (int)Control.LayoutPreset.FullRect,
            SizeFlagsHorizontal = SizeFlags.Fill,
            SizeFlagsVertical = SizeFlags.Fill
        };
        AddChild(centerContainer);
        
        // Main container for menu items
        _mainContainer = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(500, 0),  // Wide enough for content
        };
        centerContainer.AddChild(_mainContainer);
        
        // Title
        var titleLabel = new Label
        {
            Text = "NETRUNNER OS v1.0",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 60)
        };
        _mainContainer.AddChild(titleLabel);
        
        // More spacing at top
        _mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 40) });
        
        // Play button
        _playButton = new TerminalButton
        {
            IconText = AsciiStyle.CreateBox(20, 6, "PLAY")[0],
            ButtonText = "CARD GAME",
            CustomMinimumSize = new Vector2(200, 80)  // More reasonable size
        };
        _playButton.Pressed += () => EmitSignal(SignalName.OptionSelected, "CardGame");
        _mainContainer.AddChild(_playButton);
        
        // Good spacing between buttons
        _mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 30) });
        
        // Debug button
        _debugButton = new TerminalButton
        {
            IconText = AsciiStyle.CreateBox(20, 6, "DEBUG")[0],
            ButtonText = "TEST/DEBUG",
            CustomMinimumSize = new Vector2(0, 120)  // Taller buttons
        };
        _debugButton.Pressed += () => EmitSignal(SignalName.OptionSelected, "Debug");
        _mainContainer.AddChild(_debugButton);
    }
}