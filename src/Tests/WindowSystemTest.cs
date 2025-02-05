// src/Tests/WindowSystemTest.cs

using Godot;
using System;
using Trivale.Terminal;
using Trivale.OS;

namespace Trivale.Tests;

public partial class WindowSystemTest : Node
{
	private SystemDesktop _desktop;
	private Control _windowContainer;
	
	public override void _Ready()
	{
		_desktop = GetParent<SystemDesktop>();
		if (_desktop == null)
		{
			GD.PrintErr("WindowSystemTest must be a child of SystemDesktop");
			return;
		}
		
		// Get or create window container
		_windowContainer = _desktop.GetNode<Control>("MainContainer/WindowLayer");
		if (_windowContainer == null)
		{
			_windowContainer = new Control
			{
				Name = "WindowLayer",
				LayoutMode = 1,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				MouseFilter = Control.MouseFilterEnum.Pass // This is fine as MouseFilter is already the right type
			};
			_desktop.GetNode<Control>("MainContainer").AddChild(_windowContainer);
		}
		
		CreateTestWindows();
	}
	
	private void CreateTestWindows()
	{
		// Create a simple text terminal
		var textTerminal = new TerminalWindow
		{
			WindowTitle = "Command Terminal",
			Position = new Vector2(100, 100),
			CustomMinimumSize = new Vector2(400, 300)
		};
		_windowContainer.AddChild(textTerminal);
		
		// Create a card terminal
		var cardTerminal = new CardTerminalWindow
		{
			WindowTitle = "Card Display",
			Position = new Vector2(550, 100),
			CustomMinimumSize = new Vector2(400, 300)
		};
		_windowContainer.AddChild(cardTerminal);
		
		// Create a status terminal
		var statusTerminal = new TerminalWindow
		{
			WindowTitle = "System Status",
			Position = new Vector2(100, 450),
			CustomMinimumSize = new Vector2(300, 200)
		};
		_windowContainer.AddChild(statusTerminal);
		
		// Add test controls
		AddTestControls();
	}
	
	private void AddTestControls()
	{
		var controlPanel = new VBoxContainer
		{
			Position = new Vector2(20, 20)
		};
		AddChild(controlPanel);
		
		// Add window creation button
		var createButton = new Button
		{
			Text = "Create New Window",
			CustomMinimumSize = new Vector2(150, 30)
		};
		createButton.Pressed += CreateNewWindow;
		controlPanel.AddChild(createButton);
		
		// Add CRT toggle button
		var crtButton = new Button
		{
			Text = "Toggle CRT Effect",
			CustomMinimumSize = new Vector2(150, 30)
		};
		crtButton.Pressed += ToggleCRTEffect;
		controlPanel.AddChild(crtButton);
	}
	
	private void CreateNewWindow()
	{
		var viewport = GetViewport();
		var viewportRect = viewport.GetVisibleRect();
		
		var window = new TerminalWindow
		{
			WindowTitle = $"Terminal {_windowContainer.GetChildCount() + 1}",
			Position = new Vector2(
				GD.Randf() * (viewportRect.Size.X - 400),
				GD.Randf() * (viewportRect.Size.Y - 300)
			),
			CustomMinimumSize = new Vector2(400, 300)
		};
		_windowContainer.AddChild(window);
	}
	
	private void ToggleCRTEffect()
	{
		var crtOverlay = _desktop.GetNode<ColorRect>("CRTEffect");
		if (crtOverlay != null)
		{
			crtOverlay.Visible = !crtOverlay.Visible;
		}
	}
}
