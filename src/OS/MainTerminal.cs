using Godot;
using System;
using Trivale.Memory;
using Trivale.Encounters;
using Trivale.UI.Components;
using Trivale.OS.Processes;
using Trivale.OS.UI;

namespace Trivale.OS
{
	public partial class MainTerminal : Control
	{
		private ProcessManager _processManager;
		private MemoryGridView _memoryGrid;
		
		// Container + subviewport references:
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

		// Menu stuff
		private MenuProcess _menuProcess;
		private bool _isSlot0Loaded = false;
		
		public override void _Ready()
		{
			// Create managers
			_processManager = new ProcessManager();
			_windowManager = new WindowManager();

			AddChild(_processManager);
			AddChild(_windowManager);
			
			// WindowManager ignores input by default
			_windowManager.MouseFilter = MouseFilterEnum.Ignore;
			_processManager.Initialize(_windowManager);

			SetupLayout();
			SetupEffects();

			// Start with the main menu
			InitializeMainMenu();
		}

		private void InitializeMainMenu()
		{
			// Create menu process (doesn't go in a slot)
			_menuProcess = new MenuProcess("menu_main", _processManager);
			_menuProcess.Initialize(null);
			_menuProcess.ProcessEvent += OnMenuProcessEvent;

			// Clear anything previously in the SubViewport
			foreach (var child in _viewport.GetChildren())
				child.QueueFree();

			// Get the menu scene from process state
			var state = _menuProcess.GetState();
			if (state.TryGetValue("menuScene", out var sceneObj) && sceneObj is Node menuScene)
			{
				_viewport.AddChild(menuScene);
				GD.Print("Added menu scene to viewport");
			}
			else
			{
				GD.PrintErr("Failed to get menu scene from process state");
			}
		}

		private void OnMenuProcessEvent(string eventType)
		{
			if (eventType.StartsWith("loaded_"))
			{
				// A process was loaded into slot 0
				_isSlot0Loaded = true;
			
				// Update memory grid to show additional slots
				_memoryGrid?.UpdateDisplay();
			}
		}

		private void SetupLayout()
		{
			// Background panel at the bottom
			_background = new Panel
			{
				LayoutMode = 1,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
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

			// Main content container with margins
			var marginContainer = new MarginContainer
			{
				LayoutMode = 1,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
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
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				MouseFilter = MouseFilterEnum.Pass
			};
			marginContainer.AddChild(mainLayout);

			// System info at top
			_systemInfoPanel = CreateSystemInfoPanel();
			mainLayout.AddChild(_systemInfoPanel);

			// Content area in middle
			var contentLayout = new HBoxContainer
			{
				SizeFlagsVertical = SizeFlags.Fill,
				SizeFlagsHorizontal = SizeFlags.Fill,  // Added this to ensure proper filling
				MouseFilter = MouseFilterEnum.Pass
			};
			mainLayout.AddChild(contentLayout);

			// Memory grid on left with fixed width
			_memoryGrid = new MemoryGridView
			{
				CustomMinimumSize = new Vector2(TerminalConfig.Layout.MemSlotWidth, 0),
				ZIndex = 1,
				MouseFilter = MouseFilterEnum.Stop
			};
			_memoryGrid.MemorySlotSelected += OnSlotSelected;
			_memoryGrid.ShowAdditionalSlots = false;
			contentLayout.AddChild(_memoryGrid);

			// Setup viewport in center - should expand to fill available space
			SetupViewport(contentLayout);

			// Resource panel on right with fixed width
			_resourcePanel = CreateResourcePanel();
			contentLayout.AddChild(_resourcePanel);

			// Scanlines overlay on top
			_scanlines = new Panel
			{
				LayoutMode = 1,
				AnchorsPreset = (int)Control.LayoutPreset.FullRect,
				MouseFilter = MouseFilterEnum.Ignore,
				ZIndex = 100
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
				CustomMinimumSize = new Vector2(800, 600),  // Minimum size to ensure content fits
				StretchShrink = 1,
				MouseFilter = MouseFilterEnum.Pass
			};
			parent.AddChild(_mainViewport);

			_viewport = new SubViewport
			{
				HandleInputLocally = false,
				GuiDisableInput = false,
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

			// Get existing process for this slot
			var process = _processManager.GetProcess(slotId);
			if (process == null)
			{
				if (slotId == "SLOT_0_0")
				{
					// Slot 0 is empty, do nothing (main menu is already showing)
					return;
				}

			// Only allow loading into other slots if Slot 0 is loaded
				if (!_isSlot0Loaded)
				{
					GD.Print("Cannot load process - Slot 0 is empty");
					return;
				}

			// TODO: Handle loading appropriate process type based on what's in Slot 0
				GD.Print("TODO: Load appropriate process based on Slot 0 contents");
				return;
			}

			// Show the process scene in viewport
			var scene = _processManager.GetProcessScene(process.Id);
			if (scene != null)
			{
				foreach (var child in _viewport.GetChildren())
					child.QueueFree();

				_viewport.AddChild(scene);
				GD.Print($"Loaded process {process.Id} into viewport");
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

		// Dynamically update the SubViewport size:
		public override void _Process(double delta)
		{
			base._Process(delta);

			if (_mainViewport != null && _viewport != null)
			{
				// Match viewport size to container size, but never smaller than 800x600
				Vector2 size = _mainViewport.Size;
				_viewport.Size = new Vector2I(
					Mathf.Max((int)size.X, 800),
					Mathf.Max((int)size.Y, 600)
				);
			}
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventMouse mouseEvent)
			{
				GD.Print($"[MainTerminal] _UnhandledInput: {mouseEvent.GetType()} at {mouseEvent.Position}");
			}
			else
			{
				GD.Print($"[MainTerminal] _UnhandledInput: {@event}");
			}
		}

		public override void _GuiInput(InputEvent @event)
		{
			if (@event is InputEventMouse mouseEvent)
			{
				// Too spammy
				// GD.Print($"[MainTerminal] _GuiInput: {mouseEvent.GetType()} at {mouseEvent.Position}");
			}
			else
			{
				GD.Print($"[MainTerminal] _GuiInput: {@event}");
			}
		}

		public override void _ExitTree()
		{
			if (_menuProcess != null)
			{
				_menuProcess.ProcessEvent -= OnMenuProcessEvent;
				_menuProcess.Cleanup();
			}
		}
	}
}
