// src/Terminal/TerminalWindow.cs

using Godot;
using System;

namespace Trivale.Terminal;

public partial class TerminalWindow : Control
{
    protected Panel _titleBar;
    protected Label _titleLabel;
    protected Panel _contentPanel;
    protected bool _isDragging = false;
    protected Vector2 _dragStart;
    
    [Export]
    public string WindowTitle { get; set; } = "Terminal";
    
    [Export]
    public Color BorderColor { get; set; } = new Color(0, 1, 0); // Phosphor green
    
    [Export]
    public Vector2 MinSize { get; set; } = new Vector2(200, 150);
    
    public override void _Ready()
    {
        GD.Print($"Setting up window: {WindowTitle}");
        
        // DEBUG: Force a specific size
        CustomMinimumSize = new Vector2(400, 300);
        Size = new Vector2(400, 300);
        
        // DEBUG: Add a background panel for the entire window
        var background = new ColorRect
        {
            Color = new Color(1, 0, 0, 0.5f), // Semi-transparent red
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect
        };
        AddChild(background);
        
        // Layout container
        var layout = new VBoxContainer
        {
            Name = "VBoxContainer",
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect
        };
        AddChild(layout);
        
        // Title bar
        _titleBar = new Panel
        {
            Name = "TitleBar",
            CustomMinimumSize = new Vector2(0, 30)
        };
        
        // DEBUG: Make title bar very visible
        var titleStylebox = new StyleBoxFlat
        {
            BgColor = new Color(0, 0, 1, 1), // Bright blue
            BorderColor = new Color(1, 1, 0, 1), // Yellow
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2
        };
        _titleBar.AddThemeStyleboxOverride("panel", titleStylebox);
        layout.AddChild(_titleBar);
        
        // Title text
        _titleLabel = new Label
        {
            Name = "TitleLabel",
            Text = WindowTitle,
            Position = new Vector2(10, 0),
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            Modulate = new Color(1, 1, 1, 1) // White text
        };
        _titleBar.AddChild(_titleLabel);
        
        // Content panel
        _contentPanel = new Panel
        {
            Name = "ContentPanel",
            SizeFlagsVertical = SizeFlags.Fill
        };
        
        // DEBUG: Make content panel very visible
        var contentStylebox = new StyleBoxFlat
        {
            BgColor = new Color(0, 1, 0, 0.5f), // Semi-transparent green
            BorderColor = new Color(1, 1, 0, 1), // Yellow
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2
        };
        _contentPanel.AddThemeStyleboxOverride("panel", contentStylebox);
        layout.AddChild(_contentPanel);
        
        // DEBUG: Add a label to content panel
        var debugLabel = new Label
        {
            Text = "DEBUG: " + WindowTitle,
            Modulate = new Color(1, 1, 1, 1)
        };
        _contentPanel.AddChild(debugLabel);
        
        GD.Print($"Window setup complete for {WindowTitle} at position {GlobalPosition}");
    }
    
    public override void _Process(double delta)
    {
        // DEBUG: Print position every few seconds
        if (Time.GetTicksMsec() % 3000 < 16)
        {
            GD.Print($"Window {WindowTitle} at position {GlobalPosition}, size {Size}, visible: {IsVisibleInTree()}");
        }
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed && _titleBar.GetRect().HasPoint(mouseButton.Position))
                {
                    _isDragging = true;
                    _dragStart = mouseButton.GlobalPosition - GlobalPosition;
                    GD.Print($"Started dragging {WindowTitle}");
                }
                else
                {
                    _isDragging = false;
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && _isDragging)
        {
            GlobalPosition = mouseMotion.GlobalPosition - _dragStart;
            GD.Print($"Dragged {WindowTitle} to {GlobalPosition}");
        }
    }
    
    protected void AddContent(Control content)
    {
        _contentPanel.AddChild(content);
        GD.Print($"Added content to {WindowTitle}");
    }
}