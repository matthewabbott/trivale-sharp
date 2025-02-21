// src/OS/MainMenu/DebugScene.cs

using Godot;
using System.Linq;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;
using Trivale.UI.Components;

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
    private HBoxContainer _buttonContainer;
    private Button _createProcessButton;
    private Button _unloadProcessButton;
    private Button _addSlotButton;     // New button for adding slots
    private Button _removeSlotButton;  // New button for removing slots
    private SlotGridSystem _slotGridSystem;
    private SlotGridDisplay _slotDisplay;
    
    public void Initialize(IProcessManager processManager, ISlotManager slotManager)
    {
        _processManager = processManager;
        _slotManager = slotManager;
    }

    public override void _Ready()
    {
        // Clear any existing children
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
        
        SetupUI();
        ConnectSignals();
    }

    private Button _createNewSlotButton; // New button

    private void SetupUI()
    {
        // Main vertical layout
        var layout = new VBoxContainer
        {
            AnchorsPreset = (int)LayoutPreset.Center,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both
        };
        AddChild(layout);

        // Title and status
        var title = new Label
        {
            Text = "Debug Sandbox",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        layout.AddChild(title);
        
        _statusLabel = new Label
        {
            Text = "MEM Slot System Debug",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        layout.AddChild(_statusLabel);
        
        // Button container
        _buttonContainer = new HBoxContainer
        {
            CustomMinimumSize = new Vector2(0, 40)
        };
        layout.AddChild(_buttonContainer);
        
        // Create a second button container for less important buttons
        var secondaryButtonContainer = new HBoxContainer
        {
            CustomMinimumSize = new Vector2(0, 40)
        };
        layout.AddChild(secondaryButtonContainer);
        
        // Main control buttons with proper styling
        _createProcessButton = CreateStyledButton("Load Debug Process", Colors.Green);
        _unloadProcessButton = CreateStyledButton("Unload Process", Colors.Red);
        var returnButton = CreateStyledButton("Return to Menu", Colors.White);
        
        _buttonContainer.AddChild(_createProcessButton);
        _buttonContainer.AddChild(_unloadProcessButton);
        _buttonContainer.AddChild(returnButton);
        
        // Slot management buttons in second row
        _addSlotButton = CreateStyledButton("Unlock Slot", Colors.Blue);
        _removeSlotButton = CreateStyledButton("Lock Slot", Colors.Orange);
        _createNewSlotButton = CreateStyledButton("Create New Slot", Colors.Purple);
        
        secondaryButtonContainer.AddChild(_addSlotButton);
        secondaryButtonContainer.AddChild(_removeSlotButton);
        secondaryButtonContainer.AddChild(_createNewSlotButton);
        
        // Connect signals
        _createProcessButton.Pressed += OnCreateProcessPressed;
        _unloadProcessButton.Pressed += OnUnloadProcessPressed;
        _addSlotButton.Pressed += OnAddSlotPressed;
        _removeSlotButton.Pressed += OnRemoveSlotPressed;
        _createNewSlotButton.Pressed += OnCreateNewSlotPressed;
        returnButton.Pressed += OnReturnPressed;
        
        // No need for slot display system here - using main menu's display
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
            UpdateUI();
        };
        
        _slotManager.SlotUnlocked += (slotId) =>
        {
            _statusLabel.Text = $"Slot {slotId} unlocked";
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
        
        // Update Remove Slot button state - can only remove if there are unlocked slots
        var hasUnlockedSlots = _slotManager.GetAllSlots().Any(s => s.IsUnlocked);
        _removeSlotButton.Disabled = !hasUnlockedSlots;
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
    
    private void OnAddSlotPressed()
    {
        // Find the first locked slot
        var slots = _slotManager.GetAllSlots();
        var lockedSlot = slots.FirstOrDefault(s => !s.IsUnlocked);
        
        if (lockedSlot != null)
        {
            _slotManager.UnlockSlot(lockedSlot.Id);
            _statusLabel.Text = $"Unlocked slot {lockedSlot.Id}";
        }
        else
        {
            _statusLabel.Text = "No more locked slots available";
        }
        
        UpdateUI();
    }
    
    private void OnRemoveSlotPressed()
    {
        // Find the last unlocked empty slot
        var slots = _slotManager.GetAllSlots();
        var slotToRemove = slots.LastOrDefault(s => 
            s.IsUnlocked && s.Status == SlotStatus.Empty);
        
        if (slotToRemove != null)
        {
            // We need to cast to SlotManager to access the internal LockSlot method
            // If you don't have such a method, you'll need to add it to your SlotManager class
            if (_slotManager is SlotManager manager)
            {
                if (manager.LockSlot(slotToRemove.Id))
                {
                    _statusLabel.Text = $"Locked slot {slotToRemove.Id}";
                }
                else
                {
                    _statusLabel.Text = $"Could not lock slot {slotToRemove.Id}";
                }
            }
            else
            {
                _statusLabel.Text = "Cannot access slot manager lock function";
            }
        }
        else
        {
            _statusLabel.Text = "No empty slots to remove";
        }
        
        UpdateUI();
    }
    
    private void OnCreateNewSlotPressed()
    {
        // We need to cast to SlotManager to access the CreateNewSlot method
        if (_slotManager is SlotManager manager)
        {
            // Create a new slot (unlocked or locked)
            bool createUnlocked = true; // Set to false if you want new slots to start locked
            string newSlotId = manager.CreateNewSlot(createUnlocked);
            
            if (!string.IsNullOrEmpty(newSlotId))
            {
                string state = createUnlocked ? "unlocked" : "locked";
                _statusLabel.Text = $"Created new {state} slot {newSlotId}";
            }
            else
            {
                _statusLabel.Text = "Failed to create new slot";
            }
        }
        else
        {
            _statusLabel.Text = "Cannot access slot manager creation function";
        }
        
        UpdateUI();
    }

    private void OnReturnPressed()
    {
        // Clean up ALL active processes before leaving
        if (_processManager != null)
        {
            foreach (var processId in _processManager.GetActiveProcessIds())
            {
                _processManager.UnloadProcess(processId);
            }
        }
        
        EmitSignal(SignalName.SceneUnloadRequested);
    }
}