// src/OS/MainMenu/MainMenuScene.cs
using Godot;

namespace Trivale.OS.MainMenu;

/// <summary>
/// Implements the main menu UI as a standalone scene.
/// Responsible for presenting menu options and emitting signals when options are selected.
/// </summary>
public partial class MainMenuScene : Control, IOrchestratableScene
{
    [Signal]
    public delegate void MenuOptionSelectedEventHandler(string scenePath, string processType);

    private VBoxContainer _buttonContainer;
    private Button _cardGameButton;
    private Button _debugButton;
    private SceneOrchestrator _orchestrator;

    public void SetOrchestrator(SceneOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public string GetProcessId()
    {
        return HasMeta("ProcessId") ? (string)GetMeta("ProcessId") : null;
    }

    public override void _Ready()
    {
        SetupMenuButtons();
    }

    private void SetupMenuButtons()
    {
        _buttonContainer = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(250, 0),
            SizeFlagsHorizontal = Control.SizeFlags.Fill,
            SizeFlagsVertical = Control.SizeFlags.Fill,
        };
        _buttonContainer.AddThemeConstantOverride("separation", 20);
        AddChild(_buttonContainer);

        // Title
        var titleLabel = new Label
        {
            Text = "NETRUNNER OS",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 40)
        };
        _buttonContainer.AddChild(titleLabel);

        // Spacer
        var spacer = new Control { CustomMinimumSize = new Vector2(0, 20) };
        _buttonContainer.AddChild(spacer);

        // Menu buttons
        _cardGameButton = CreateStyledButton("CARD GAME", Colors.Green);
        _buttonContainer.AddChild(_cardGameButton);

        _debugButton = CreateStyledButton("DEBUG SANDBOX", Colors.Orange);
        _buttonContainer.AddChild(_debugButton);
        
        // Bottom spacer
        var bottomSpacer = new Control { SizeFlagsVertical = Control.SizeFlags.Expand };
        _buttonContainer.AddChild(bottomSpacer);

        // Connect signals
        _cardGameButton.Pressed += OnCardGamePressed;
        _debugButton.Pressed += OnDebugPressed;
    }

    // Add these new methods for button handlers
    private void OnCardGamePressed()
    {
        if (_orchestrator != null)
        {
            _orchestrator.LoadScene("CardGame", "res://Scenes/MainMenu/CardGameScene.tscn");
        }
        else
        {
            // Fallback to signal for backward compatibility
            EmitSignal(SignalName.MenuOptionSelected, 
                "res://Scenes/MainMenu/CardGameScene.tscn", "CardGame");
        }
    }

    private void OnDebugPressed()
    {
        if (_orchestrator != null)
        {
            _orchestrator.LoadScene("Debug", "res://Scenes/MainMenu/DebugScene.tscn");
        }
        else
        {
            // Fallback to signal for backward compatibility
            EmitSignal(SignalName.MenuOptionSelected, 
                "res://Scenes/MainMenu/DebugScene.tscn", "Debug");
        }
    }

    private Button CreateStyledButton(string text, Color accentColor)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(0, 50),  // Taller buttons
            SizeFlagsHorizontal = Control.SizeFlags.Fill,    // Fill width
            SizeFlagsVertical = Control.SizeFlags.Fill      // Fill height
        };
        
        // Normal state
        var normalStyle = new StyleBoxFlat
        {
            BgColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),  // Dark background
            BorderColor = accentColor,                    // Accent color border
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            ContentMarginLeft = 15,
            ContentMarginRight = 15,
            ContentMarginTop = 10,
            ContentMarginBottom = 10
        };
        button.AddThemeStyleboxOverride("normal", normalStyle);
        
        // Hover state
        var hoverStyle = normalStyle.Duplicate() as StyleBoxFlat;
        if (hoverStyle != null)
        {
            hoverStyle.BgColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);  // Slightly lighter
            hoverStyle.BorderWidthBottom = 3;
            hoverStyle.BorderWidthLeft = 3;
            hoverStyle.BorderWidthRight = 3;
            hoverStyle.BorderWidthTop = 3;
        }
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        
        // Pressed state
        var pressedStyle = normalStyle.Duplicate() as StyleBoxFlat;
        if (pressedStyle != null)
        {
            pressedStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);  // Even lighter when pressed
            pressedStyle.BorderColor = new Color(accentColor, 1.0f);   // Full brightness border
        }
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
        
        return button;
    }

    public override void _ExitTree()
    {
        // Disconnect button signals to prevent memory leaks or disposed object access
        if (_cardGameButton != null)
        {
            _cardGameButton.Pressed -= OnCardGamePressed;
        }
        
        if (_debugButton != null)
        {
            _debugButton.Pressed -= OnDebugPressed;
        }
        
        base._ExitTree();
    }
}