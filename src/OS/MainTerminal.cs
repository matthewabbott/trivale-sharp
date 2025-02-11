// src/OS/MainTerminal.cs
using Godot;
using System;
using System.Collections.Generic;
using Trivale.Memory;

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
    
    // Layout constants
    private const int MARGIN = 20;
    private const int INFO_HEIGHT = 80;
    private const int SLOT_GRID_WIDTH = 250;
    
    public override void _Ready()
    {
        _processManager = new ProcessManager();
        AddChild(_processManager);
        
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
            BgColor = new Color(0.0f, 0.05f, 0.0f, 1.0f),
            BorderColor = new Color(0.0f, 0.3f, 0.0f),
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2
        };
        _background.AddThemeStyleboxOverride("panel", bgStyle);
        AddChild(_background);
        
        // Main margin container
        var marginContainer = new MarginContainer
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect
        };
        marginContainer.AddThemeConstantOverride("margin_left", MARGIN);
        marginContainer.AddThemeConstantOverride("margin_right", MARGIN);
        marginContainer.AddThemeConstantOverride("margin_top", MARGIN);
        marginContainer.AddThemeConstantOverride("margin_bottom", MARGIN);
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
            CustomMinimumSize = new Vector2(SLOT_GRID_WIDTH, 0)
        };
        contentLayout.AddChild(slotContainer);
        
        var slotLabel = new Label { Text = "SYSTEM MEMORY" };
        slotContainer.AddChild(slotLabel);
        
        _memSlotGrid = new GridContainer
        {
            SizeFlagsVertical = SizeFlags.Fill,
            Columns = 2
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
            Size = new Vector2(800, 600),
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
            CustomMinimumSize = new Vector2(0, INFO_HEIGHT)
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
        
        // TODO: Add resource displays (Memory, CPU, etc)
        
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
            BorderColor = new Color(0.0f, 0.8f, 0.0f, 0.7f),
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1
        };
        button.AddThemeStyleboxOverride("normal", style);
        
        button.Pressed += () => OnSlotSelected(slot);
        
        return button;
    }
    
    private void OnSlotSelected(IMemorySlot slot)
    {
        // TODO: Handle slot activation and viewport content switching
        GD.Print($"Selected slot: {slot.Id}");
    }
    
    private void SetupEffects()
    {
        // TODO: Add CRT shader effect to scanlines panel
    }
    
    private void UpdateResourceDisplay()
    {
        // TODO: Update resource panel with current system stats
    }
    
    public override void _Process(double delta)
    {
        UpdateResourceDisplay();
    }
}