// src/Tests/TerminalTestScene.cs

using Godot;
using System.Collections.Generic;
using Trivale.Terminal;
using Trivale.Cards;
using Trivale.Game;

namespace Trivale.Tests;

public partial class TerminalTestScene : Node2D
{
    private GameState _gameState;
    private CardTerminalWindow _playerHandWindow;
    private CardTerminalWindow _tableCardsWindow;
    
    public override void _Ready()
    {
        // Create game state
        _gameState = new GameState();
        AddChild(_gameState);
        
        // Create player hand window
        _playerHandWindow = new CardTerminalWindow
        {
            WindowTitle = "Player Hand",
            Position = new Vector2(50, 50),
            BorderColor = new Color(0, 1, 0) // Green
        };
        AddChild(_playerHandWindow);
        _playerHandWindow.CardSelected += OnPlayerCardSelected;
        
        // Create table cards window
        _tableCardsWindow = new CardTerminalWindow
        {
            WindowTitle = "Table Cards",
            Position = new Vector2(300, 50),
            BorderColor = new Color(0, 0.7f, 1) // Cyan
        };
        AddChild(_tableCardsWindow);
        
        // Update displays
        UpdateWindows();
    }
    
    private void UpdateWindows()
    {
        // Update player hand window
        var playerHand = _gameState.GetHand(0);
        _playerHandWindow.DisplayCards(playerHand, "Your Hand:");
        
        // Update table cards window
        var tableCards = _gameState.GetTableCards();
        _tableCardsWindow.DisplayCards(tableCards, "Cards on Table:");
    }
    
    private void OnPlayerCardSelected(Card card)
    {
        if (_gameState.PlayCard(0, card))
        {
            GD.Print($"Played card: {card.GetFullName()}");
            UpdateWindows();
        }
        else
        {
            GD.Print($"Invalid play: {card.GetFullName()}");
        }
    }
}