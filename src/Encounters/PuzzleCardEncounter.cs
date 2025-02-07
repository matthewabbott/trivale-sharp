using Godot;
using System.Collections.Generic;
using Trivale.Cards;
using Trivale.Terminal;

namespace Trivale.Encounters;

public class PuzzleCardEncounter : CardGameEncounter
{
    // We don't want to store UI elements in State since they can't be serialized
    private Dictionary<string, CardTerminalWindow> _windows = new();
    
    public PuzzleCardEncounter(string id, GameConfiguration config = null) : base(id, config)
    {
        GD.Print($"PuzzleCardEncounter constructor called: {id}");
    }
    
    protected override void OnInitialize()
    {
        GD.Print($"PuzzleCardEncounter.OnInitialize called for {Id}");
        base.OnInitialize();
        CreateWindows();
        UpdateDisplays();
    }
    
    public CardTerminalWindow GetWindow(string name)
    {
        GD.Print($"Getting window {name} for {Id}");
        return _windows.GetValueOrDefault(name);
    }
    
    private void CreateWindows()
    {
        GD.Print($"Creating windows for {Id}");
        
        // Create player hand window
        _windows["hand"] = new CardTerminalWindow
        {
            WindowTitle = $"Your Hand - {Id}",
            Position = new Vector2(50, 50),
            BorderColor = new Color(0, 1, 0) // Green
        };
        _windows["hand"].CardSelected += OnPlayerCardSelected;
        GD.Print("Created hand window");
        
        // Create table cards window
        _windows["table"] = new CardTerminalWindow
        {
            WindowTitle = $"Table - {Id}",
            Position = new Vector2(300, 50),
            BorderColor = new Color(0, 0.7f, 1) // Cyan
        };
        GD.Print("Created table window");
        
        EmitEncounterEvent("windows_created");
    }
    
    private void UpdateDisplays()
    {
        GD.Print($"Updating displays for {Id}");
        
        var handWindow = GetWindow("hand");
        if (handWindow != null)
        {
            var playerHand = GameState.GetHand(0);
            handWindow.DisplayCards(playerHand, "Your Hand:");
            GD.Print($"Updated hand window with {playerHand.Count} cards");
        }
        else
        {
            GD.PrintErr("Hand window was null during update!");
        }
        
        var tableWindow = GetWindow("table");
        if (tableWindow != null)
        {
            var tableCards = GameState.GetTableCards();
            tableWindow.DisplayCards(tableCards, "Cards on Table:");
            GD.Print($"Updated table window with {tableCards.Count} cards");
        }
        else
        {
            GD.PrintErr("Table window was null during update!");
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
        
        GD.Print($"Cleaning up windows for {Id}");
        // Clean up windows
        foreach (var window in _windows.Values)
        {
            window?.QueueFree();
        }
        _windows.Clear();
    }
    
    protected override void HandleGameStateChanged()
    {
        base.HandleGameStateChanged();
        UpdateDisplays();
    }
}