// src/OS/WindowManager.cs

using Godot;
using System.Collections.Generic;
using Trivale.Terminal;

namespace Trivale.OS;

public partial class WindowManager : Control
{
    private List<TerminalWindow> _windows = new();
    private TerminalWindow _focusedWindow;
    
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Pass;
    }
    
    public void AddWindow(TerminalWindow window)
    {
        _windows.Add(window);
        AddChild(window);
        window.GuiInput += (InputEvent @event) => HandleWindowInput(window, @event);
        FocusWindow(window);
    }
    
    private void HandleWindowInput(TerminalWindow window, InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && 
            mouseEvent.ButtonIndex == MouseButton.Left && 
            mouseEvent.Pressed)
        {
            FocusWindow(window);
        }
    }
    
    private void FocusWindow(TerminalWindow window)
    {
        if (_focusedWindow == window) return;
        
        _focusedWindow = window;
        
        // Move focused window to top
        RemoveChild(window);
        AddChild(window);
        
        // Update window appearance
        foreach (var w in _windows)
        {
            bool isFocused = w == window;
            if (w.GetNode<Panel>("TitleBar") is Panel titleBar)
            {
                var styleBox = (StyleBoxFlat)titleBar.GetThemeStylebox("panel").Duplicate();
                styleBox.BgColor = isFocused ? 
                    new Color(0.2f, 0.2f, 0.2f, 0.95f) : 
                    new Color(0.1f, 0.1f, 0.1f, 0.95f);
                titleBar.AddThemeStyleboxOverride("panel", styleBox);
            }
        }
    }
    
    public void RemoveWindow(TerminalWindow window)
    {
        _windows.Remove(window);
        window.QueueFree();
        
        if (_focusedWindow == window)
        {
            _focusedWindow = _windows.Count > 0 ? _windows[^1] : null;
        }
    }
}