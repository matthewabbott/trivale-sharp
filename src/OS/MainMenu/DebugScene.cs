// src/OS/MainMenu/DebugScene.cs
using Godot;
using System;

namespace Trivale.Scenes;

public partial class DebugScene : Control
{
    public override void _Ready()
    {
        var label = new Label
        {
            Text = "Debug Sandbox Placeholder",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AnchorsPreset = (int)LayoutPreset.Center
        };
        AddChild(label);
    }
}