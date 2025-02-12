// src/UI/Components/MemoryGridView.cs
using Godot;
using System;
using Trivale.Memory;
using Trivale.OS;

namespace Trivale.UI.Components;

public partial class MemoryGridView : VBoxContainer
{
    private GridContainer _gridContainer;
    private ProcessManager _processManager;
    
    [Signal]
    public delegate void MemorySlotSelectedEventHandler(string slotId);
    
    public override void _Ready()
    {
        SetupLayout();
    }
    
    public void Initialize(ProcessManager processManager)
    {
        _processManager = processManager;
        UpdateDisplay();
    }
    
    private void SetupLayout()
    {
        // Title label at top
        var label = new Label { Text = "SYSTEM MEMORY" };
        AddChild(label);
        
        // Grid for memory slots
        _gridContainer = new GridContainer
        {
            SizeFlagsVertical = SizeFlags.Fill,
            Columns = TerminalConfig.Layout.MemSlotColumns,
            MouseFilter = MouseFilterEnum.Pass
        };
        AddChild(_gridContainer);
        
        // Set this container to pass through mouse events
        MouseFilter = MouseFilterEnum.Pass;
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouse mouseEvent)
        {
            GD.Print($"MemoryGridView received mouse event: {mouseEvent.GetType()}");
        }
    }

    private Button CreateSlotButton(IMemorySlot slot)
    {
        var button = new Button
        {
            Text = $"MEM_{slot.Id}",
            CustomMinimumSize = new Vector2(120, 80),
            TooltipText = $"Memory: {slot.MemoryUsage:P0}\nCPU: {slot.CpuUsage:P0}",
            MouseFilter = MouseFilterEnum.Stop,  // Ensure button captures mouse input
            FocusMode = FocusModeEnum.All  // Allow button to receive focus
        };
        
        // Add hover debug
        button.MouseEntered += () => GD.Print($"Mouse entered button for slot {slot.Id}");
        button.MouseExited += () => GD.Print($"Mouse exited button for slot {slot.Id}");
        
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
        
        // Add hover state style
        var hoverStyle = style.Duplicate() as StyleBoxFlat;
        if (hoverStyle != null)
        {
            hoverStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            hoverStyle.BorderColor = TerminalConfig.Colors.Border;
        }
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        button.Pressed += () => {
            GD.Print($"MEM slot {slot.Id} clicked");
            EmitSignal(SignalName.MemorySlotSelected, slot.Id);
        };
        
        return button;
    }
    
    public void UpdateDisplay()
    {
        if (_processManager == null || _gridContainer == null) return;
        
        // Clear existing buttons
        foreach (var child in _gridContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Create new buttons for each slot
        var slots = _processManager.GetAllSlots();
        foreach (var slot in slots)
        {
            var button = CreateSlotButton(slot);
            _gridContainer.AddChild(button);
        }
    }
}