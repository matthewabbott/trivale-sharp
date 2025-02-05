// src/Terminal/TerminalWindow.cs

using Godot;
using System;

namespace Trivale.Terminal;

public enum WindowStyle
{
    Normal,
    Alert,
    Secure,
    Corrupted,
    Debug
}

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
    public Color BorderColor { get; set; } = new Color(0, 1, 0); // Default phosphor green
    
    [Export]
    public WindowStyle Style { get; set; } = WindowStyle.Normal;
    
    [Export]
    public Vector2 MinSize { get; set; } = new Vector2(400, 300);
    
    public override void _Ready()
    {
        GD.Print($"Setting up window: {WindowTitle}");
        
        CustomMinimumSize = MinSize;
        Size = MinSize;
        
        // Add main container
        var layout = new VBoxContainer
        {
            Name = "VBoxContainer",
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect
        };
        AddChild(layout);
        
        // Setup window style
        ApplyStyle(Style);
        
        GD.Print($"Window setup complete for {WindowTitle} at position {GlobalPosition}");
    }
    
    private void ApplyStyle(WindowStyle style)
    {
        // Create title bar
        _titleBar = new Panel
        {
            Name = "TitleBar",
            CustomMinimumSize = new Vector2(0, 30)
        };
        GetNode<VBoxContainer>("VBoxContainer").AddChild(_titleBar);
        
        // Create content panel
        _contentPanel = new Panel
        {
            Name = "ContentPanel",
            SizeFlagsVertical = SizeFlags.Fill
        };
        GetNode<VBoxContainer>("VBoxContainer").AddChild(_contentPanel);
        
        // Title text
        _titleLabel = new Label
        {
            Name = "TitleLabel",
            Text = WindowTitle,
            Position = new Vector2(10, 0),
            SizeFlagsVertical = SizeFlags.ShrinkCenter
        };
        _titleBar.AddChild(_titleLabel);
        
        var (bgColor, borderColor, titleBgColor) = GetColorsForStyle(style);
        
        // Apply title bar style
        var titleStylebox = new StyleBoxFlat
        {
            BgColor = titleBgColor,
            BorderColor = borderColor,
            BorderWidthBottom = style == WindowStyle.Debug ? 2 : 1,
            BorderWidthLeft = 0,
            BorderWidthRight = 0,
            BorderWidthTop = 0
        };
        _titleBar.AddThemeStyleboxOverride("panel", titleStylebox);
        
        // Apply content panel style
        var contentStylebox = new StyleBoxFlat
        {
            BgColor = bgColor,
            BorderColor = borderColor,
            BorderWidthBottom = style == WindowStyle.Debug ? 2 : 1,
            BorderWidthLeft = style == WindowStyle.Debug ? 2 : 1,
            BorderWidthRight = style == WindowStyle.Debug ? 2 : 1,
            BorderWidthTop = 0,
            ContentMarginLeft = 10,
            ContentMarginRight = 10,
            ContentMarginTop = 10,
            ContentMarginBottom = 10
        };
        _contentPanel.AddThemeStyleboxOverride("panel", contentStylebox);
        
        // Set text color
        _titleLabel.Modulate = borderColor;
        
        if (style == WindowStyle.Debug)
        {
            var debugLabel = new Label
            {
                Text = "DEBUG: " + WindowTitle,
                Modulate = borderColor
            };
            _contentPanel.AddChild(debugLabel);
        }
    }
    
    private (Color bg, Color border, Color titleBg) GetColorsForStyle(WindowStyle style)
    {
        return style switch
        {
            WindowStyle.Normal => (
                new Color(0, 0.05f, 0, 0.9f),    // Dark green bg
                new Color(0, 1, 0, 1),           // Bright green border
                new Color(0, 0.1f, 0, 0.95f)     // Slightly lighter title bg
            ),
            WindowStyle.Alert => (
                new Color(0.2f, 0, 0, 0.9f),     // Dark red bg
                new Color(1, 0, 0, 1),           // Bright red border
                new Color(0.3f, 0, 0, 0.95f)     // Slightly lighter title bg
            ),
            WindowStyle.Secure => (
                new Color(0, 0, 0.05f, 0.9f),    // Dark blue bg
                new Color(0, 0.5f, 1, 1),        // Bright blue border
                new Color(0, 0, 0.1f, 0.95f)     // Slightly lighter title bg
            ),
            WindowStyle.Corrupted => (
                new Color(0.1f, 0, 0.1f, 0.9f),  // Dark purple bg
                new Color(1, 0, 1, 1),           // Bright purple border
                new Color(0.15f, 0, 0.15f, 0.95f)// Slightly lighter title bg
            ),
            WindowStyle.Debug => (
                new Color(1, 0, 0, 0.5f),        // Semi-transparent red bg
                new Color(1, 1, 0, 1),           // Yellow border
                new Color(0, 0, 1, 1)            // Blue title bg
            ),
            _ => (
                new Color(0, 0.05f, 0, 0.9f),
                new Color(0, 1, 0, 1),
                new Color(0, 0.1f, 0, 0.95f)
            )
        };
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
        }
    }
    
    protected void AddContent(Control content)
    {
        _contentPanel.AddChild(content);
        GD.Print($"Added content to {WindowTitle}");
    }
}