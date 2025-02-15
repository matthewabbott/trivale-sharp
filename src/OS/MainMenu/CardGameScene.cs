// src/OS/MainMenu/CardGameScene.cs
using Godot;
using System;

namespace Trivale.Scenes;

public partial class CardGameScene : Control
{
    public override void _Ready()
    {
        var label = new Label
        {
            Text = "Card Game Placeholder",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AnchorsPreset = (int)LayoutPreset.Center
        };
        AddChild(label);
    }
}