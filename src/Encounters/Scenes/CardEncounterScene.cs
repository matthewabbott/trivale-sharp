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
    private CardTerminalWindow _handWindow;
    private CardTerminalWindow _tableWindow;
    private CardTerminalWindow _controlWindow;
    private Dictionary<Card, List<Card>> _currentPreviews;
    private bool _isShowingPreview = false;
    
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
        
        GD.Print($"CardEncounterScene initialized for {encounter.Id}");
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        
        // Clean up windows
        _handWindow?.QueueFree();
        _tableWindow?.QueueFree();
        _controlWindow?.QueueFree();
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
        _handWindow.CardHovered += OnCardHovered;
        _handWindow.CardUnhovered += OnCardUnhovered;
        _windowManager.AddWindow(_handWindow);
        
        // Create table cards window
        _tableWindow = new CardTerminalWindow
        {
            WindowTitle = $"Table - {Encounter.Id}",
            Position = new Vector2(300, 50),
            BorderColor = new Color(0, 0.7f, 1) // Cyan
        };
        _windowManager.AddWindow(_tableWindow);
        
        // Add control window
        _controlWindow = new CardTerminalWindow
        {
            WindowTitle = "Game Controls",
            Position = new Vector2(550, 50),
            MinSize = new Vector2(200, 150),
            BorderColor = new Color(1, 1, 0) // Yellow
        };
        
        var container = new VBoxContainer();
        
        var undoButton = new Button
        {
            Text = "Undo Move",
            CustomMinimumSize = new Vector2(0, 30)
        };
        undoButton.Pressed += OnUndoPressed;
        container.AddChild(undoButton);
        
        var aiTurnButton = new Button
        {
            Text = "Play AI Turns",
            CustomMinimumSize = new Vector2(0, 30)
        };
        aiTurnButton.Pressed += OnAITurnPressed;
        container.AddChild(aiTurnButton);
        
        var statusLabel = new Label
        {
            Name = "StatusLabel",
            Text = "Required Tricks: 0/0\nCurrent Turn: Player"
        };
        container.AddChild(statusLabel);
        
        _controlWindow.AddContent(container);
        _windowManager.AddWindow(_controlWindow);
    }
    
    private void UpdateDisplays()
    {
        var cardEncounter = (CardGameEncounter)Encounter;
        
        // Update player hand window
        var playerHand = cardEncounter.GetPlayerHand();
        _handWindow?.DisplayCards(playerHand, "Your Hand:");
        
        // Update table cards window without preview
        if (!_isShowingPreview)
        {
            var tableCards = cardEncounter.GetTableCards();
            _tableWindow?.DisplayCards(tableCards, "Cards on Table:");
        }
        
        // Update control window status
        if (_controlWindow != null)
        {
            var container = _controlWindow.GetNode<VBoxContainer>("VBoxContainer");
            if (container != null)
            {
                var statusLabel = container.GetNode<Label>("StatusLabel");
                if (statusLabel != null)
                {
                    var playerScore = cardEncounter.GetPlayerScore();
                    var requiredTricks = cardEncounter.GetRequiredTricks();
                    var currentPlayer = cardEncounter.GetCurrentPlayer() == 0 ? "Player" : $"AI {cardEncounter.GetCurrentPlayer()}";
                    
                    statusLabel.Text = $"Required Tricks: {playerScore}/{requiredTricks}\n" +
                                     $"Current Turn: {currentPlayer}";
                }
            }
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