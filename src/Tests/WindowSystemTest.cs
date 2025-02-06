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
		
		GD.Print("Looking for WindowManager at 'WindowLayer'...");
		_windowManager = _desktop.GetNode<WindowManager>("WindowLayer");
		if (_windowManager == null)
		{
			GD.PrintErr("Could not find WindowManager node at 'WindowLayer'");
			return;
		}
		
		SetupControlPanel();
		CreateInitialWindows();
		
		GD.Print("WindowSystemTest initialization complete");
	}
	
	private void SetupControlPanel()
	{
		_controlPanel = new Control
		{
			Position = new Vector2(20, 20),
		};
		AddChild(_controlPanel);
		
		var buttonContainer = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(150, 0)
		};
		_controlPanel.AddChild(buttonContainer);
		
		// Create window button
		var createButton = new Button
		{
			Text = "Create New Window",
			CustomMinimumSize = new Vector2(150, 30)
		};
		createButton.Pressed += CreateNewWindow;
		buttonContainer.AddChild(createButton);
		
		// Toggle CRT button
		var crtButton = new Button
		{
			Text = "Toggle CRT Effect",
			CustomMinimumSize = new Vector2(150, 30)
		};
		crtButton.Pressed += ToggleCRTEffect;
		buttonContainer.AddChild(crtButton);
		
		// Alert cascade button
		var alertButton = new Button
		{
			Text = "ALERT!",
			CustomMinimumSize = new Vector2(150, 30)
		};
		var alertStyle = new StyleBoxFlat
		{
			BgColor = new Color(0.5f, 0, 0, 1),
			BorderColor = new Color(1, 0, 0, 1),
			BorderWidthBottom = 2,
			BorderWidthLeft = 2,
			BorderWidthRight = 2,
			BorderWidthTop = 2,
			ContentMarginLeft = 10,
			ContentMarginRight = 10,
			ContentMarginTop = 5,
			ContentMarginBottom = 5
		};
		alertButton.AddThemeStyleboxOverride("normal", alertStyle);
		
		// Add hover effect
		var hoverStyle = alertStyle.Duplicate() as StyleBoxFlat;
		if (hoverStyle != null)
		{
			hoverStyle.BgColor = new Color(0.7f, 0, 0, 1);
		}
		alertButton.AddThemeStyleboxOverride("hover", hoverStyle);
		
		alertButton.Pressed += StartAlertCascade;
		buttonContainer.AddChild(alertButton);
	}
	
	private async void StartAlertCascade()
	{
		var viewport = GetViewport();
		var viewportRect = viewport.GetVisibleRect();
		double delay = 0.1; // Delay between alerts in seconds
		int numAlerts = 8;  // Number of alert windows to create
		
		for (int i = 0; i < numAlerts; i++)
		{
			var randomPos = new Vector2(
				GD.Randf() * (viewportRect.Size.X - 400),
				GD.Randf() * (viewportRect.Size.Y - 300)
			);
			
			var window = new TerminalWindow
			{
				WindowTitle = $"SYSTEM ALERT {i + 1}",
				Position = randomPos,
				CustomMinimumSize = new Vector2(400, 300),
				Style = WindowStyle.Alert
			};
			
			// Add alert content
			var content = new VBoxContainer();
			content.AddChild(new Label 
			{ 
				Text = "⚠ WARNING ⚠\n\nSecurity breach detected!\nUnauthorized access attempt in progress.\nInitiating countermeasures...",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(0, 100)
			});
			
			window.AddChild(content);
			_windowManager.AddWindow(window);
			
			await ToSignal(GetTree().CreateTimer(delay), "timeout");
		}
	}
	
	private void CreateInitialWindows()
	{
		GD.Print("Creating initial test windows");
		
		// Create a few initial windows with specific styles
		for (int i = 0; i < 3; i++)
		{
			CreateNewWindow();
		}
		
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
	
	private bool _debugVisible = true;
	
	private void ToggleCRTEffect()
	{
		_debugVisible = !_debugVisible;
		
		var crtOverlay = _desktop.GetNode<ColorRect>("CRTEffect");
		if (crtOverlay != null)
		{
			crtOverlay.Visible = !crtOverlay.Visible;
		}
		
		_windowManager.SetDebugBoundsVisible(_debugVisible);
	}
}
