// src/OS/UIThemeManager.cs

using Godot;
using System;

namespace Trivale.OS;

public enum UIStyle
{
    ASCII,
    Modern
}

public partial class UIThemeManager : Node
{
    private static UIThemeManager _instance;
    public static UIThemeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UIThemeManager();
            }
            return _instance;
        }
    }
    
    public UIStyle CurrentStyle { get; private set; } = UIStyle.ASCII;
    
    // Color scheme
    public Color PrimaryColor { get; private set; } = new Color(0, 1, 0, 1); // Green
    public Color BackgroundColor { get; private set; } = new Color(0, 0.05f, 0, 1);
    public Color TextColor { get; private set; } = new Color(0, 1, 0, 1);
    
    private SystemFont _asciiFont;
    private SystemFont _modernFont;
    
    public void Initialize()
    {
        LoadFonts();
    }
    
    private void LoadFonts()
    {
        _asciiFont = new SystemFont();
        _asciiFont.FontNames = new string[] { "JetBrainsMono-Regular" };
        
        _modernFont = new SystemFont();
        _modernFont.FontNames = new string[] { "Arial" }; // Can change this later
    }
    
    public Theme CreateTheme(UIStyle style = UIStyle.ASCII)
    {
        var theme = new Theme();
        
        switch (style)
        {
            case UIStyle.ASCII:
                return CreateAsciiTheme(theme);
            case UIStyle.Modern:
                return CreateModernTheme(theme);
            default:
                return CreateAsciiTheme(theme);
        }
    }
    
    private Theme CreateAsciiTheme(Theme theme)
    {
        // Base style for containers
        var styleNormal = new StyleBoxFlat
        {
            BgColor = Colors.Transparent,
            BorderColor = PrimaryColor
        };
        
        // Button style
        var buttonNormal = new StyleBoxFlat
        {
            BgColor = new Color(0, 0.15f, 0, 0.8f),
            BorderColor = PrimaryColor,
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            ContentMarginLeft = 10,
            ContentMarginRight = 10,
            ContentMarginTop = 10,
            ContentMarginBottom = 10
        };
        
        // Hide scrollbars
        var emptyStyle = new StyleBoxEmpty();
        
        theme.DefaultFont = _asciiFont;
        theme.DefaultFontSize = 16;
        
        theme.SetStylebox("normal", "ProgramButton", buttonNormal);
        
        // Remove scrollbar styling
        theme.SetStylebox("normal", "VScrollBar", emptyStyle);
        theme.SetStylebox("normal", "HScrollBar", emptyStyle);
        theme.SetStylebox("hover", "VScrollBar", emptyStyle);
        theme.SetStylebox("hover", "HScrollBar", emptyStyle);
        theme.SetStylebox("pressed", "VScrollBar", emptyStyle);
        theme.SetStylebox("pressed", "HScrollBar", emptyStyle);
        theme.SetConstant("scroll_speed", "VScrollBar", 0);
        theme.SetConstant("scroll_speed", "HScrollBar", 0);
        
        return theme;
    }
    
    private Theme CreateModernTheme(Theme theme)
    {
        // TODO: Implement modern theme
        return CreateAsciiTheme(theme); // Fallback for now
    }
    
    public void SetStyle(UIStyle style)
    {
        CurrentStyle = style;
        // Emit signal for style change if needed
    }
    
    public string GetProgramIcon(string programName, UIStyle style = UIStyle.ASCII)
    {
        if (style == UIStyle.ASCII)
        {
            return programName switch
            {
                "PUZZLE_MODE.EXE" => AsciiStyle.PROGRAM_ICON,
                "SETTINGS.EXE" => AsciiStyle.SETTINGS_ICON,
                _ => AsciiStyle.PROGRAM_ICON
            };
        }
        
        // Return path to graphical icon when in modern mode
        return $"res://assets/icons/{programName.ToLower()}.png";
    }
}