// src/OS/MainTerminal.cs
/*
using Godot;
using System;
using Trivale.Memory;
using Trivale.Encounters;
using Trivale.UI.Components;

namespace Trivale.OS;

public partial class MainTerminal : Control
{
	private ProcessManager _processManager;
	private MemoryGridView _memoryGrid;
	private SubViewportContainer _mainViewport;
	private SubViewport _viewport;
	private Control _systemInfoPanel;
	private Control _resourcePanel;
	private Panel _background;
	private Panel _scanlines;
	private WindowManager _windowManager;
	private Label _memoryLabel;
	private Label _cpuLabel;
	private Label _availableLabel;
		
	public override void _Ready()
	{
		// Create managers
		_processManager = new ProcessManager();
		_windowManager = new WindowManager();
		
		AddChild(_processManager);
		AddChild(_windowManager);
		
		// Set WindowManager to ignore input by default
		_windowManager.MouseFilter = MouseFilterEnum.Ignore;
		
		_processManager.Initialize(_windowManager);
		
		SetupLayout();
		SetupEffects();
	}
	
	private void SetupLayout()
	{
		// Background panel at the bottom
		_background = new Panel
		{
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
			MouseFilter = MouseFilterEnum.Ignore,
			ZIndex = -1  // Put background behind everything
		};
		var bgStyle = new StyleBoxFlat
		{
			BgColor = TerminalConfig.Colors.Background,
			BorderColor = TerminalConfig.Colors.DimBorder,
			BorderWidthBottom = TerminalConfig.Layout.BorderWidth,
			BorderWidthLeft = TerminalConfig.Layout.BorderWidth,
			BorderWidthRight = TerminalConfig.Layout.BorderWidth,
			BorderWidthTop = TerminalConfig.Layout.BorderWidth
		};
		_background.AddThemeStyleboxOverride("panel", bgStyle);
		AddChild(_background);
		
		// Main content container
		var marginContainer = new MarginContainer
		{
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
			MouseFilter = MouseFilterEnum.Pass,
			ZIndex = 0  // Normal UI elements
		};
		marginContainer.AddThemeConstantOverride("margin_left", TerminalConfig.Layout.WindowMargin);
		marginContainer.AddThemeConstantOverride("margin_right", TerminalConfig.Layout.WindowMargin);
		marginContainer.AddThemeConstantOverride("margin_top", TerminalConfig.Layout.WindowMargin);
		marginContainer.AddThemeConstantOverride("margin_bottom", TerminalConfig.Layout.WindowMargin);
		AddChild(marginContainer);
		
		// Main vertical layout
		var mainLayout = new VBoxContainer
		{
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
			MouseFilter = MouseFilterEnum.Pass  // Allow clicks to pass through to children
		};
		marginContainer.AddChild(mainLayout);
		
		// System info at top
		_systemInfoPanel = CreateSystemInfoPanel();
		mainLayout.AddChild(_systemInfoPanel);
		
		// Content area in middle
		var contentLayout = new HBoxContainer
		{
			SizeFlagsVertical = SizeFlags.Fill,
			MouseFilter = MouseFilterEnum.Pass  // Allow clicks to pass through to children
		};
		mainLayout.AddChild(contentLayout);
		
		// Memory grid on left
		_memoryGrid = new MemoryGridView
		{
			CustomMinimumSize = new Vector2(TerminalConfig.Layout.MemSlotWidth, 0),
			ZIndex = 1,
			MouseFilter = MouseFilterEnum.Stop  // Ensure the grid can receive input
		};
		_memoryGrid.MemorySlotSelected += OnSlotSelected;
		contentLayout.AddChild(_memoryGrid);
		
		// Setup viewport in center
		SetupViewport(contentLayout);
		
		// Resource panel on right
		_resourcePanel = CreateResourcePanel();
		contentLayout.AddChild(_resourcePanel);
		
		// Scanlines overlay on top
		_scanlines = new Panel
		{
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
			MouseFilter = MouseFilterEnum.Ignore,  // Important: ignore input
			ZIndex = 100  // Put scanlines on top
		};
		AddChild(_scanlines);
		
		// Initialize memory grid
		_memoryGrid.Initialize(_processManager);
	}
	
	private void SetupViewport(Control parent)
	{
		_mainViewport = new SubViewportContainer
		{
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Expand | SizeFlags.Fill,
			StretchShrink = 1
		};
		parent.AddChild(_mainViewport);
		
		_viewport = new SubViewport
		{
			HandleInputLocally = true,
			Size = new Vector2I(800, 600),
			RenderTargetUpdateMode = SubViewport.UpdateMode.Always
		};
		_mainViewport.AddChild(_viewport);
	}
	
	private Control CreateSystemInfoPanel()
	{
		var panel = new Panel
		{
			CustomMinimumSize = new Vector2(0, 80),
			MouseFilter = MouseFilterEnum.Pass
		};
		
		var container = new VBoxContainer
		{
			MouseFilter = MouseFilterEnum.Pass
		};
		panel.AddChild(container);
		
		var title = new Label { Text = "NETRUNNER OS v1.0" };
		container.AddChild(title);
		
		var statusLabel = new Label { Text = "SYSTEM STATUS: OPERATIONAL" };
		container.AddChild(statusLabel);
		
		return panel;
	}
	
	private Control CreateResourcePanel()
	{
		var panel = new Panel
		{
			CustomMinimumSize = new Vector2(200, 0),
			MouseFilter = MouseFilterEnum.Pass
		};
		
		var container = new VBoxContainer
		{
			MouseFilter = MouseFilterEnum.Pass
		};
		panel.AddChild(container);
		
		var title = new Label { Text = "RESOURCES" };
		container.AddChild(title);
		
		_memoryLabel = new Label { Text = "MEMORY USAGE: 0%" };
		container.AddChild(_memoryLabel);
		
		_cpuLabel = new Label { Text = "CPU USAGE: 0%" };
		container.AddChild(_cpuLabel);
		
		container.AddChild(new HSeparator());
		
		_availableLabel = new Label { Text = "AVAILABLE:\nMEMORY: 100%\nCPU: 100%" };
		container.AddChild(_availableLabel);
		
		return panel;
	}
	
	private void OnSlotSelected(string slotId)
	{
		GD.Print($"Processing slot selection for {slotId}");
		
		// If no process exists, create one
		var process = _processManager.GetProcess(slotId);
		if (process == null)
		{
			var cardGameProcess = new CardGameProcess($"card_game_{slotId}");
			if (!_processManager.StartProcess(cardGameProcess))
			{
				GD.PrintErr($"Failed to start card game in slot: {slotId}");
				return;
			}
			process = cardGameProcess;
		}
		
		// Get and show the process scene
		var scene = _processManager.GetProcessScene(process.Id);
		if (scene == null)
		{
			GD.PrintErr($"No ProcessScene found for {process.Id}");
			return;
		}
		
		// Clear viewport and add new scene
		foreach (var child in _viewport.GetChildren())
			child.QueueFree();
		
		_viewport.AddChild(scene);
		GD.Print($"Loaded process {process.Id} into viewport");
	}
	
	private void OnGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouse mouseEvent)
		{
			GD.Print($"MainTerminal received mouse event at: {mouseEvent.Position}");
		}
	}
	
	private void SetupEffects()
	{
		if (_scanlines == null) return;

		var shader = GD.Load<Shader>("res://Assets/Shaders/crt_effect.gdshader");
		if (shader == null)
		{
			GD.PrintErr("Failed to load CRT shader");
			return;
		}

		var material = new ShaderMaterial { Shader = shader };
		TerminalConfig.CRTEffect.ApplyToMaterial(material);
		_scanlines.Material = material;
	}
}
*/
