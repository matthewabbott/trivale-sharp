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
        MouseFilter = MouseFilterEnum.Pass;
        
        // Make sure we're visible and at the right Z-index
        ZIndex = 100;
        ZAsRelative = false;
        
        // DEBUG: Add a visible rectangle to show the bounds
        _debugBounds = new ColorRect
        {
            Name = "DebugBounds",
            Color = new Color(1, 0, 1, 0.2f), // Semi-transparent magenta
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            ZIndex = -1  // Put it behind windows
        };
        AddChild(_debugBounds);
    }
    
    public void SetDebugBoundsVisible(bool visible)
    {
        if (_debugBounds != null)
        {
            _debugBounds.Visible = visible;
        }
    }
    
    public void AddWindow(TerminalWindow window)
    {
        GD.Print($"Adding window: {window.WindowTitle}");
        
        _windows.Add(window);
        AddChild(window);
        
        // Connect window input
        window.GuiInput += (@event) => HandleWindowInput(window, @event);
        
        // Focus the new window
        FocusWindow(window);
        
        GD.Print($"Window count: {_windows.Count}");
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
        
        GD.Print($"Focusing window: {window.WindowTitle}");
        _focusedWindow = window;
        
        // Move focused window to top
        MoveChild(window, GetChildCount() - 1);
        
        // Update window appearance
        foreach (var w in _windows)
        {
            bool isFocused = w == window;
            var titleBar = w.GetNodeOrNull<Panel>("VBoxContainer/TitleBar");
            if (titleBar == null)
            {
                GD.PrintErr($"Could not find title bar at 'VBoxContainer/TitleBar' for window: {w.WindowTitle}");
                continue;
            }
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
            else
            {
                GD.PrintErr($"Could not find title bar for window: {w.WindowTitle}");
            }
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