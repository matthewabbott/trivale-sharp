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
    private SystemFont _terminalFont;
    
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
            Theme = _defaultTheme
        };
        marginContainer.AddChild(_mainDisplay);
        
        // System status bar at top
        _systemStatus = new RichTextLabel
        {
            BbcodeEnabled = true,
            CustomMinimumSize = new Vector2(0, 60), // Two lines of ASCII
            Theme = _defaultTheme,
            SizeFlagsHorizontal = SizeFlags.Fill
        };
        _mainDisplay.AddChild(_systemStatus);
        
        // Add some spacing
        _mainDisplay.AddChild(new Control { CustomMinimumSize = new Vector2(0, 40) });
        
        // Program grid with centering
        var gridCenter = new CenterContainer
        {
            SizeFlagsHorizontal = SizeFlags.Fill,
            SizeFlagsVertical = SizeFlags.Fill
        };
        _mainDisplay.AddChild(gridCenter);
        
        _programGrid = new GridContainer
        {
            Columns = 4,
            CustomMinimumSize = new Vector2(800, 0)
        };
        gridCenter.AddChild(_programGrid);
        
        // Window layer
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
        
        // Base style for containers
        var styleNormal = new StyleBoxFlat
        {
            BgColor = Colors.Transparent,
            BorderColor = new Color(0, 1, 0, 0.5f) // Semi-transparent green
        };
        
        // Button style
        var buttonNormal = new StyleBoxFlat
        {
            BgColor = new Color(0, 0.15f, 0, 0.8f),
            BorderColor = new Color(0, 1, 0, 0.5f),
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            ContentMarginLeft = 10,
            ContentMarginRight = 10,
            ContentMarginTop = 10,
            ContentMarginBottom = 10
        };
        
        // Set default font
        theme.DefaultFont = _terminalFont;
        theme.DefaultFontSize = 16;
        
        // Add styles to theme
        theme.SetStylebox("normal", "ProgramButton", buttonNormal);
        
        return theme;
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
            AsciiArt = AsciiStyle.SETTINGS_ICON
        });
        
        foreach (var program in _programs.Values)
        {
            var button = CreateProgramButton(program);
            _programGrid.AddChild(button);
        }
    }
    
    private Button CreateProgramButton(ProgramInfo program)
    {
        var button = new Button
        {
            CustomMinimumSize = new Vector2(180, 180),
            Theme = _defaultTheme
        };
        
        var container = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(170, 170)
        };
        button.AddChild(container);
        
        // Add some top padding
        container.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
        
        var icon = new RichTextLabel
        {
            BbcodeEnabled = true,
            Text = $"[center]{program.AsciiArt}[/center]",
            CustomMinimumSize = new Vector2(0, 80),
            SizeFlagsHorizontal = SizeFlags.Fill
        };
        container.AddChild(icon);
        
        var name = new Label
        {
            Text = program.Name,
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 30)
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