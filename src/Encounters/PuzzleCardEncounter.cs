// src/Encounters/PuzzleCardEncounter.cs

using Godot;
using System.Collections.Generic;
using Trivale.Cards;
using Trivale.Terminal;

namespace Trivale.Encounters;

public class PuzzleCardEncounter : CardGameEncounter
{
    private CardTerminalWindow _playerHandWindow;
    private CardTerminalWindow _tableCardsWindow;
    
    public PuzzleCardEncounter(string id, GameConfiguration config = null) : base(id, config)
    {
    }
    
    protected override void OnInitialize()
    {
        base.OnInitialize();
        CreateWindows();
        UpdateDisplays();
    }
    
    // We don't want to store UI elements in State since they can't be serialized
private Dictionary<string, CardTerminalWindow> _windows = new();

public CardTerminalWindow GetWindow(string name) => _windows.GetValueOrDefault(name);

private void CreateWindows()
{
    // Create player hand window
    _windows["hand"] = new CardTerminalWindow
    {
        WindowTitle = $"Your Hand - {Id}",
        Position = new Vector2(50, 50),
        BorderColor = new Color(0, 1, 0) // Green
    };
    _windows["hand"].CardSelected += OnPlayerCardSelected;
    
    // Create table cards window
    _windows["table"] = new CardTerminalWindow
    {
        WindowTitle = $"Table - {Id}",
        Position = new Vector2(300, 50),
        BorderColor = new Color(0, 0.7f, 1) // Cyan
    };
    
    EmitEncounterEvent("windows_created");
}
    
    private void UpdateDisplays()
    {
        var handWindow = GetWindow("hand");
        if (handWindow != null)
        {
            var playerHand = GameState.GetHand(0);
            handWindow.DisplayCards(playerHand, "Your Hand:");
        }
        
        var tableWindow = GetWindow("table");
        if (tableWindow != null)
        {
            var tableCards = GameState.GetTableCards();
            tableWindow.DisplayCards(tableCards, "Cards on Table:");
        }
    }
    
    private void OnPlayerCardSelected(Card card)
    {
        if (PlayCard(card))
        {
            GD.Print($"Played card: {card.GetFullName()}");
            UpdateDisplays();
        }
        else
        {
            GD.Print($"Invalid play: {card.GetFullName()}");
        }
    }
    
    protected override void OnCleanup()
    {
        base.OnCleanup();
        
        // Clean up windows
        foreach (var window in _windows.Values)
        {
            window?.QueueFree();
        }
        _windows.Clear();
    }
    
    // Override state change handler to update displays
    private void OnGameStateChanged()
    {
        base.OnGameStateChanged();
        UpdateDisplays();
    }
}