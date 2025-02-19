// src/Tests/ProcessManagementTest.cs
using Godot;
using System;
using System.Linq;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;

namespace Trivale.Tests;

public partial class ProcessManagementTest : Control
{
    private ISlotManager _slotManager;
    private IProcessManager _processManager;
    private Label _statusLabel;
    private VBoxContainer _slotDisplay;
    private HBoxContainer _buttonContainer;
    
    // Test control buttons
    private Button _createProcessButton;
    private Button _unloadProcessButton;
    private Button _unlockSlotButton;
    private Button _lockSlotButton;
    
    public override void _Ready()
    {
        // Create managers - using a 3x2 grid for more testing space
        _slotManager = new SlotManager(3, 2);  // 6 total slots
        _processManager = new ProcessManager(_slotManager);
        
        SetupUI();
        ConnectSignals();
    }
    
    private void SetupUI()
    {
        // Main vertical layout
        var layout = new VBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1
        };
        AddChild(layout);
        
        // Status label at top
        _statusLabel = new Label { Text = "Process Management Test" };
        layout.AddChild(_statusLabel);
        
        // Button container
        _buttonContainer = new HBoxContainer
        {
            CustomMinimumSize = new Vector2(0, 40)
        };
        layout.AddChild(_buttonContainer);
        
        // Control buttons
        _createProcessButton = CreateStyledButton("Create Debug Process", Colors.Green);
        _unloadProcessButton = CreateStyledButton("Unload Process", Colors.Red);
        _unlockSlotButton = CreateStyledButton("Unlock Next Slot", Colors.Blue);
        _lockSlotButton = CreateStyledButton("Lock Last Slot", Colors.Orange);
        
        _buttonContainer.AddChild(_createProcessButton);
        _buttonContainer.AddChild(_unloadProcessButton);
        _buttonContainer.AddChild(_unlockSlotButton);
        _buttonContainer.AddChild(_lockSlotButton);
        
        // Slot display with monospace font for ASCII art
        _slotDisplay = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(0, 200)
        };
        var font = new SystemFont();
        font.FontNames = new string[] { "JetBrainsMono-Regular", "Consolas", "Courier New" };
        
        layout.AddChild(_slotDisplay);
        
        UpdateSlotDisplay();
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
        
        return button;
    }
    
    private void ConnectSignals()
    {
        _createProcessButton.Pressed += OnCreateProcessPressed;
        _unloadProcessButton.Pressed += OnUnloadProcessPressed;
        _unlockSlotButton.Pressed += OnUnlockSlotPressed;
        _lockSlotButton.Pressed += OnLockSlotPressed;
        
        // Listen for slot changes
        _slotManager.SlotStatusChanged += (slotId, status) => 
        {
            GD.Print($"Slot {slotId} status changed to {status}");
            UpdateSlotDisplay();
        };
        
        // Listen for process events
        _processManager.ProcessStarted += (processId, slotId) => 
        {
            GD.Print($"Process {processId} started in slot {slotId}");
            // Unlock the next two slots when a process starts
            var slots = _slotManager.GetAllSlots();
            int unlockedCount = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsUnlocked && unlockedCount < 2)
                {
                    _slotManager.UnlockSlot(slot.Id);
                    unlockedCount++;
                }
            }
        };
        _processManager.ProcessEnded += (processId) => 
            GD.Print($"Process {processId} ended");
    }
    
    private void OnCreateProcessPressed()
    {
        var processId = _processManager.CreateProcess("Debug");
        if (processId != null)
        {
            if (_processManager.StartProcess(processId, out var slotId))
            {
                _statusLabel.Text = $"Created and started process {processId} in slot {slotId}";
            }
            else
            {
                _statusLabel.Text = $"Failed to start process {processId}";
            }
        }
        else
        {
            _statusLabel.Text = "Failed to create process";
        }
    }
    
    private void OnUnloadProcessPressed()
    {
        var activeProcess = _processManager.GetActiveProcessIds().FirstOrDefault();
        if (activeProcess != null)
        {
            if (_processManager.UnloadProcess(activeProcess))
            {
                _statusLabel.Text = $"Unloaded process {activeProcess}";
            }
            else
            {
                _statusLabel.Text = $"Failed to unload process {activeProcess}";
            }
        }
        else
        {
            _statusLabel.Text = "No active process to unload";
        }
    }
    
    private void OnUnlockSlotPressed()
    {
        var slots = _slotManager.GetAllSlots();
        var nextLocked = slots.FirstOrDefault(s => !s.IsUnlocked);
        if (nextLocked != null)
        {
            if (_slotManager.UnlockSlot(nextLocked.Id))
            {
                _statusLabel.Text = $"Unlocked slot {nextLocked.Id}";
            }
            else
            {
                _statusLabel.Text = $"Failed to unlock slot {nextLocked.Id}";
            }
        }
        else
        {
            _statusLabel.Text = "No more locked slots";
        }
    }
    
    private void OnLockSlotPressed()
    {
        var slots = _slotManager.GetAllSlots();
        var lastUnlocked = slots.LastOrDefault(s => s.IsUnlocked && s.Status == SlotStatus.Empty);
        if (lastUnlocked != null)
        {
            // Note: We don't have a public Lock method, but we could add one if needed
            _statusLabel.Text = $"Locking not implemented yet for slot {lastUnlocked.Id}";
        }
        else
        {
            _statusLabel.Text = "No unlocked empty slots to lock";
        }
    }
    
    private void UpdateSlotDisplay()
    {
        // Clear existing display
        foreach (var child in _slotDisplay.GetChildren())
        {
            child.QueueFree();
        }
        
        // Show each slot's status in a tree-like structure
        bool firstActive = false;
        foreach (var slot in _slotManager.GetAllSlots())
        {
            // If this is an active slot, remember it for the tree structure
            if (slot.Status == SlotStatus.Active && !firstActive)
            {
                firstActive = true;
                var slotLabel = new Label
                {
                    Text = $"└── ■ [{slot.CurrentProcess?.Type.PadRight(6) ?? "      "}]"
                };
                _slotDisplay.AddChild(slotLabel);
            }
            // If we have an active slot, indent the rest
            else if (firstActive)
            {
                var symbol = slot.Status == SlotStatus.Active ? "■" : "□";
                var slotLabel = new Label
                {
                    Text = $"    ├── {symbol} [{(slot.CurrentProcess?.Type ?? "").PadRight(6)}]"
                };
                _slotDisplay.AddChild(slotLabel);
            }
            // Otherwise show normally
            else
            {
                var symbol = slot.Status == SlotStatus.Active ? "■" : "□";
                var slotLabel = new Label
                {
                    Text = $"└── {symbol} [{(slot.CurrentProcess?.Type ?? "").PadRight(6)}]"
                };
                _slotDisplay.AddChild(slotLabel);
            }
        }
    }
}