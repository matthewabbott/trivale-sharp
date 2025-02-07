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
		
		_windowManager = _desktop.GetNode<WindowManager>("WindowLayer");
		if (_windowManager == null)
		{
			GD.PrintErr("Could not find WindowManager");
			return;
		}
		
		SetupControlPanel();
		CreateInitialWindows();
	}
	
	private void SetupControlPanel()
	{
		GD.Print("Setting up WindowSystemTest control panel");
		_controlPanel = new Control
		{
			Position = new Vector2(20, 20),
			Visible = true  // Changed from Show to Visible
		};
		AddChild(_controlPanel);
		
		var buttonContainer = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(150, 0),
			Visible = true
		};
		_controlPanel.AddChild(buttonContainer);
		
		var createButton = new Button
		{
			Text = "Create New Window",
			CustomMinimumSize = new Vector2(150, 30)
		};
		createButton.Pressed += CreateNewWindow;
		buttonContainer.AddChild(createButton);
		
		var crtButton = new Button
		{
			Text = "Toggle CRT Effect",
			CustomMinimumSize = new Vector2(150, 30)
		};
		crtButton.Pressed += ToggleCRTEffect;
		buttonContainer.AddChild(crtButton);
		
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
		
		var hoverStyle = alertStyle.Duplicate() as StyleBoxFlat;
		if (hoverStyle != null)
		{
			hoverStyle.BgColor = new Color(0.7f, 0, 0, 1);
		}
		alertButton.AddThemeStyleboxOverride("hover", hoverStyle);
		
		alertButton.Pressed += StartAlertCascade;
		buttonContainer.AddChild(alertButton);
		
		GD.Print("WindowSystemTest control panel setup complete");
	}
	
	private void CreateInitialWindows()
	{
		GD.Print("Creating initial test windows");
		
		for (int i = 0; i < 3; i++)
		{
			CreateNewWindow();
		}
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
		
		var style = (WindowStyle)(_windowManager.GetChildCount() % 5);
		
		var window = new TerminalWindow
		{
			WindowTitle = $"Terminal {_windowManager.GetChildCount() + 1} ({style})",
			Position = randomPos,
			MinSize = new Vector2(400, 300),
			Style = style
		};
		
		var content = new VBoxContainer();
		content.AddChild(new Label { Text = "Test Window Content" });
		window.AddContent(content);
		
		GD.Print($"Adding window to manager: {window.WindowTitle}");
		_windowManager.AddWindow(window);
	}
	
	private async void StartAlertCascade()
	{
		GD.Print("Starting alert cascade");
		var viewport = GetViewport();
		var viewportRect = viewport.GetVisibleRect();
		double delay = 0.1;
		int numAlerts = 8;
		
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
				MinSize = new Vector2(400, 300),
				Style = WindowStyle.Alert
			};
			
			var content = new VBoxContainer();
			content.AddChild(new Label 
			{ 
				Text = "⚠ WARNING ⚠\n\nSecurity breach detected!\nUnauthorized access attempt in progress.\nInitiating countermeasures...",
				HorizontalAlignment = HorizontalAlignment.Center,
				CustomMinimumSize = new Vector2(0, 100)
			});
			
			window.AddContent(content);
			_windowManager.AddWindow(window);
			
			await ToSignal(GetTree(), "process_frame");
			await ToSignal(GetTree().CreateTimer(delay), "timeout");
		}
	}
	
	private bool _debugVisible = true;
	
	private void ToggleCRTEffect()
	{
		GD.Print("Toggling CRT effect");
		_debugVisible = !_debugVisible;
		
		var crtOverlay = _desktop.GetNode<ColorRect>("CRTEffect");
		if (crtOverlay != null)
		{
			crtOverlay.Visible = !crtOverlay.Visible;
		}
		
		_windowManager.SetDebugBoundsVisible(_debugVisible);
	}
}
