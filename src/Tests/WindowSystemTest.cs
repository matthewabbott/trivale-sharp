// src/Tests/WindowSystemTest.cs

using Godot;
using System;
using Trivale.Terminal;
using Trivale.OS;

namespace Trivale.Tests;

public partial class WindowSystemTest : Node
{
	private SystemDesktop _desktop;
	private WindowManager _windowManager;
	private Control _controlPanel;
	
	public override void _Ready()
	{
		GD.Print("WindowSystemTest._Ready called");
		
		_desktop = GetParent<SystemDesktop>();
		if (_desktop == null)
		{
			GD.PrintErr("WindowSystemTest must be a child of SystemDesktop");
			return;
		}
		
		// Get or create window manager
		_windowManager = _desktop.GetNode<WindowManager>("MainContainer/WindowLayer");
		if (_windowManager == null)
		{
			GD.PrintErr("Could not find WindowManager node");
			return;
		}
		
		SetupControlPanel();
		CreateInitialWindows();
		
		GD.Print("WindowSystemTest initialization complete");
	}
	
	private void SetupControlPanel()
	{
		GD.Print("Setting up control panel");
		
		_controlPanel = new Control
		{
			Position = new Vector2(20, 20),
		};
		AddChild(_controlPanel);
		
		var buttonContainer = new VBoxContainer();
		_controlPanel.AddChild(buttonContainer);
		
		// Create window button
		var createButton = new Button
		{
			Text = "Create New Window",
			CustomMinimumSize = new Vector2(150, 30)
		};
		createButton.Pressed += () => {
			GD.Print("Create window button pressed");
			CreateNewWindow();
		};
		buttonContainer.AddChild(createButton);
		
		// Toggle CRT button
		var crtButton = new Button
		{
			Text = "Toggle CRT Effect",
			CustomMinimumSize = new Vector2(150, 30)
		};
		crtButton.Pressed += () => {
			GD.Print("Toggle CRT button pressed");
			ToggleCRTEffect();
		};
		buttonContainer.AddChild(crtButton);
		
		GD.Print("Control panel setup complete");
	}
	
	private void CreateInitialWindows()
	{
		GD.Print("Creating initial test windows");
		
		CreateWindowAt("Initial Terminal", new Vector2(100, 100));
		CreateWindowAt("Card Display", new Vector2(550, 100));
		CreateWindowAt("Status Window", new Vector2(100, 450));
		
		GD.Print("Initial windows created");
	}
	
	private void CreateNewWindow()
	{
		GD.Print("Creating new window");
		
		var viewport = GetViewport();
		var viewportRect = viewport.GetVisibleRect();
		var randomPos = new Vector2(
			GD.Randf() * (viewportRect.Size.X - 400),
			GD.Randf() * (viewportRect.Size.Y - 300)
		);
		
		// Cycle through different styles
		var style = (WindowStyle)(_windowManager.GetChildCount() % 5);
		
		var window = new TerminalWindow
		{
			WindowTitle = $"Terminal {_windowManager.GetChildCount() + 1} ({style})",
			Position = randomPos,
			CustomMinimumSize = new Vector2(400, 300),
			Style = style
		};
		
		_windowManager.AddWindow(window);
	}
	
	private void CreateWindowAt(string title, Vector2 position)
	{
		var window = new TerminalWindow
		{
			WindowTitle = title,
			Position = position,
			CustomMinimumSize = new Vector2(400, 300)
		};
		
		GD.Print($"Adding window '{title}' at position {position}");
		_windowManager.AddWindow(window);
	}
	
	private void ToggleCRTEffect()
	{
		GD.Print("Toggling CRT effect");
		var crtOverlay = _desktop.GetNode<ColorRect>("CRTEffect");
		if (crtOverlay != null)
		{
			crtOverlay.Visible = !crtOverlay.Visible;
			GD.Print($"CRT effect visibility: {crtOverlay.Visible}");
		}
		else
		{
			GD.PrintErr("Could not find CRT effect node");
		}
	}
}
