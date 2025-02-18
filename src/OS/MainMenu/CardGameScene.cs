// src/OS/MainMenu/CardGameScene.cs
using Godot;

namespace Trivale.OS.MainMenu;

/// <summary>
/// Represents a loadable scene in the main menu system.
/// 
/// Scene Contract:
/// 1. Must emit SceneUnloadRequested signal when it wants to be unloaded
/// 2. Should not attempt to unload itself or modify its parent viewport
/// 3. Can expect its QueueFree() to be called by the parent system
/// 4. Should clean up its own internal resources in _ExitTree if needed
/// 
/// The actual unloading, process management, and UI state restoration
/// is handled by the parent SimpleMainMenu system.
/// </summary>
public partial class CardGameScene : Control
{
    [Signal]
    public delegate void SceneUnloadRequestedEventHandler();

    public override void _Ready()
    {
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

    private void OnReturnPressed()
    {
        EmitSignal(SignalName.SceneUnloadRequested);
    }
}