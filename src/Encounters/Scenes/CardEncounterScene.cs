// src/Encounters/Scenes/CardEncounterScene.cs
using Godot;
using System;
using System.Collections.Generic;
using Trivale.Terminal;
using Trivale.Cards;
using Trivale.OS;

namespace Trivale.Encounters.Scenes;

/// <summary>
/// Scene manager for card-based encounters. Handles the visualization and interaction
/// of card games through terminal windows.
/// </summary>
public partial class CardEncounterScene : EncounterScene
{
    private WindowManager _windowManager;
    private CardTerminalWindow _handWindow;
    private CardTerminalWindow _tableWindow;
    
    public override void Initialize(IEncounter encounter)
    {
        if (!(encounter is CardGameEncounter))
        {
            throw new ArgumentException($"Expected CardGameEncounter, got {encounter.GetType().Name}");
        }
        
        // Find the window manager
        var desktop = GetNode<SystemDesktop>("/root/SystemDesktop");
        _windowManager = desktop.GetNode<WindowManager>("WindowLayer");
        
        base.Initialize(encounter);
        
        CreateWindows();
        UpdateDisplays();
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        
        // Clean up windows
        _handWindow?.QueueFree();
        _tableWindow?.QueueFree();
    }
    
    private void CreateWindows()
    {
        GD.Print($"Creating windows for {Encounter.Id}");
        
        // Create player hand window
        _handWindow = new CardTerminalWindow
        {
            WindowTitle = $"Your Hand - {Encounter.Id}",
            Position = new Vector2(50, 50),
            BorderColor = new Color(0, 1, 0) // Green
        };
        _handWindow.CardSelected += OnCardSelected;
        _windowManager.AddWindow(_handWindow);
        
        // Create table cards window
        _tableWindow = new CardTerminalWindow
        {
            WindowTitle = $"Table - {Encounter.Id}",
            Position = new Vector2(300, 50),
            BorderColor = new Color(0, 0.7f, 1) // Cyan
        };
        _windowManager.AddWindow(_tableWindow);
    }
    
    private void UpdateDisplays()
    {
        var cardEncounter = (CardGameEncounter)Encounter;
        var state = cardEncounter.GetState();
        
        if (state.TryGetValue("hand", out var handObj) && handObj is List<Card> hand)
        {
            _handWindow?.DisplayCards(hand, "Your Hand:");
        }
        
        if (state.TryGetValue("table", out var tableObj) && tableObj is List<Card> table)
        {
            _tableWindow?.DisplayCards(table, "Cards on Table:");
        }
    }
    
    private void OnCardSelected(Card card)
    {
        var cardEncounter = (CardGameEncounter)Encounter;
        if (cardEncounter.PlayCard(card))
        {
            GD.Print($"Played card: {card.GetFullName()}");
        }
        else
        {
            GD.Print($"Invalid play: {card.GetFullName()}");
        }
    }
    
    protected override void OnEncounterStateChanged(Dictionary<string, object> state)
    {
        base.OnEncounterStateChanged(state);
        UpdateDisplays();
    }
}