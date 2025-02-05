// src/OS/WindowManager.cs

using Godot;
using System.Collections.Generic;
using Trivale.Terminal;

namespace Trivale.OS;

public partial class WindowManager : Control
{
    private List<TerminalWindow> _windows = new();
    private TerminalWindow _focusedWindow;
    private ColorRect _debugBounds;
    
    public override void _Ready()
    {
        GD.Print("WindowManager._Ready called");
        
        // Make sure we're at the top layer and can receive input
        MouseFilter = MouseFilterEnum.Pass;
        ZIndex = 100;
        // These are crucial for making sure we're on top
        Position = Vector2.Zero;
        AnchorRight = 1;
        AnchorBottom = 1;
        
        // Set up layout
        SizeFlagsHorizontal = SizeFlags.Fill;
        SizeFlagsVertical = SizeFlags.Fill;
        
        // Make this a top-level node so it draws above everything else
        TopLevel = true;
        
        // DEBUG: Add a visible rectangle to show the bounds
        _debugBounds = new ColorRect
        {
            Name = "DebugBounds",
            Color = new Color(1, 0, 1, 0.2f),
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            MouseFilter = MouseFilterEnum.Ignore,
            ZIndex = -1
        };
        AddChild(_debugBounds);
        
        GD.Print($"WindowManager initialized at position {Position}, size {Size}, mouse_filter: {MouseFilter}");
    }
    
    public void AddWindow(TerminalWindow window)
    {
        _windows.Add(window);
        AddChild(window);
        
        window.MouseFilter = MouseFilterEnum.Stop;
        window.ZIndex = _windows.Count * 10;
        window.GuiInput += (@event) => HandleWindowInput(window, @event);
        
        FocusWindow(window);
    }
    
    private void HandleWindowInput(TerminalWindow window, InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && 
            mouseEvent.ButtonIndex == MouseButton.Left && 
            mouseEvent.Pressed)
        {
            GD.Print($"Window clicked: {window.WindowTitle}");
            FocusWindow(window);
        }
    }
    
    private void FocusWindow(TerminalWindow window)
    {
        if (_focusedWindow == window) return;
        
        _focusedWindow = window;
        
        // Update z-indices for all windows
        int baseZ = 0;
        foreach (var w in _windows)
        {
            w.ZIndex = baseZ;
            baseZ += 10;
        }
        
        // Move focused window to top and set highest z-index
        MoveChild(window, GetChildCount() - 1);
        window.ZIndex = baseZ;
        
        // Update window appearance
        foreach (var w in _windows)
        {
            bool isFocused = w == window;
            var titleBar = w.GetNode<Panel>("VBoxContainer/TitleBar");
            if (titleBar != null)
            {
                var styleBox = new StyleBoxFlat
                {
                    BgColor = isFocused ? 
                        new Color(0.2f, 0.2f, 0.2f, 0.95f) : 
                        new Color(0.1f, 0.1f, 0.1f, 0.95f),
                    BorderColor = w.BorderColor,
                    BorderWidthBottom = 1
                };
                titleBar.AddThemeStyleboxOverride("panel", styleBox);
            }
        }
    }
    
    public void SetDebugBoundsVisible(bool visible)
    {
        if (_debugBounds != null)
        {
            _debugBounds.Visible = visible;
        }
    }
    
    public void RemoveWindow(TerminalWindow window)
    {
        GD.Print($"Removing window: {window.WindowTitle}");
        _windows.Remove(window);
        window.QueueFree();
        
        if (_focusedWindow == window)
        {
            _focusedWindow = _windows.Count > 0 ? _windows[^1] : null;
        }
    }
}