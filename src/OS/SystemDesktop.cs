// src/OS/SystemDesktop.cs

using Godot;
using System;
using System.Collections.Generic;

namespace Trivale.OS;

public partial class SystemDesktop : Control
{
    private VBoxContainer _mainDisplay;
    private Label _systemStatus;
    private GridContainer _programGrid;
    private Control _windowLayer;
    
    // Keep track of available programs
    private Dictionary<string, ProgramInfo> _programs = new();
    
    public override void _Ready()
    {
        SetupLayout();
        PopulatePrograms();
        UpdateSystemStatus();
    }
    
    private void SetupLayout()
    {
        // Main layout container
        _mainDisplay = new VBoxContainer
        {
            AnchorsPreset = (int)LayoutPreset.FullRect,
            Theme = CreateDefaultTheme()
        };
        AddChild(_mainDisplay);
        
        // System status bar at top
        _systemStatus = new Label
        {
            Text = "SYSTEM READY",
            HorizontalAlignment = HorizontalAlignment.Left,
            CustomMinimumSize = new Vector2(0, 30)
        };
        _mainDisplay.AddChild(_systemStatus);
        
        // Program grid
        _programGrid = new GridContainer
        {
            Columns = 4,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.Fill
        };
        _mainDisplay.AddChild(_programGrid);
        
        // Window layer (sits above everything else)
        _windowLayer = new Control
        {
            AnchorsPreset = (int)LayoutPreset.FullRect,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(_windowLayer);
    }
    
    private Theme CreateDefaultTheme()
    {
        var theme = new Theme();
        
        // Create "hacker" style
        var styleNormal = new StyleBoxFlat
        {
            BgColor = new Color(0, 0.1f, 0, 1), // Dark green background
            BorderColor = new Color(0, 1, 0, 1), // Bright green border
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1
        };
        
        // Add styles to theme
        theme.SetStylebox("normal", "ProgramButton", styleNormal);
        
        return theme;
    }
    
    private void PopulatePrograms()
    {
        // Add some test programs
        AddProgram(new ProgramInfo
        {
            Name = "PUZZLE_MODE.EXE",
            Description = "Security System Testing Suite",
            Icon = "🎮" // We can replace these with proper icons later
        });
        
        AddProgram(new ProgramInfo
        {
            Name = "SETTINGS.EXE",
            Description = "System Configuration",
            Icon = "⚙️"
        });
        
        // Add program buttons to grid
        foreach (var program in _programs.Values)
        {
            var button = CreateProgramButton(program);
            _programGrid.AddChild(button);
        }
    }
    
    private Button CreateProgramButton(ProgramInfo program)
    {
        var container = new VBoxContainer();
        
        var icon = new Label
        {
            Text = program.Icon,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        container.AddChild(icon);
        
        var name = new Label
        {
            Text = program.Name,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        container.AddChild(name);
        
        var button = new Button
        {
            CustomMinimumSize = new Vector2(120, 120)
        };
        button.AddChild(container);
        
        // Connect click handler
        button.Pressed += () => OnProgramClicked(program.Name);
        
        return button;
    }
    
    private void AddProgram(ProgramInfo program)
    {
        _programs[program.Name] = program;
    }
    
    private void OnProgramClicked(string programName)
    {
        GD.Print($"Launching {programName}..."); // For now, just print
        // TODO: Create appropriate window for program
    }
    
    private void UpdateSystemStatus()
    {
        // We can add more system info here later
        _systemStatus.Text = $"SYSTEM READY | Programs Loaded: {_programs.Count}";
    }
}

public class ProgramInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}