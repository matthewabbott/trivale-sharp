// src/UI/Components/TerminalButton.cs

using Godot;
using System;

namespace Trivale.UI.Components;

public partial class TerminalButton : Button
{
    private RichTextLabel _iconDisplay;
    private Label _textDisplay;
    private VBoxContainer _container;
    
    [Export]
    public string IconText { get; set; } = "";
    
    [Export]
    public string ButtonText { get; set; } = "";
    
    public override void _Ready()
    {
        SetupLayout();
        SetupStyles();
    }
    
    private void SetupLayout()
    {
        CustomMinimumSize = new Vector2(220, 220);
        
        _container = new VBoxContainer
        {
            LayoutMode = 1,
            CustomMinimumSize = new Vector2(200, 200)
        };
        AddChild(_container);
        
        _iconDisplay = new RichTextLabel
        {
            LayoutMode = 1,
            BbcodeEnabled = true,
            Text = $"[center]{IconText}[/center]",
            CustomMinimumSize = new Vector2(0, 100),
            SizeFlagsHorizontal = SizeFlags.Fill,
            ScrollActive = false,
            Modulate = Colors.White
        };
        _container.AddChild(_iconDisplay);
        
        _textDisplay = new Label
        {
            Text = ButtonText,
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 40),
            Modulate = Colors.White
        };
        _container.AddChild(_textDisplay);
        
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }
    
    private void SetupStyles()
    {
        var styleNormal = new StyleBoxFlat
        {
            BgColor = new Color(0.1f, 0.1f, 0.1f, 0.95f),
            BorderColor = new Color(0.0f, 0.8f, 0.0f, 0.7f),
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            ContentMarginLeft = 20,
            ContentMarginRight = 20,
            ContentMarginTop = 20,
            ContentMarginBottom = 20
        };
        
        var styleHover = styleNormal.Duplicate() as StyleBoxFlat;
        if (styleHover != null)
        {
            styleHover.BgColor = new Color(0.12f, 0.12f, 0.12f, 0.95f);
            styleHover.BorderColor = new Color(0.0f, 1.0f, 0.0f, 0.9f);
        }
        
        var stylePressed = styleNormal.Duplicate() as StyleBoxFlat;
        if (stylePressed != null)
        {
            stylePressed.BgColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            stylePressed.BorderColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        }
        
        AddThemeStyleboxOverride("normal", styleNormal);
        AddThemeStyleboxOverride("hover", styleHover);
        AddThemeStyleboxOverride("pressed", stylePressed);
    }
    
    private void OnMouseEntered()
    {
        _iconDisplay.Modulate = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        _textDisplay.Modulate = new Color(0.0f, 1.0f, 0.0f, 1.0f);
    }
    
    private void OnMouseExited()
    {
        _iconDisplay.Modulate = Colors.White;
        _textDisplay.Modulate = Colors.White;
    }
    
    public void UpdateContent(string iconText, string buttonText)
    {
        IconText = iconText;
        ButtonText = buttonText;
        if (_iconDisplay != null)
            _iconDisplay.Text = $"[center]{IconText}[/center]";
        if (_textDisplay != null)
            _textDisplay.Text = ButtonText;
    }
}