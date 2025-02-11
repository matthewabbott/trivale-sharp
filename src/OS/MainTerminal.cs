// src/OS/MainTerminal.cs
using Godot;
using System;
using System.Collections.Generic;
using Trivale.Memory;
using Trivale.Encounters;

namespace Trivale.OS;

public partial class MainTerminal : Control
{
	private ProcessManager _processManager;
	private GridContainer _memSlotGrid;
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
		// Create and initialize ProcessManager first
		_processManager = new ProcessManager();
		AddChild(_processManager);
		
		// Create WindowManager before setting up layout
		_windowManager = new WindowManager();
		AddChild(_windowManager);
		
		// Initialize ProcessManager with WindowManager
		_processManager.Initialize(_windowManager);
		
		SetupLayout();
		SetupEffects();
	}
	
	private void SetupLayout()
	{
		// Main background
		_background = new Panel
		{
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
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
		
		// Main margin container
		var marginContainer = new MarginContainer
		{
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect
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
			AnchorsPreset = (int)LayoutPreset.FullRect
		};
		marginContainer.AddChild(mainLayout);
		
		// System info panel (top)
		_systemInfoPanel = CreateSystemInfoPanel();
		mainLayout.AddChild(_systemInfoPanel);
		
		// Main content area (middle)
		var contentLayout = new HBoxContainer
		{
			SizeFlagsVertical = SizeFlags.Fill
		};
		mainLayout.AddChild(contentLayout);
		
		// MEM slot grid (left)
		var slotContainer = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(TerminalConfig.Layout.MemSlotWidth, 0)
		};
		contentLayout.AddChild(slotContainer);
		
		var slotLabel = new Label { Text = "SYSTEM MEMORY" };
		slotContainer.AddChild(slotLabel);
		
		_memSlotGrid = new GridContainer
		{
			SizeFlagsVertical = SizeFlags.Fill,
			Columns = TerminalConfig.Layout.MemSlotColumns
		};
		slotContainer.AddChild(_memSlotGrid);
		
		// Main viewport (center)
		_mainViewport = new SubViewportContainer
		{
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Expand | SizeFlags.Fill,
			StretchShrink = 1
		};
		contentLayout.AddChild(_mainViewport);
		
		_viewport = new SubViewport
		{
			HandleInputLocally = true,
			Size = new Vector2I(800, 600),
			RenderTargetUpdateMode = SubViewport.UpdateMode.Always
		};
		_mainViewport.AddChild(_viewport);
		
		// Resource panel (right)
		_resourcePanel = CreateResourcePanel();
		contentLayout.AddChild(_resourcePanel);
		
		// Scanline effect overlay
		_scanlines = new Panel
		{
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
			MouseFilter = MouseFilterEnum.Ignore
		};
		AddChild(_scanlines);
		
		// Initial MEM slot display
		UpdateMemoryDisplay();
	}
	
	private Control CreateSystemInfoPanel()
	{
		var panel = new Panel
		{
			CustomMinimumSize = new Vector2(0, 80)
		};
		
		var container = new VBoxContainer();
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
			CustomMinimumSize = new Vector2(200, 0)
		};
		
		var container = new VBoxContainer();
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
	
	private void UpdateMemoryDisplay()
	{
		// Clear existing display
		foreach (var child in _memSlotGrid.GetChildren())
		{
			child.QueueFree();
		}
		
		var slots = _processManager.GetAllSlots();
		foreach (var slot in slots)
		{
			var slotButton = CreateMemSlotButton(slot);
			_memSlotGrid.AddChild(slotButton);
		}
	}
	
	private Button CreateMemSlotButton(IMemorySlot slot)
	{
		var button = new Button
		{
			Text = $"MEM_{slot.Id}",
			CustomMinimumSize = new Vector2(120, 80),
			TooltipText = $"Memory: {slot.MemoryUsage:P0}\nCPU: {slot.CpuUsage:P0}"
		};
		
		var style = new StyleBoxFlat
		{
			BgColor = new Color(0.1f, 0.1f, 0.1f, 0.95f),
			BorderColor = TerminalConfig.Colors.DimBorder,
			BorderWidthBottom = 1,
			BorderWidthLeft = 1,
			BorderWidthRight = 1,
			BorderWidthTop = 1
		};
		button.AddThemeStyleboxOverride("normal", style);
		
		button.Pressed += () => OnSlotSelected(slot);
		
		return button;
	}
	
	private bool StartCardGameProcess(IMemorySlot slot)
	{
		string processId = $"card_game_{slot.Id}";
		var cardGameProcess = new CardGameProcess(processId);
		bool started = _processManager.StartProcess(cardGameProcess);
		if (!started)
		{
			GD.PrintErr($"Failed to start card game in slot: {slot.Id}");
			return false;
		}
		return true;
	}
	
	private void OnSlotSelected(IMemorySlot slot)
	{
		// If no process is found, create one
		var existingProcess = _processManager.GetProcess(slot.Id);
		if (existingProcess == null)
		{
			if (!StartCardGameProcess(slot))
			{
				// Could not start the card game process
				return;
			}
		}
		
		// Now retrieve whichever process is in there
		var process = _processManager.GetProcess(slot.Id);
		if (process == null)
		{
			GD.PrintErr("Process is null after attempted creation.");
			return;
		}
		
		// Get the scene and place it in the viewport
		var scene = _processManager.GetProcessScene(process.Id);
		if (scene == null)
		{
			GD.PrintErr($"No ProcessScene found for {process.Id}");
			return;
		}
		
		// Clear old content, then show the new scene
		foreach (var child in _viewport.GetChildren())
			child.QueueFree();
		
		_viewport.AddChild(scene);
		GD.Print($"Loaded process {process.Id} into viewport");
	}
	
	private void SetupEffects()
	{
		if (_scanlines == null)
		{
			GD.PrintErr("Scanlines panel not created");
			return;
		}

		var shader = GD.Load<Shader>("res://Assets/Shaders/crt_effect.gdshader");
		if (shader == null)
		{
			GD.PrintErr("Failed to load CRT shader");
			return;
		}

		var material = new ShaderMaterial
		{
			Shader = shader
		};

		// Apply standard CRT settings from config
		TerminalConfig.CRTEffect.ApplyToMaterial(material);
		
		_scanlines.Material = material;
		
		GD.Print("CRT effect setup complete");
	}
	
	private void UpdateResourceDisplay()
	{
		var slots = _processManager.GetAllSlots();
		float totalMemory = 0;
		float totalCpu = 0;
		
		foreach (var slot in slots)
		{
			totalMemory += slot.MemoryUsage;
			totalCpu += slot.CpuUsage;
		}
		
		_memoryLabel.Text = $"MEMORY USAGE: {totalMemory:P0}";
		_cpuLabel.Text = $"CPU USAGE: {totalCpu:P0}";
		_availableLabel.Text = $"AVAILABLE:\nMEMORY: {_processManager.AvailableMemory:P0}\nCPU: {_processManager.AvailableCpu:P0}";
	}
	
	public override void _Process(double delta)
	{
		UpdateResourceDisplay();
	}
}
