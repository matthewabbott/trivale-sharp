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
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0.0f, 0.1f, 0.0f, 0.8f),
            BorderColor = UIThemeManager.Instance.PrimaryColor,
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            ContentMarginLeft = 20,
            ContentMarginRight = 20,
            ContentMarginTop = 20,
            ContentMarginBottom = 20,
            ShadowColor = new Color(0, 0, 0, 0.5f),
            ShadowSize = 4,
            ShadowOffset = new Vector2(2, 2)
        };

        var button = new Button
        {
            LayoutMode = 1,
            CustomMinimumSize = new Vector2(220, 220),
            Theme = _defaultTheme
        };
        button.AddThemeStyleboxOverride("normal", style);

        var container = new VBoxContainer
        {
            LayoutMode = 1,
            CustomMinimumSize = new Vector2(200, 200)
        };
        button.AddChild(container);
        
        // Add some top padding
        container.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
        
        var icon = new RichTextLabel
        {
            LayoutMode = 1,
            BbcodeEnabled = true,
            Text = $"[center]{program.AsciiArt}[/center]",
            CustomMinimumSize = new Vector2(0, 100),
            SizeFlagsHorizontal = SizeFlags.Fill,
            ScrollActive = false
        };
        container.AddChild(icon);
        
        var name = new Label
        {
            Text = program.Name,
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 40)
        };
        container.AddChild(name);
        
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