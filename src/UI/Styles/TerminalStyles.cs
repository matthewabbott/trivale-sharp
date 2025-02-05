// src/UI/Styles/TerminalStyles.cs

using Godot;

namespace Trivale.UI.Styles;

public static class TerminalStyles
{
    public static class Colors
    {
        public static readonly Color PrimaryGreen = new(0.0f, 1.0f, 0.0f, 1.0f);
        public static readonly Color DimGreen = new(0.0f, 0.8f, 0.0f, 0.7f);
        public static readonly Color DarkBackground = new(0.1f, 0.1f, 0.1f, 0.95f);
        public static readonly Color DarkBackgroundHover = new(0.12f, 0.12f, 0.12f, 0.95f);
        public static readonly Color DarkBackgroundPressed = new(0.15f, 0.15f, 0.15f, 0.95f);
    }
    
    public static StyleBoxFlat CreateTerminalStyleBox(
        Color bgColor,
        Color borderColor,
        int borderWidth = 1,
        int contentMargin = 20)
    {
        return new StyleBoxFlat
        {
            BgColor = bgColor,
            BorderColor = borderColor,
            BorderWidthBottom = borderWidth,
            BorderWidthLeft = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthTop = borderWidth,
            ContentMarginLeft = contentMargin,
            ContentMarginRight = contentMargin,
            ContentMarginTop = contentMargin,
            ContentMarginBottom = contentMargin
        };
    }
    
    public static Theme CreateTerminalTheme()
    {
        var theme = new Theme();
        // Add common theme overrides here
        return theme;
    }
}