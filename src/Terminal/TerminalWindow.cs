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
    public Color BorderColor { get; set; } = new Color(0, 1, 0);
    
    [Export]
    public WindowStyle Style { get; set; } = WindowStyle.Normal;
    
    [Export]
    public Vector2 MinSize { get; set; } = new Vector2(400, 300);
    
    public override void _Ready()
    {
        GD.Print($"Setting up window: {WindowTitle}");
        
        // Set up Control node properties
        CustomMinimumSize = MinSize;
        Size = MinSize;
        MouseFilter = MouseFilterEnum.Stop;  // Make sure we get mouse events
        
        // Add main container
        var layout = new VBoxContainer
        {
            Name = "VBoxContainer",
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            SizeFlagsHorizontal = SizeFlags.Fill,
            SizeFlagsVertical = SizeFlags.Fill,
            MouseFilter = MouseFilterEnum.Pass  // Let events pass through to children
        };
        AddChild(layout);
        
        // Add background
        var background = new ColorRect
        {
            Name = "Background",
            ZIndex = -1,
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            Color = new Color(0, 0, 0, 0.9f),
            MouseFilter = MouseFilterEnum.Ignore  // Ignore mouse events
        };
        AddChild(background);
        
        // Setup window style
        ApplyStyle(Style);
        
        GD.Print($"Window setup complete for {WindowTitle} at position {GlobalPosition}");
        GD.Print("Mouse event handlers initialized");
    }
    
    private void ApplyStyle(WindowStyle style)
    {
        var layout = GetNode<VBoxContainer>("VBoxContainer");
        
        // Create title bar
        _titleBar = new Panel
        {
            Name = "TitleBar",
            CustomMinimumSize = new Vector2(0, 30),
            MouseFilter = MouseFilterEnum.Stop  // Make sure title bar gets mouse events
        };
        layout.AddChild(_titleBar);
        
        // Create content panel
        _contentPanel = new Panel
        {
            Name = "ContentPanel",
            SizeFlagsVertical = SizeFlags.Fill,
            MouseFilter = MouseFilterEnum.Pass  // Let events through to content
        };
        layout.AddChild(_contentPanel);
        
        // Title text
        _titleLabel = new Label
        {
            Name = "TitleLabel",
            Text = WindowTitle,
            Position = new Vector2(10, 0),
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            MouseFilter = MouseFilterEnum.Ignore  // Ignore mouse events on label
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
        
        _titleLabel.Modulate = borderColor;
        
        // Add input event handlers for the title bar
        _titleBar.GuiInput += OnTitleBarInput;
        
        GD.Print($"Style applied to {WindowTitle}, title bar input handler connected");
    }
    
    private void OnTitleBarInput(InputEvent @event)
    {
        GD.Print($"Title bar input on {WindowTitle}: {@event}");
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    _isDragging = true;
                    _dragStart = mouseButton.GlobalPosition - GlobalPosition;
                    GD.Print($"Started dragging {WindowTitle} from {_dragStart}");
                }
                else
                {
                    _isDragging = false;
                    GD.Print($"Stopped dragging {WindowTitle}");
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && _isDragging)
        {
            var newPos = mouseMotion.GlobalPosition - _dragStart;
            GlobalPosition = newPos;
            GD.Print($"Dragging {WindowTitle} to {newPos}");
        }
    }
    
    private (Color bg, Color border, Color titleBg) GetColorsForStyle(WindowStyle style)
    {
        // ... (same as before)
        return style switch
        {
            WindowStyle.Normal => (
                new Color(0, 0.05f, 0, 0.9f),
                new Color(0, 1, 0, 1),
                new Color(0, 0.1f, 0, 0.95f)
            ),
            WindowStyle.Alert => (
                new Color(0.2f, 0, 0, 0.9f),
                new Color(1, 0, 0, 1),
                new Color(0.3f, 0, 0, 0.95f)
            ),
            WindowStyle.Secure => (
                new Color(0, 0, 0.05f, 0.9f),
                new Color(0, 0.5f, 1, 1),
                new Color(0, 0, 0.1f, 0.95f)
            ),
            WindowStyle.Corrupted => (
                new Color(0.1f, 0, 0.1f, 0.9f),
                new Color(1, 0, 1, 1),
                new Color(0.15f, 0, 0.15f, 0.95f)
            ),
            WindowStyle.Debug => (
                new Color(1, 0, 0, 0.5f),
                new Color(1, 1, 0, 1),
                new Color(0, 0, 1, 1)
            ),
            _ => (
                new Color(0, 0.05f, 0, 0.9f),
                new Color(0, 1, 0, 1),
                new Color(0, 0.1f, 0, 0.95f)
            )
        };
    }
    
    protected void AddContent(Control content)
    {
        _contentPanel.AddChild(content);
        GD.Print($"Added content to {WindowTitle}");
    }
}