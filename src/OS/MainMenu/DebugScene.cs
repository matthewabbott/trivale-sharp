// src/OS/MainMenu/DebugScene.cs

using Godot;
using System.Linq;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;

namespace Trivale.OS.MainMenu;

/// <summary>
/// Debug sandbox scene demonstrating MEM slot system integration.
/// This scene serves as a reference implementation for how to:
/// 1. Set up process and slot management in a scene
/// 2. Handle process lifecycle (create, load, unload)
/// 3. Display and update slot states
/// 4. Clean up properly when exiting
/// 
/// Integration Pattern:
/// 1. Create slot and process managers at scene start
/// 2. Hook up UI controls to process operations
/// 3. Listen to manager events for state updates
/// 4. Clean up processes on scene exit
/// </summary>
public partial class DebugScene : Control
{
    [Signal]
    public delegate void SceneUnloadRequestedEventHandler();
    
    private IProcessManager _processManager;
    private ISlotManager _slotManager;
    private Label _statusLabel;
    private VBoxContainer _slotDisplay;
    private HBoxContainer _buttonContainer;
    private Button _createProcessButton;
    private Button _unloadProcessButton;
    private SystemFont _monospaceFont;
    
    public override void _Ready()
    {
        // Clear any existing children
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }

        // Create managers
        _slotManager = new SlotManager(3, 2);  // 3x2 grid for testing
        _processManager = new ProcessManager(_slotManager);

        // Set up font for ASCII art
        _monospaceFont = new SystemFont();
        _monospaceFont.FontNames = new string[] { "JetBrainsMono-Regular", "Consolas", "Courier New" };
        
        SetupUI();
        ConnectSignals();
        UpdateSlotDisplay();
    }

    private void SetupUI()
    {
        // Main vertical layout that fills the scene
        var layout = new VBoxContainer
        {
            AnchorsPreset = (int)LayoutPreset.FullRect,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both
        };
        AddChild(layout);

        // Title and status area
        var headerContainer = new VBoxContainer();
        layout.AddChild(headerContainer);

        var title = new Label
        {
            Text = "Debug Sandbox",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        headerContainer.AddChild(title);
        
        _statusLabel = new Label
        {
            Text = "MEM Slot System Debug",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        headerContainer.AddChild(_statusLabel);
        
        // Button container with proper margins
        _buttonContainer = new HBoxContainer
        {
            CustomMinimumSize = new Vector2(0, 40)
        };
        layout.AddChild(_buttonContainer);
        
        // Test control buttons
        _createProcessButton = CreateStyledButton("Load Debug Process", Colors.Green);
        _unloadProcessButton = CreateStyledButton("Unload Process", Colors.Red);
        var returnButton = CreateStyledButton("Return to Menu", Colors.White);
        
        _buttonContainer.AddChild(_createProcessButton);
        _buttonContainer.AddChild(_unloadProcessButton);
        _buttonContainer.AddChild(returnButton);
        
        _createProcessButton.Pressed += OnCreateProcessPressed;
        _unloadProcessButton.Pressed += OnUnloadProcessPressed;
        returnButton.Pressed += OnReturnPressed;
        
        // Slot display area with monospace font
        _slotDisplay = new VBoxContainer();
        _slotDisplay.AddThemeFontOverride("font", _monospaceFont);
        layout.AddChild(_slotDisplay);
    }
    
    private Button CreateStyledButton(string text, Color color)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(150, 30)
        };
        
        var normalStyle = new StyleBoxFlat
        {
            BgColor = new Color(color, 0.2f),
            BorderColor = new Color(color, 0.8f),
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            ContentMarginLeft = 10,
            ContentMarginRight = 10
        };
        button.AddThemeStyleboxOverride("normal", normalStyle);
        
        var hoverStyle = normalStyle.Duplicate() as StyleBoxFlat;
        if (hoverStyle != null)
        {
            hoverStyle.BgColor = new Color(color, 0.3f);
        }
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        var pressedStyle = normalStyle.Duplicate() as StyleBoxFlat;
        if (pressedStyle != null)
        {
            pressedStyle.BgColor = new Color(color, 0.4f);
        }
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
        
        return button;
    }

    private void ConnectSignals()
    {
        // STEP 2: Connect Process Events
        _processManager.ProcessStarted += (processId, slotId) => 
        {
            _statusLabel.Text = $"Started process {processId} in slot {slotId}";
            // ProcessManager will automatically unlock additional slots
        };
        
        _processManager.ProcessEnded += (processId) => 
        {
            _statusLabel.Text = $"Ended process {processId}";
            UpdateUI();
        };
            
        // STEP 3: Connect Slot Events
        _slotManager.SlotStatusChanged += (slotId, status) => 
        {
            UpdateSlotDisplay();
            UpdateUI();
        };
    }
    
    private void UpdateUI()
    {
        // Update button states based on current system state
        var hasActiveProcess = _processManager.GetActiveProcessIds().Any();
        _unloadProcessButton.Disabled = !hasActiveProcess;
        
        var hasAvailableSlot = _slotManager.GetAllSlots().Any(s => 
            s.IsUnlocked && s.Status == SlotStatus.Empty);
        _createProcessButton.Disabled = !hasAvailableSlot;
    }
    
    private void OnCreateProcessPressed()
    {
        // STEP 4: Process Creation Pattern
        var processId = _processManager.CreateProcess("Debug");
        if (processId != null)
        {
            // Start process, which will:
            // 1. Load it into an available slot
            // 2. Trigger slot unlocking
            // 3. Update the UI via events
            _processManager.StartProcess(processId, out _);
        }
    }
    
    private void OnUnloadProcessPressed()
    {
        // STEP 5: Process Cleanup Pattern
        var activeProcess = _processManager.GetActiveProcessIds().FirstOrDefault();
        if (activeProcess != null)
        {
            _processManager.UnloadProcess(activeProcess);
        }
    }

    private void OnReturnPressed()
    {
        // Clean up before signaling
        if (_processManager != null)
        {
            foreach (var processId in _processManager.GetActiveProcessIds().ToList())
            {
                _processManager.UnloadProcess(processId);
            }
        }
        
        EmitSignal(SignalName.SceneUnloadRequested);
    }

    private void UpdateSlotDisplay()
    {
        // STEP 7: Slot Visualization Pattern
        foreach (var child in _slotDisplay.GetChildren())
        {
            child.QueueFree();
        }
        
        var slots = _slotManager.GetAllSlots().ToList();
        var firstActive = slots.FirstOrDefault(s => s.Status == SlotStatus.Active);
        bool hasActiveSlot = firstActive != null;
        
        foreach (var slot in slots)
        {
            // Use appropriate symbol for slot state
            string symbol = !slot.IsUnlocked ? "⚿" :  // Locked
                          slot.Status == SlotStatus.Active ? "■" :  // Active
                          "□";  // Empty but unlocked
            
            var slotLabel = new Label();
            slotLabel.AddThemeFontOverride("font", _monospaceFont);

            if (hasActiveSlot)
            {
                if (slot == firstActive)
                {
                    // Root of the tree
                    slotLabel.Text = $"└── {symbol} [{(slot.CurrentProcess?.Type ?? "").PadRight(10)}]";
                }
                else if (slot.IsUnlocked)
                {
                    // Branch (indented and with tree structure)
                    slotLabel.Text = $"    ├── {symbol} [{(slot.CurrentProcess?.Type ?? "").PadRight(10)}]";
                }
                else continue;  // Skip locked slots in tree view
            }
            else
            {
                // No active process, flat display
                slotLabel.Text = $"└── {symbol} [{(slot.CurrentProcess?.Type ?? "").PadRight(10)}]";
            }
            
            _slotDisplay.AddChild(slotLabel);
        }
    }
}