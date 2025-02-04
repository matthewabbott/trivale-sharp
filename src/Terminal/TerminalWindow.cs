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
        // Title bar
        _titleBar = new Panel();
        _titleBar.CustomMinimumSize = new Vector2(0, 25);
        _titleBar.MouseEntered += () => MouseDefaultCursorShape = CursorShape.Move;
        _titleBar.MouseExited += () => MouseDefaultCursorShape = CursorShape.Arrow;
        AddChild(_titleBar);
        
        // Title text
        _titleLabel = new Label
        {
            Text = WindowTitle,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        _titleBar.AddChild(_titleLabel);
        
        // Content panel
        _contentPanel = new Panel();
        AddChild(_contentPanel);
        
        // Set up styles
        var stylebox = new StyleBoxFlat();
        stylebox.BorderColor = BorderColor;
        stylebox.BorderWidthBottom = stylebox.BorderWidthLeft = 
        stylebox.BorderWidthRight = stylebox.BorderWidthTop = 1;
        stylebox.BgColor = new Color(0, 0, 0, 0.9f);
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