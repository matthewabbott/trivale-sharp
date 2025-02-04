// src/MainScene.cs

using Godot;
using System.Collections.Generic;
using Trivale.Terminal;
using Trivale.Game;

namespace Trivale;

public partial class MainScene : Node2D
{
    private DesktopTerminal _desktop;
    private GameState _gameState;
    private List<TerminalWindow> _gameWindows = new();
    
    public override void _Ready()
    {
        SetupDesktop();
    }
    
    private void SetupDesktop()
    {
        _desktop = new DesktopTerminal();
        AddChild(_desktop);
        _desktop.GameStartRequested += StartGame;
    }
    
    private void StartGame(int numPlayers, int handSize)
    {
        // Clean up any existing game
        CleanupGame();
        
        // Create new game state
        _gameState = new GameState();
        _gameState.SetupGame(numPlayers, handSize);
        AddChild(_gameState);
        
        // Create game windows
        CreateGameWindows();
        
        _desktop.PrintLine("Game started successfully.");
    }
    
    private void CreateGameWindows()
    {
        // Player hand window
        var handWindow = new CardTerminalWindow
        {
            WindowTitle = "Your Hand",
            Position = new Vector2(50, 50),
            BorderColor = new Color(0, 1, 0) // Green
        };
        AddChild(handWindow);
        handWindow.CardSelected += OnPlayerCardSelected;
        
        // Table window
        var tableWindow = new CardTerminalWindow
        {
            WindowTitle = "Table",
            Position = new Vector2(300, 50),
            BorderColor = new Color(0, 0.7f, 1) // Cyan
        };
        AddChild(tableWindow);
        
        _gameWindows.Add(handWindow);
        _gameWindows.Add(tableWindow);
        
        // Initial update
        UpdateGameWindows();
    }
    
    private void UpdateGameWindows()
    {
        if (_gameState == null) return;
        
        // Update player hand
        var handWindow = _gameWindows[0] as CardTerminalWindow;
        handWindow?.DisplayCards(_gameState.GetHand(0), "Your Hand:");
        
        // Update table
        var tableWindow = _gameWindows[1] as CardTerminalWindow;
        tableWindow?.DisplayCards(_gameState.GetTableCards(), "Cards on Table:");
    }
    
    private void OnPlayerCardSelected(Card card)
    {
        if (_gameState.PlayCard(0, card))
        {
            _desktop.PrintLine($"Played: {card.GetFullName()}");
            if (_gameState.IsHumanTurn())
            {
                _desktop.PrintLine("It's your turn.");
            }
            else
            {
                PlayAITurns();
            }
            UpdateGameWindows();
        }
        else
        {
            _desktop.PrintLine($"Can't play {card.GetFullName()} right now.");
        }
    }
    
    private void PlayAITurns()
    {
        while (!_gameState.IsGameOver && !_gameState.IsHumanTurn())
        {
            var aiPlayer = _gameState.GetCurrentPlayer();
            var aiHand = _gameState.GetHand(aiPlayer);
            
            foreach (var card in aiHand)
            {
                if (_gameState.PlayCard(aiPlayer, card))
                {
                    _desktop.PrintLine($"AI {aiPlayer} plays: {card.GetFullName()}");
                    UpdateGameWindows();
                    break;
                }
            }
        }
    }
    
    private void CleanupGame()
    {
        // Remove old game state
        if (_gameState != null)
        {
            _gameState.QueueFree();
            _gameState = null;
        }
        
        // Remove game windows
        foreach (var window in _gameWindows)
        {
            window.QueueFree();
        }
        _gameWindows.Clear();
    }
}