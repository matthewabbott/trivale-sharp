// src/OS/MainMenu/GameShell.cs
using Godot;
using Trivale.Memory;
using Trivale.UI.Components;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;
using Trivale.OS.Events;

namespace Trivale.OS;

/// <summary>
/// Core system bootstrapper that initializes managers, sets up UI, and launches MainMenuProcess.
/// Provides a clean separation between system bootstrapping and the main menu's functionality.
/// 
/// Key Responsibilities:
/// - Initialize core system managers (Process, Slot, Scene)
/// - Set up the three-panel layout
/// - Start MainMenuProcess in slot_0_0
/// - Handle system event subscriptions
/// 
/// Dependencies:
/// - ProcessManager: For process lifecycle management
/// - SlotManager: For memory slot management
/// - SceneOrchestrator: For scene switching and management
/// - UI Components: SlotGridSystem, ResourcePanel
/// - SystemEventBus: For decoupled event communication
/// </summary>
public partial class GameShell : Control
{
	private SlotGridSystem _slotSystem;
	private Control _mainContent;
	
	// Core system managers
	private SlotManager _slotManager;
	private ProcessManager _processManager;
	private SceneOrchestrator _sceneOrchestrator;
	private SystemEventBus _eventBus;
	private ProcessSlotRegistry _processSlotRegistry;

	public override void _Ready()
	{
		GD.Print("GameShell._Ready started"); // Diagnostic log
		CustomMinimumSize = new Vector2(800, 600);

		// Get instance of event bus
		_eventBus = SystemEventBus.Instance;
		
		// Set up event listeners
		SubscribeToEvents();

		// Create the process-slot registry
		_processSlotRegistry = new ProcessSlotRegistry();

		// Create core managers
		_slotManager = new SlotManager(2, 2);  // 2x2 grid of slots
        _slotManager.UnlockSlot("slot_0_1");
        _slotManager.UnlockSlot("slot_1_0");
        
        _sceneOrchestrator = new SceneOrchestrator();
		_processManager = new ProcessManager(_slotManager, _processSlotRegistry, _sceneOrchestrator);
		
		// Add managers as children to ensure their _Ready methods are called
		AddChild(_slotManager);      // Ensure SlotManager._Ready runs
		AddChild(_processManager);   // Ensure ProcessManager._Ready runs
		AddChild(_sceneOrchestrator);

		// Set up the three-panel layout
		SetupLayout();
		
		// Initialize orchestrator - pass registry
		_sceneOrchestrator.Initialize(_processManager, _slotManager, _processSlotRegistry, _mainContent);
		
        // Start main menu as a standard process
        string processId = _processManager.CreateProcess("MainMenu", null, "mainmenu");
        if (_processManager.StartProcess(processId, "slot_0_0", out string slotId))
        {
            GD.Print($"Main menu started in {slotId}");
            _processSlotRegistry.SetActiveProcess(processId);
        }
        else
        {
            GD.PrintErr("Failed to start main menu process");
        }
		
		// Listen for system mode changes
		_eventBus.SystemModeChanged += OnSystemModeChanged;
		GD.Print("GameShell._Ready completed"); // Confirm completion
	}
	
	private void SubscribeToEvents()
	{
		// Subscribe to system-wide events for logging or handling at the application level
		_eventBus.SystemStarted += () => GD.Print("System started");
		_eventBus.SystemShutdown += () => GD.Print("System shutting down");
		
		// Process lifecycle events can be logged at this level
		_eventBus.ProcessCreated += (processId) => GD.Print($"[EVENT] Process created: {processId}");
		_eventBus.ProcessStarted += (processId, slotId) => GD.Print($"[EVENT] Process started: {processId} in slot {slotId}");
		_eventBus.ProcessEnded += (processId) => GD.Print($"[EVENT] Process ended: {processId}");
		
		// Scene lifecycle events
		_eventBus.SceneLoaded += (scenePath) => GD.Print($"[EVENT] Scene loaded: {scenePath}");
		_eventBus.SceneUnloaded += (scenePath, returningToMainMenu) => 
			GD.Print($"[EVENT] Scene unloaded: {scenePath}, returning to menu: {returningToMainMenu}");
	}
	
	private void OnSystemModeChanged(SystemMode mode)
	{
		GD.Print($"[EVENT] System mode changed to: {mode}");
		
		// Here you could modify UI elements based on mode
		// For example, showing different resource visualizations
		// or changing theme colors based on mode
	}

	private StyleBoxFlat CreatePanelStyle()
	{
		return new StyleBoxFlat
		{
			BgColor = new Color(0, 0.05f, 0, 0.9f),  // Very dark green
			BorderColor = new Color(0, 1, 0),         // Bright green
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1
		};
	}

	private void SetupLayout()
	{
		// Set up the root Control node to fill the window
		LayoutMode = 1;  // Use anchors
		AnchorsPreset = (int)LayoutPreset.FullRect;
		GrowHorizontal = GrowDirection.Both;
		GrowVertical = GrowDirection.Both;

		// Main container with margins
		var marginContainer = new MarginContainer
		{
			LayoutMode = 1,
			AnchorsPreset = (int)LayoutPreset.FullRect,
			GrowHorizontal = GrowDirection.Both,
			GrowVertical = GrowDirection.Both
		};
		marginContainer.AddThemeConstantOverride("margin_left", 20);
		marginContainer.AddThemeConstantOverride("margin_right", 20);
		marginContainer.AddThemeConstantOverride("margin_top", 20);
		marginContainer.AddThemeConstantOverride("margin_bottom", 20);
		AddChild(marginContainer);

		// Main horizontal container
		var mainContainer = new HBoxContainer
		{
			AnchorsPreset = (int)LayoutPreset.FullRect,
			GrowHorizontal = GrowDirection.Both,
			GrowVertical = GrowDirection.Both,
			Theme = new Theme()
		};
		marginContainer.AddChild(mainContainer);

		// === Left Panel (Memory Slots) ===
		var leftPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(200, 0),
			SizeFlagsVertical = SizeFlags.Fill,
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,
			SizeFlagsStretchRatio = 0.25f  // 25% of space
		};
		leftPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		mainContainer.AddChild(leftPanel);

		var leftContent = new VBoxContainer
		{
			SizeFlagsVertical = SizeFlags.Fill,
			SizeFlagsHorizontal = SizeFlags.Fill
		};
		leftPanel.AddChild(leftContent);

		// MEM header
		var memHeader = new Label
		{
			Text = "MEM",
			CustomMinimumSize = new Vector2(0, 30)
		};
		leftContent.AddChild(memHeader);

		// Memory slot management UI
		_slotSystem = new SlotGridSystem();
		_slotSystem.Initialize(_slotManager, _processSlotRegistry);
		_slotSystem.SlotSelected += OnSlotSelected;
		leftContent.AddChild(_slotSystem);

		// Slot grid display
		var slotDisplay = new SlotGridDisplay();
		slotDisplay.Initialize(_slotSystem);
		leftContent.AddChild(slotDisplay);

		// === Center Panel (Main Content) ===
		var centerPanel = new PanelContainer
		{
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill,
			SizeFlagsStretchRatio = 0.5f  // 50% of space
		};
		centerPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		mainContainer.AddChild(centerPanel);

		_mainContent = new MarginContainer
		{
			SizeFlagsHorizontal = SizeFlags.Fill,
			SizeFlagsVertical = SizeFlags.Fill
		};
		_mainContent.AddThemeConstantOverride("margin_left", 10);
		_mainContent.AddThemeConstantOverride("margin_right", 10);
		_mainContent.AddThemeConstantOverride("margin_top", 10);
		_mainContent.AddThemeConstantOverride("margin_bottom", 10);
		centerPanel.AddChild(_mainContent);

		// === Right Panel (Resources) ===
		var rightPanel = new PanelContainer
		{
			CustomMinimumSize = new Vector2(200, 0),
			SizeFlagsVertical = SizeFlags.Fill,
			SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,
			SizeFlagsStretchRatio = 0.25f  // 25% of space
		};
		rightPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
		mainContainer.AddChild(rightPanel);

		// Resource monitoring component
		var resourcePanel = new UI.Components.ResourcePanel();
		resourcePanel.Initialize(_slotManager);
		rightPanel.AddChild(resourcePanel);
	}

	private void OnSlotSelected(string slotId, string processId)
	{
		GD.Print($"Slot selected: {slotId}, Process: {processId}");
		if (_sceneOrchestrator != null && !string.IsNullOrEmpty(processId))
		{
			_sceneOrchestrator.HandleSlotSelected(slotId, processId);
		}
	}

	public override void _ExitTree()
	{
		// Unsubscribe from events
		if (_eventBus != null)
		{
			_eventBus.SystemModeChanged -= OnSystemModeChanged;
			
			// No need to unsubscribe from others as they're using lambdas
			// and will be garbage collected with this instance
		}
		
		// Clean up orchestrator
		if (_sceneOrchestrator != null)
		{
			_sceneOrchestrator.QueueFree();
		}

		if (_slotSystem != null)
		{
			_slotSystem.SlotSelected -= OnSlotSelected;
		}
		
		// Clear references
		_processManager = null;
		_slotManager = null;
		_sceneOrchestrator = null;
		_eventBus = null;
		_processSlotRegistry = null;
		
		base._ExitTree();
	}
}
