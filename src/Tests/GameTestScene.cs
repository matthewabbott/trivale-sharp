// src/Tests/GameTestScene.cs

using Godot;
using System;
using System.Linq;
using Trivale.Game;
using Trivale.Cards;
using Trivale.Utils;

namespace Trivale.Tests;

public partial class GameTestScene : Node2D
{
    private GameState _gameState;
    private VBoxContainer _mainContainer;
    private Label _statusLabel;
    private VBoxContainer _playerHandsContainer;
    private HBoxContainer _tableCardsContainer;
    private Button _nextTurnButton;
    private Button _resetGameButton;
    
    public override void _Ready()
    {
        SetupUI();
        InitializeGame();
    }
    
    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(800, 600),
            Position = new Vector2(20, 20)
        };
        AddChild(_mainContainer);
        
        // Status display
        _statusLabel = new Label();
        _mainContainer.AddChild(_statusLabel);
        
        // Table cards display
        var tableLabel = new Label { Text = "Cards on Table:" };
        _mainContainer.AddChild(tableLabel);
        
        _tableCardsContainer = new HBoxContainer();
        _mainContainer.AddChild(_tableCardsContainer);
        
        // Player hands display
        _playerHandsContainer = new VBoxContainer();
        _mainContainer.AddChild(_playerHandsContainer);
        
        // Control buttons
        var buttonContainer = new HBoxContainer();
        _mainContainer.AddChild(buttonContainer);
        
        _nextTurnButton = new Button { Text = "Next Turn" };
        _nextTurnButton.Pressed += OnNextTurnPressed;
        buttonContainer.AddChild(_nextTurnButton);
        
        _resetGameButton = new Button { Text = "Reset Game" };
        _resetGameButton.Pressed += OnResetGamePressed;
        buttonContainer.AddChild(_resetGameButton);
    }
    
    private void InitializeGame()
    {
        _gameState = new GameState();
        _gameState.GameStateChanged += UpdateDisplay;
        _gameState.TrickCompleted += OnTrickCompleted;
        _gameState.GameOver += OnGameOver;
        
        AddChild(_gameState);
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        // Update status
        var status = $"Current Player: {_gameState.GetCurrentPlayer()}\n";
        status += $"Lead Suit: {_gameState.GetLeadSuit()}\n";
        status += $"Trump Suit: {_gameState.GetTrumpSuit()}\n";
        _statusLabel.Text = status;
        
        // Update table cards
        _tableCardsContainer.QueueFreeChildren();
        foreach (var card in _gameState.GetTableCards())
        {
            var cardLabel = new Label { Text = card.GetFullName() };
            _tableCardsContainer.AddChild(cardLabel);
        }
        
        // Update player hands
        _playerHandsContainer.QueueFreeChildren();
        for (int i = 0; i < 4; i++)
        {
            var playerLabel = new Label
            {
                Text = $"Player {i} " +
                      $"(Score: {_gameState.GetScore(i)}, " +
                      $"Required: {_gameState.GetRequiredTricks(i)})"
            };
            _playerHandsContainer.AddChild(playerLabel);
            
            var handContainer = new HBoxContainer();
            _playerHandsContainer.AddChild(handContainer);
            
            foreach (var card in _gameState.GetHand(i))
            {
                var cardButton = new Button 
                { 
                    Text = card.GetFullName(),
                    Disabled = _gameState.GetCurrentPlayer() != i
                };
                cardButton.Pressed += () => OnCardPressed(i, card);
                handContainer.AddChild(cardButton);
            }
        }
    }
    
    private void OnCardPressed(int playerIndex, Card card)
    {
        if (_gameState.PlayCard(playerIndex, card))
        {
            GD.Print($"Player {playerIndex} played {card.GetFullName()}");
        }
        else
        {
            GD.Print($"Invalid play: Player {playerIndex} tried to play {card.GetFullName()}");
        }
    }
    
    private void OnNextTurnPressed()
    {
        // If it's an AI's turn, make them play
        int currentPlayer = _gameState.GetCurrentPlayer();
        if (!_gameState.IsHumanPlayer(currentPlayer))
        {
            var hand = _gameState.GetHand(currentPlayer);
            if (hand.Count > 0)
            {
                // For testing, just play the first valid card
                foreach (var card in hand)
                {
                    if (_gameState.PlayCard(currentPlayer, card))
                    {
                        break;
                    }
                }
            }
        }
    }
    
    private void OnResetGamePressed()
    {
        InitializeGame();
    }
    
    private void OnTrickCompleted(int winner)
    {
        GD.Print($"Trick completed! Winner: Player {winner}");
    }
    
    private void OnGameOver(int winner)
    {
        GD.Print($"Game Over! Winner: Player {winner}");
        _nextTurnButton.Disabled = true;
    }
}