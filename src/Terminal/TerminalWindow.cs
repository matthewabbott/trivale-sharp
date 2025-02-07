using Godot;
using System;
using System.Collections.Generic;

namespace Trivale.Terminal;

public enum WindowStyle
{
	Normal,
	Alert,
	Secure,
	Corrupted,
	Debug
}

public partial class TerminalWindow : Control
{
	protected Panel _titleBar;
	protected Label _titleLabel;
	protected Panel _contentPanel;
	protected Button _closeButton;
	protected Vector2 _dragOffset;
	protected bool _isDragging;
	protected bool _isResizing;
	protected Vector2 _minSize = new(400, 300);
	protected Vector2 _resizeHandleSize = new(10, 10);
	protected Queue<Control> _pendingContent = new();  // Add this for content queuing
	
	[Export]
	public string WindowTitle { get; set; } = "Terminal";
	
	[Export]
	public Color BorderColor { get; set; } = new(0, 1, 0);
	
	[Export]
	public WindowStyle Style { get; set; } = WindowStyle.Normal;
	
	[Export]
	public Vector2 MinSize
	{
		get => _minSize;
		set
		{
			_minSize = value;
			CustomMinimumSize = _minSize;
		}
	}
	
	public override void _Ready()
	{
		GD.Print($"Setting up window: {WindowTitle}");
		
		// Set up Control node properties
		CustomMinimumSize = MinSize;
		Size = MinSize;
		MouseFilter = MouseFilterEnum.Stop;
		
		// Add main container first
		var layout = new VBoxContainer
		{
			Name = "VBoxContainer",
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill,
			MouseFilter = MouseFilterEnum.Pass
		};
		AddChild(layout);
		
		// Add background with drop shadow
		var background = new ColorRect
		{
			Name = "Background",
			ZIndex = -1,
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
			Color = new Color(0, 0, 0, 0.9f),
			MouseFilter = MouseFilterEnum.Ignore
		};
		AddChild(background);
		
		// Create content panel early
		_contentPanel = new Panel
		{
			Name = "ContentPanel",
			SizeFlagsVertical = SizeFlags.Fill,
			MouseFilter = MouseFilterEnum.Pass
		};
		layout.AddChild(_contentPanel);
		
		// Setup window style after content panel exists
		ApplyStyle(Style);
		
		// Process any content that was queued before _Ready
		ProcessPendingContent();
		
		// Ensure window starts within viewport bounds
		CallDeferred(nameof(ClampToViewport));
		
		GD.Print($"Window setup complete for {WindowTitle} at position {GlobalPosition}");
	}
	
	public override void _Process(double delta)
	{
		if (_isDragging || _isResizing)
		{
			ClampToViewport();
		}
	}
	
	private void ClampToViewport()
	{
		var viewport = GetViewport();
		if (viewport != null)
		{
			var rect = viewport.GetVisibleRect();
			Position = new Vector2(
				Mathf.Clamp(Position.X, 0, rect.Size.X - Size.X),
				Mathf.Clamp(Position.Y, 0, rect.Size.Y - Size.Y)
			);
		}
	}
	
	private void ApplyStyle(WindowStyle style)
	{
		var layout = GetNode<VBoxContainer>("VBoxContainer");
		
		// Create title bar panel directly
		_titleBar = new Panel
		{
			Name = "TitleBar",
			CustomMinimumSize = new Vector2(0, 30),
			SizeFlagsHorizontal = SizeFlags.Fill,
			MouseFilter = MouseFilterEnum.Stop
		};
		layout.AddChild(_titleBar);
		
		// Create inner container for title bar contents
		var titleBarContents = new HBoxContainer
		{
			Name = "TitleBarContents",
			AnchorsPreset = (int)LayoutPreset.FullRect,
			MouseFilter = MouseFilterEnum.Ignore
		};
		_titleBar.AddChild(titleBarContents);
		
		// Title text
		_titleLabel = new Label
		{
			Name = "TitleLabel",
			Text = WindowTitle,
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Left,
			MouseFilter = MouseFilterEnum.Ignore,
		};
		titleBarContents.AddChild(_titleLabel);
		
		// Close button
		_closeButton = new Button
		{
			Text = "×",
			CustomMinimumSize = new Vector2(30, 0),
			SizeFlagsVertical = SizeFlags.Fill,
			MouseFilter = MouseFilterEnum.Stop
		};
		_closeButton.Pressed += OnClosePressed;
		titleBarContents.AddChild(_closeButton);

		
		// Create content panel
		_contentPanel = new Panel
		{
			Name = "ContentPanel",
			SizeFlagsVertical = SizeFlags.Fill,
			MouseFilter = MouseFilterEnum.Pass
		};
		layout.AddChild(_contentPanel);
		
		var (bgColor, borderColor, titleBgColor) = GetColorsForStyle(style);
		
		// Apply title bar style
		var titleStylebox = new StyleBoxFlat
		{
			BgColor = titleBgColor,
			BorderColor = borderColor,
			BorderWidthBottom = 1,
			BorderWidthLeft = 0,
			BorderWidthRight = 0,
			BorderWidthTop = 0,
			ContentMarginBottom = 4
		};
		_titleBar.AddThemeStyleboxOverride("panel", titleStylebox);
		
		// Apply content panel style
		var contentStylebox = new StyleBoxFlat
		{
			BgColor = bgColor,
			BorderColor = borderColor,
			BorderWidthBottom = style == WindowStyle.Debug ? 2 : 1,
			BorderWidthLeft = style == WindowStyle.Debug ? 2 : 1,
			BorderWidthRight = style == WindowStyle.Debug ? 2 : 1,
			ContentMarginLeft = 10,
			ContentMarginRight = 10,
			ContentMarginTop = 10,
			ContentMarginBottom = 10
		};
		_contentPanel.AddThemeStyleboxOverride("panel", contentStylebox);
		
		_titleLabel.Modulate = borderColor;
		
		// Add input event handlers
		_titleBar.GuiInput += OnTitleBarInput;
		GuiInput += OnWindowInput;
	}
	
	private void OnTitleBarInput(InputEvent @event)
	{
		// Print debug info to verify the handler is being called
		GD.Print($"Title bar input received: {@event.GetType()}");
		
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				if (mouseButton.Pressed)
				{
					_isDragging = true;
					_dragOffset = GetLocalMousePosition();
					GD.Print($"Started dragging at offset: {_dragOffset}");
					
					// Request focus when starting drag
					EmitSignal(SignalName.GuiInput, @event);
				}
				else
				{
					_isDragging = false;
					GD.Print("Stopped dragging");
				}
				GetViewport().SetInputAsHandled();
			}
		}
		else if (@event is InputEventMouseMotion mouseMotion && _isDragging)
		{
			var newPos = GlobalPosition + mouseMotion.Relative;
			GlobalPosition = newPos;
			GD.Print($"Dragging to: {newPos}");
			GetViewport().SetInputAsHandled();
		}
	}
	
	private void OnWindowInput(InputEvent @event)
	{
		var resizeArea = new Rect2(
			Size - _resizeHandleSize,
			_resizeHandleSize
		);
		
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				Vector2 localPos = mouseButton.Position;
				
				if (resizeArea.HasPoint(localPos))
				{
					_isResizing = mouseButton.Pressed;
					GetViewport().SetInputAsHandled();
				}
			}
		}
		else if (@event is InputEventMouseMotion mouseMotion && _isResizing)
		{
			Size = new Vector2(
				Mathf.Max(Size.X + mouseMotion.Relative.X, MinSize.X),
				Mathf.Max(Size.Y + mouseMotion.Relative.Y, MinSize.Y)
			);
			GetViewport().SetInputAsHandled();
		}
	}
	
	private void OnClosePressed()
	{
		GD.Print($"Closing window: {WindowTitle}");
		QueueFree();
	}
	
	private (Color bg, Color border, Color titleBg) GetColorsForStyle(WindowStyle style)
	{
		return style switch
		{
			WindowStyle.Normal => (
				new Color(0, 0.05f, 0, 0.9f),
				new Color(0, 1, 0, 1),
				new Color(0, 0.1f, 0, 0.95f)
			),
			WindowStyle.Alert => (
				new Color(0.2f, 0, 0, 0.9f),
				new Color(1, 0, 0, 1),
				new Color(0.3f, 0, 0, 0.95f)
			),
			WindowStyle.Secure => (
				new Color(0, 0, 0.05f, 0.9f),
				new Color(0, 0.5f, 1, 1),
				new Color(0, 0, 0.1f, 0.95f)
			),
			WindowStyle.Corrupted => (
				new Color(0.1f, 0, 0.1f, 0.9f),
				new Color(1, 0, 1, 1),
				new Color(0.15f, 0, 0.15f, 0.95f)
			),
			WindowStyle.Debug => (
				new Color(0.1f, 0.1f, 0.1f, 0.5f),
				new Color(1, 1, 0, 1),
				new Color(0.15f, 0.15f, 0.15f, 0.95f)
			),
			_ => (
				new Color(0, 0.05f, 0, 0.9f),
				new Color(0, 1, 0, 1),
				new Color(0, 0.1f, 0, 0.95f)
			)
		};
	}
	
	public void AddContent(Control content)
	{
		if (_contentPanel != null)
		{
			GD.Print($"Adding content directly to {WindowTitle}");
			_contentPanel.AddChild(content);
		}
		else
		{
			GD.Print($"Queuing content for {WindowTitle}");
			_pendingContent.Enqueue(content);
		}
	}
	
	protected void ProcessPendingContent()
	{
		GD.Print($"Processing pending content for {WindowTitle}");
		while (_pendingContent.Count > 0 && _contentPanel != null)
		{
			var content = _pendingContent.Dequeue();
			_contentPanel.AddChild(content);
			GD.Print($"Added queued content to {WindowTitle}");
		}
	}
	
	public override void _ExitTree()
	{
		base._ExitTree();
		GD.Print($"Window cleaned up: {WindowTitle}");
	}
}
