// src/OS/MainMenu/DebugScene.cs
using Godot;

namespace Trivale.OS.MainMenu;

public partial class DebugScene : Control
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
            Text = "Debug Sandbox",
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