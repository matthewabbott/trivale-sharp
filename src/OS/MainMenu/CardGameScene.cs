// src/OS/MainMenu/CardGameScene.cs
using Godot;

namespace Trivale.OS.MainMenu;

/// <summary>
/// Represents a loadable card game scene in the main menu system.
/// 
/// Scene Contract:
/// 1. Receives SceneOrchestrator reference via SetOrchestrator method
/// 2. Uses direct method call for requesting unload instead of signals
/// 3. Stores and retrieves its process ID from metadata
/// 4. Does not attempt to unload itself or modify its parent viewport
/// </summary>
public partial class CardGameScene : Control
{
    /// <summary>
    /// Reference to the SceneOrchestrator for direct method calls
    /// This is more reliable than signal-based communication
    /// </summary>
    private SceneOrchestrator _orchestrator;
    
    /// <summary>
    /// Sets the SceneOrchestrator reference used for direct method calls
    /// Called by SceneOrchestrator during scene initialization
    /// </summary>
    /// <param name="orchestrator">The SceneOrchestrator instance</param>
    public void SetOrchestrator(SceneOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public override void _Ready()
    {
        // Setup UI layout
        var layout = new VBoxContainer
        {
            AnchorsPreset = (int)LayoutPreset.Center,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both
        };
        AddChild(layout);

        // Title
        var title = new Label
        {
            Text = "Card Game Placeholder",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        layout.AddChild(title);

        // Return button
        var returnButton = new Button
        {
            Text = "Return to Menu",
            CustomMinimumSize = new Vector2(200, 40)
        };
        returnButton.Pressed += OnReturnPressed;
        layout.AddChild(returnButton);
    }

    /// <summary>
    /// Handles the return button press by requesting scene unload
    /// Uses direct method call to SceneOrchestrator instead of signals
    /// </summary>
    private void OnReturnPressed()
    {
        if (_orchestrator != null)
        {
            // Get process ID from metadata
            // This was set by SceneOrchestrator during initialization
            string processId = null;
            if (HasMeta("ProcessId"))
            {
                processId = (string)GetMeta("ProcessId");
            }
            
            // Use direct method call instead of signal
            _orchestrator.RequestSceneUnload(processId);
        }
        else
        {
            GD.PrintErr("CardGameScene: Orchestrator not set, can't request unload");
        }
    }
}