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
        SetupWindow();
        CustomMinimumSize = MinSize;
    }
    
    private void SetupWindow()
    {
        // Title bar with darker background
        _titleBar = new Panel();
        _titleBar.CustomMinimumSize = new Vector2(0, 30); // Slightly taller
        var titleStylebox = new StyleBoxFlat();
        titleStylebox.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        titleStylebox.BorderColor = BorderColor;
        titleStylebox.BorderWidthBottom = 1;
        _titleBar.AddThemeStyleboxOverride("panel", titleStylebox);
        AddChild(_titleBar);
        
        // Title text with padding
        _titleLabel = new Label
        {
            Text = WindowTitle,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Position = new Vector2(10, 0), // Add left padding
            Modulate = BorderColor
        };
        _titleBar.AddChild(_titleLabel);
        
        // Content panel with padding
        _contentPanel = new Panel();
        var stylebox = new StyleBoxFlat();
        stylebox.BorderColor = BorderColor;
        stylebox.BorderWidthBottom = stylebox.BorderWidthLeft = 
        stylebox.BorderWidthRight = stylebox.BorderWidthTop = 1;
        stylebox.BgColor = new Color(0, 0, 0, 0.9f);
        stylebox.ContentMarginLeft = stylebox.ContentMarginRight = 
        stylebox.ContentMarginTop = stylebox.ContentMarginBottom = 10;
        _contentPanel.AddThemeStyleboxOverride("panel", stylebox);
        
        // Layout
        var layout = new VBoxContainer();
        layout.AddChild(_titleBar);
        layout.AddChild(_contentPanel);
        AddChild(layout);
        
        // CRT shader (to be implemented)
        SetupShader();
    }
    
    private void SetupShader()
    {
        // TODO: Implement CRT shader
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
    
    // Method to add content to the window
    protected void AddContent(Control content)
    {
        _contentPanel.AddChild(content);
    }
}