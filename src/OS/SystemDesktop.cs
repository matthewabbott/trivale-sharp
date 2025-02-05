// src/OS/SystemDesktop.cs

using Godot;
using System;
using System.Collections.Generic;

namespace Trivale.OS;

public partial class SystemDesktop : Control
{
    private VBoxContainer _mainDisplay;
    private RichTextLabel _systemStatus;
    private GridContainer _programGrid;
    private Control _windowLayer;
    private Panel _background;
    
    private Theme _defaultTheme;
    
    // Keep track of available programs
    private Dictionary<string, ProgramInfo> _programs = new();
    
    public override void _Ready()
    {
        LoadResources();
        SetupLayout();
        PopulatePrograms();
        UpdateSystemStatus();
    }
    
    private void LoadResources()
    {
        UIThemeManager.Instance.Initialize();
        _defaultTheme = UIThemeManager.Instance.CreateTheme();
    }
    
    private void SetupLayout()
    {
        // Full screen background
        _background = new Panel
        {
            LayoutMode = 1, // 1 is anchors
            AnchorsPreset = (int)LayoutPreset.FullRect,
            SizeFlagsHorizontal = SizeFlags.Fill,
            SizeFlagsVertical = SizeFlags.Fill
        };
        
        var bgStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.0f, 0.05f, 0.0f, 1.0f), // Very dark green
            BorderWidthBottom = 0,
            BorderWidthLeft = 0,
            BorderWidthRight = 0,
            BorderWidthTop = 0
        };
        _background.AddThemeStyleboxOverride("panel", bgStyle);
        AddChild(_background);
        
        // Main content container with margins
        var marginContainer = new MarginContainer
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            Theme = _defaultTheme
        };
        marginContainer.AddThemeConstantOverride("margin_left", 40);
        marginContainer.AddThemeConstantOverride("margin_right", 40);
        marginContainer.AddThemeConstantOverride("margin_top", 20);
        marginContainer.AddThemeConstantOverride("margin_bottom", 20);
        AddChild(marginContainer);
        
        // Main layout container
        _mainDisplay = new VBoxContainer
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            Theme = _defaultTheme
        };
        marginContainer.AddChild(_mainDisplay);
        
        // System status bar at top
        _systemStatus = new RichTextLabel
        {
            LayoutMode = 1,
            BbcodeEnabled = true,
            CustomMinimumSize = new Vector2(0, 60), // Two lines of ASCII
            Theme = _defaultTheme,
            SizeFlagsHorizontal = SizeFlags.Fill,
            ScrollActive = false // Disable scrolling
        };
        _mainDisplay.AddChild(_systemStatus);
        
        // Add some spacing
        _mainDisplay.AddChild(new Control { CustomMinimumSize = new Vector2(0, 40) });
        
        // Program grid with centering
        var gridCenter = new CenterContainer
        {
            LayoutMode = 1,
            SizeFlagsHorizontal = SizeFlags.Fill,
            SizeFlagsVertical = SizeFlags.Fill
        };
        _mainDisplay.AddChild(gridCenter);
        
        _programGrid = new GridContainer
        {
            LayoutMode = 1,
            Columns = 4,
            CustomMinimumSize = new Vector2(800, 0)
        };
        gridCenter.AddChild(_programGrid);
        
        // Window layer
        _windowLayer = new Control
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(_windowLayer);
    }
    
    private void PopulatePrograms()
    {
        AddProgram(new ProgramInfo
        {
            Name = "PUZZLE_MODE.EXE",
            Description = "Security System Testing Suite",
            AsciiArt = UIThemeManager.Instance.GetProgramIcon("PUZZLE_MODE.EXE"),
            IconPath = UIThemeManager.Instance.GetProgramIcon("PUZZLE_MODE.EXE", UIStyle.Modern)
        });
        
        AddProgram(new ProgramInfo
        {
            Name = "SETTINGS.EXE",
            Description = "System Configuration",
            AsciiArt = UIThemeManager.Instance.GetProgramIcon("SETTINGS.EXE"),
            IconPath = UIThemeManager.Instance.GetProgramIcon("SETTINGS.EXE", UIStyle.Modern)
        });
        
        foreach (var program in _programs.Values)
        {
            var button = CreateProgramButton(program);
            _programGrid.AddChild(button);
        }
    }
    
    private Button CreateProgramButton(ProgramInfo program)
    {
        // Normal state - dark background, white text
        var styleNormal = new StyleBoxFlat
        {
            BgColor = new Color(0.1f, 0.1f, 0.1f, 0.95f), // Very dark gray, almost black
            BorderColor = new Color(0.0f, 0.8f, 0.0f, 0.7f), // Softer green
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            ContentMarginLeft = 20,
            ContentMarginRight = 20,
            ContentMarginTop = 20,
            ContentMarginBottom = 20
        };
        
        // Hover state - same dark background, but glowing border
        var styleHover = new StyleBoxFlat
        {
            BgColor = new Color(0.12f, 0.12f, 0.12f, 0.95f), // Slightly lighter
            BorderColor = new Color(0.0f, 1.0f, 0.0f, 0.9f), // Brighter green
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            ContentMarginLeft = 20,
            ContentMarginRight = 20,
            ContentMarginTop = 20,
            ContentMarginBottom = 20
        };
        
        // Pressed state
        var stylePressed = new StyleBoxFlat
        {
            BgColor = new Color(0.15f, 0.15f, 0.15f, 0.95f), // Even lighter when pressed
            BorderColor = new Color(0.0f, 1.0f, 0.0f, 1.0f), // Full bright green
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            ContentMarginLeft = 20,
            ContentMarginRight = 20,
            ContentMarginTop = 20,
            ContentMarginBottom = 20
        };

        var button = new Button
        {
            LayoutMode = 1,
            CustomMinimumSize = new Vector2(220, 220),
            Theme = _defaultTheme
        };
        
        button.AddThemeStyleboxOverride("normal", styleNormal);
        button.AddThemeStyleboxOverride("hover", styleHover);
        button.AddThemeStyleboxOverride("pressed", stylePressed);

        var container = new VBoxContainer
        {
            LayoutMode = 1,
            CustomMinimumSize = new Vector2(200, 200)
        };
        button.AddChild(container);
        
        var icon = new RichTextLabel
        {
            LayoutMode = 1,
            BbcodeEnabled = true,
            Text = $"[center]{program.AsciiArt}[/center]",
            CustomMinimumSize = new Vector2(0, 100),
            SizeFlagsHorizontal = SizeFlags.Fill,
            ScrollActive = false,
            Modulate = Colors.White // Start with white text
        };
        container.AddChild(icon);
        
        var name = new Label
        {
            Text = program.Name,
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 40),
            Modulate = Colors.White // Start with white text
        };
        container.AddChild(name);
        
        // Update colors on hover
        button.MouseEntered += () => {
            icon.Modulate = new Color(0.0f, 1.0f, 0.0f, 1.0f); // Bright green
            name.Modulate = new Color(0.0f, 1.0f, 0.0f, 1.0f); // Bright green
        };
        button.MouseExited += () => {
            icon.Modulate = Colors.White; // Back to white
            name.Modulate = Colors.White; // Back to white
        };
        
        button.Pressed += () => OnProgramClicked(program.Name);
        
        return button;
    }
    
    private void AddProgram(ProgramInfo program)
    {
        _programs[program.Name] = program;
    }
    
    private void OnProgramClicked(string programName)
    {
        GD.Print($"Launching {programName}...");
        // TODO: Create program window
    }
    
    private void UpdateSystemStatus()
    {
        var statusBox = AsciiStyle.CreateBox(120, 2, "SYSTEM STATUS");
        string status = $"PROGRAMS: {_programs.Count} | MEM: {AsciiStyle.CreateBar(0.64f, 20)} | SEC: {AsciiStyle.CreateBar(0.75f, 10)}";
        
        _systemStatus.Text = string.Join("\n", statusBox);
        _systemStatus.Text += $"\n{AsciiStyle.VERTICAL} {status.PadRight(118)} {AsciiStyle.VERTICAL}";
    }
}