// src/Tests/ProcessManagementTest.cs
using Godot;
using System;
using Trivale.Memory.ProcessManagement;
using Trivale.Memory.SlotManagement;

namespace Trivale.Tests;

public partial class ProcessManagementTest : Control
{
    private ISlotManager _slotManager;
    private IProcessManager _processManager;
    private Label _statusLabel;
    private VBoxContainer _slotDisplay;
    private Button _createProcessButton;
    
    public override void _Ready()
    {
        // Create managers
        _slotManager = new SlotManager(2, 2);  // 2x2 grid of slots
        _processManager = new ProcessManager(_slotManager);
        
        SetupUI();
        ConnectSignals();
    }
    
    private void SetupUI()
    {
        // Basic vertical layout
        var layout = new VBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1
        };
        AddChild(layout);
        
        // Status label at top
        _statusLabel = new Label { Text = "Process Management Test" };
        layout.AddChild(_statusLabel);
        
        // Create process button
        _createProcessButton = new Button { Text = "Create Debug Process" };
        layout.AddChild(_createProcessButton);
        
        // Simple slot display
        _slotDisplay = new VBoxContainer();
        layout.AddChild(_slotDisplay);
        
        UpdateSlotDisplay();
    }
    
    private void ConnectSignals()
    {
        _createProcessButton.Pressed += OnCreateProcessPressed;
        
        // Listen for slot changes
        _slotManager.SlotStatusChanged += (slotId, status) => 
        {
            GD.Print($"Slot {slotId} status changed to {status}");
            UpdateSlotDisplay();
        };
        
        // Listen for process events
        _processManager.ProcessStarted += (processId, slotId) => 
            GD.Print($"Process {processId} started in slot {slotId}");
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
    
    private void UpdateSlotDisplay()
    {
        // Clear existing display
        foreach (var child in _slotDisplay.GetChildren())
        {
            child.QueueFree();
        }
        
        // Show each slot's status
        foreach (var slot in _slotManager.GetAllSlots())
        {
            var slotLabel = new Label
            {
                Text = $"Slot {slot.Id}: {slot.Status}" + 
                      (slot.CurrentProcess != null ? $" - {slot.CurrentProcess.Type}" : "")
            };
            _slotDisplay.AddChild(slotLabel);
        }
    }
}