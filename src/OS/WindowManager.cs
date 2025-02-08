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
	private HashSet<TerminalWindow> _pendingRemoval = new();
	
	public override void _Ready()
	{
		GD.Print("WindowManager._Ready called");
		
		MouseFilter = MouseFilterEnum.Pass;
		ZIndex = 100;
		
		Position = Vector2.Zero;
		AnchorRight = 1;
		AnchorBottom = 1;
		
		SizeFlagsHorizontal = SizeFlags.Fill;
		SizeFlagsVertical = SizeFlags.Fill;
		
		TopLevel = true;
		
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
		if (_pendingRemoval.Contains(window))
		{
			GD.PrintErr($"Attempted to add window that is pending removal: {window.WindowTitle}");
			return;
		}

		_windows.Add(window);
		AddChild(window);
		
		window.MouseFilter = MouseFilterEnum.Stop;
		window.ZIndex = _windows.Count * 10;
		
		// Store the handler reference so we can properly disconnect it later
		window.GuiInput += (@event) => SafeHandleWindowInput(window, @event);
		window.TreeExiting += () => OnWindowExiting(window);
		
		FocusWindow(window);
		
		GD.Print($"Added window: {window.WindowTitle}, ZIndex: {window.ZIndex}");
	}
	
	private void SafeHandleWindowInput(TerminalWindow window, InputEvent @event)
	{
		if (!IsInstanceValid(window) || _pendingRemoval.Contains(window))
		{
			GD.PrintErr($"Attempted to handle input for invalid window");
			return;
		}

		HandleWindowInput(window, @event);
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
	
	private void OnWindowExiting(TerminalWindow window)
	{
		GD.Print($"Window exiting: {window.WindowTitle}");
		_pendingRemoval.Add(window);
		RemoveWindow(window);
	}
	
	private void FocusWindow(TerminalWindow window)
	{
		if (!IsInstanceValid(window) || _pendingRemoval.Contains(window))
		{
			GD.PrintErr("Attempted to focus invalid window");
			return;
		}

		if (_focusedWindow == window) return;
		
		GD.Print($"Focusing window: {window.WindowTitle}");
		
		_focusedWindow = window;
		
		// Find highest current Z-index
		int maxZ = 0;
		foreach (var w in _windows)
		{
			if (IsInstanceValid(w) && !_pendingRemoval.Contains(w))
			{
				maxZ = Mathf.Max(maxZ, w.ZIndex);
			}
		}
		
		// Set focused window above highest
		window.ZIndex = maxZ + 10;
		CallDeferred(nameof(MoveChildDeferred), window, GetChildCount() - 1);
		
		// Update window appearance
		foreach (var w in _windows.ToArray()) // Use ToArray to avoid modification during enumeration
		{
			if (!IsInstanceValid(w) || _pendingRemoval.Contains(w)) continue;
			
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
		
		GD.Print($"Window {window.WindowTitle} focused with ZIndex: {window.ZIndex}");
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
		
		if (!_windows.Contains(window))
		{
			GD.PrintErr($"Attempted to remove window that isn't tracked: {window.WindowTitle}");
			return;
		}

		_windows.Remove(window);
		
		// Find next window to focus before removing current
		TerminalWindow nextFocus = null;
		if (_focusedWindow == window)
		{
			_focusedWindow = null; // Clear current focus
			foreach (var w in _windows.ToArray())
			{
				if (IsInstanceValid(w) && !_pendingRemoval.Contains(w))
				{
					nextFocus = w;
					break;
				}
			}
		}

		window.QueueFree();
		
		// Set new focus if we found a valid window
		if (nextFocus != null)
		{
			FocusWindow(nextFocus);
		}
	}
	
	private void MoveChildDeferred(Node child, int newIndex)
	{
		if (IsInstanceValid(child) && child.GetParent() == this)
		{
			MoveChild(child, newIndex);
		}
	}

	public override void _ExitTree()
	{
		// Clean up any remaining windows
		foreach (var window in _windows.ToArray())
		{
			if (IsInstanceValid(window) && !_pendingRemoval.Contains(window))
			{
				window.QueueFree();
			}
		}
		_windows.Clear();
		_pendingRemoval.Clear();
		_focusedWindow = null;
	}
}
