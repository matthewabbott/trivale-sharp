// src/Encounters/Scenes/CardEncounterScene.cs

using Godot;
using System;
using System.Collections.Generic;
using Trivale.Terminal;
using Trivale.Cards;
using Trivale.OS;
using Trivale.Game;

namespace Trivale.Encounters.Scenes;

/// <summary>
/// Scene manager for card-based encounters. Handles the visualization and interaction
/// of card games through terminal windows.
/// </summary>
public partial class CardEncounterScene : EncounterScene
{
    private WindowManager _windowManager;
    private CardTerminalWindow _playerHandWindow;  // renamed from _handWindow
    private Dictionary<int, CardTerminalWindow> _aiHandWindows = new();
    private CardTerminalWindow _tableWindow;
    private CardTerminalWindow _controlWindow;
    private Dictionary<Card, List<Card>> _currentPreviews;
    private bool _isShowingPreview = false;
    private Control _controlContainer;
    private Label _statusLabel;

    // Layout constants
    private const float PLAYER_HAND_X = 50;
    private const float TABLE_X = 300;
    private const float AI_HANDS_X = 600;
    private const float CONTROL_X = 850;
    private const float INITIAL_Y = 50;
    private const float AI_WINDOW_SPACING = 20;
    private static readonly Vector2 AI_WINDOW_SIZE = new Vector2(200, 150);
    
    public override void Initialize(IEncounter encounter)
    {
        if (!(encounter is CardGameEncounter))
        {
            throw new ArgumentException($"Expected CardGameEncounter, got {encounter.GetType().Name}");
        }
        
        var desktop = GetNode<SystemDesktop>("/root/SystemDesktop");
        _windowManager = desktop.GetNode<WindowManager>("WindowLayer");
        
        base.Initialize(encounter);
        
        CreateWindows();
        UpdateDisplays();
        
        GD.Print($"CardEncounterScene initialized for {encounter.Id}");
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        
        _playerHandWindow?.QueueFree();
        foreach (var window in _aiHandWindows.Values)
        {
            window?.QueueFree();
        }
        _aiHandWindows.Clear();
        _tableWindow?.QueueFree();
        _controlWindow?.QueueFree();
    }
    
    private void CreateWindows()
    {
        GD.Print($"Creating windows for {Encounter.Id}");
        var cardEncounter = (CardGameEncounter)Encounter;
        
        // Create player hand window (left side)
        _playerHandWindow = new CardTerminalWindow
        {
            WindowTitle = $"Your Hand - {Encounter.Id}",
            Position = new Vector2(PLAYER_HAND_X, INITIAL_Y),
            BorderColor = new Color(0, 1, 0) // Green
        };
        _playerHandWindow.CardSelected += OnCardSelected;
        _playerHandWindow.CardHovered += OnCardHovered;
        _playerHandWindow.CardUnhovered += OnCardUnhovered;
        _windowManager.AddWindow(_playerHandWindow);
        
        // Create table cards window (center)
        _tableWindow = new CardTerminalWindow
        {
            WindowTitle = $"Table - {Encounter.Id}",
            Position = new Vector2(TABLE_X, INITIAL_Y),
            BorderColor = new Color(0, 0.7f, 1) // Cyan
        };
        _windowManager.AddWindow(_tableWindow);
        
        // Create AI hand windows (right side)
        int numPlayers = cardEncounter.GetPlayerCount();
        float currentY = INITIAL_Y;
        
        for (int i = 1; i < numPlayers; i++)  // Start from 1 to skip player 0
        {
            var aiWindow = new CardTerminalWindow
            {
                WindowTitle = $"AI Player {i}",
                Position = new Vector2(AI_HANDS_X, currentY),
                MinSize = AI_WINDOW_SIZE,
                BorderColor = new Color(1, 0, 0) // Red for AI
            };
            _aiHandWindows[i] = aiWindow;
            _windowManager.AddWindow(aiWindow);
            
            currentY += AI_WINDOW_SIZE.Y + AI_WINDOW_SPACING;
        }
        
        // Create control window (far right)
        _controlWindow = new CardTerminalWindow
        {
            WindowTitle = "Game Controls",
            Position = new Vector2(CONTROL_X, INITIAL_Y),
            MinSize = new Vector2(200, 150),
            BorderColor = new Color(1, 1, 0) // Yellow
        };
        
        _controlContainer = new VBoxContainer();
        
        var undoButton = new Button
        {
            Text = "Undo Move",
            CustomMinimumSize = new Vector2(0, 30)
        };
        undoButton.Pressed += OnUndoPressed;
        _controlContainer.AddChild(undoButton);
        
        var aiTurnButton = new Button
        {
            Text = "Play AI Turns",
            CustomMinimumSize = new Vector2(0, 30)
        };
        aiTurnButton.Pressed += OnAITurnPressed;
        _controlContainer.AddChild(aiTurnButton);
        
        _statusLabel = new Label
        {
            Text = "Required Tricks: 0/0\nCurrent Turn: Player"
        };
        _controlContainer.AddChild(_statusLabel);
        
        _controlWindow.AddContent(_controlContainer);
        _windowManager.AddWindow(_controlWindow);
    }
    
    private void UpdateDisplays()
    {
        var cardEncounter = (CardGameEncounter)Encounter;
        
        // Update player hand window
        var playerHand = cardEncounter.GetPlayerHand();
        _playerHandWindow?.DisplayCards(playerHand, "Your Hand:");
        
        // Update AI hand windows
        foreach (var (playerId, window) in _aiHandWindows)
        {
            var aiHand = cardEncounter.GetHand(playerId);
            var playerScore = cardEncounter.GetScore(playerId);
            var requiredTricks = cardEncounter.GetRequiredTricks();
            window?.DisplayCards(aiHand, $"AI {playerId} (Tricks: {playerScore}/{requiredTricks})");
        }
        
        // Update table cards window without preview
        if (!_isShowingPreview)
        {
            var tableCards = cardEncounter.GetTableCards();
            _tableWindow?.DisplayCards(tableCards, "Cards on Table:");
        }
        
        // Update status label using stored reference
        if (_statusLabel != null && _statusLabel.IsInsideTree())
        {
            var playerScore = cardEncounter.GetPlayerScore();
            var requiredTricks = cardEncounter.GetRequiredTricks();
            var currentPlayer = cardEncounter.GetCurrentPlayer() == 0 ? "Player" : $"AI {cardEncounter.GetCurrentPlayer()}";
            
            _statusLabel.Text = $"Required Tricks: {playerScore}/{requiredTricks}\n" +
                               $"Current Turn: {currentPlayer}";
        }
    }
    
    private void OnCardSelected(Card card)
    {
        var cardEncounter = (CardGameEncounter)Encounter;
        if (cardEncounter.PlayCard(card))
        {
            GD.Print($"Played card: {card.GetFullName()}");
            _isShowingPreview = false;
            UpdateDisplays();
        }
        else
        {
            GD.Print($"Invalid play: {card.GetFullName()}");
        }
    }
    
    private void OnCardHovered(Card card)
    {
        var cardEncounter = (CardGameEncounter)Encounter;
        
        // Get AI responses
        _currentPreviews = cardEncounter.PreviewPlay(card);
        if (_currentPreviews != null && _currentPreviews.ContainsKey(card))
        {
            HighlightAIResponses(_currentPreviews[card]);
        }
    }
    
    private void OnCardUnhovered(Card card)
    {
        _isShowingPreview = false;
        UpdateDisplays();
    }
    
    private void HighlightAIResponses(List<Card> responses)
    {
        var tableCards = new List<Card>();
        var cardEncounter = (CardGameEncounter)Encounter;
        
        // Add the current table cards
        tableCards.AddRange(cardEncounter.GetTableCards());
        
        // Add the predicted responses with a special display state
        foreach (var response in responses)
        {
            var previewCard = response.CreateDuplicate();
            tableCards.Add(previewCard);
        }
        
        _isShowingPreview = true;
        _tableWindow?.DisplayCards(tableCards, "Cards on Table (Preview):", true);
    }
    
    private void OnUndoPressed()
    {
        var cardEncounter = (CardGameEncounter)Encounter;
        if (cardEncounter.Undo())
        {
            _isShowingPreview = false;
            UpdateDisplays();
        }
    }
    
    private void OnAITurnPressed()
    {
        var cardEncounter = (CardGameEncounter)Encounter;
        if (cardEncounter.PlayAITurns())
        {
            _isShowingPreview = false;
            UpdateDisplays();
        }
    }
    
    protected override void OnEncounterStateChanged(Dictionary<string, object> state)
    {
        base.OnEncounterStateChanged(state);
        _isShowingPreview = false;
        UpdateDisplays();
    }
}