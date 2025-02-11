// src/Encounters/Scenes/CardProcessScene.cs
using Godot;
using System;
using System.Collections.Generic;
using Trivale.Terminal;
using Trivale.Cards;
using Trivale.OS;
using Trivale.Game;
using Trivale.Memory;

namespace Trivale.Encounters.Scenes;

/// <summary>
/// Scene manager for card-based processes. Handles the visualization and interaction
/// of card games through terminal windows.
/// </summary>
public partial class CardProcessScene : ProcessScene
{
	private WindowManager _windowManager;
	private CardTerminalWindow _playerHandWindow;
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
	
	public override void Initialize(IProcess process, WindowManager windowManager)
	{
		_windowManager = windowManager;
		
		if (!(process is CardGameProcess))
		{
			throw new ArgumentException($"Expected CardGameProcess, got {process.GetType().Name}");
		}
		
		base.Initialize(process, windowManager);
		
		CreateWindows();
		UpdateDisplays();
		
		GD.Print($"CardProcessScene initialized for {process.Id}");
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
		GD.Print($"Creating windows for {Process.Id}");
		var cardProcess = (CardGameProcess)Process;
		
		// Create player hand window (left side)
		_playerHandWindow = new CardTerminalWindow
		{
			WindowTitle = $"Your Hand - {Process.Id}",
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
			WindowTitle = $"Table - {Process.Id}",
			Position = new Vector2(TABLE_X, INITIAL_Y),
			BorderColor = new Color(0, 0.7f, 1) // Cyan
		};
		_windowManager.AddWindow(_tableWindow);
		
		// Create AI hand windows (right side)
		int numPlayers = cardProcess.GetPlayerCount();
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
		var cardProcess = (CardGameProcess)Process;
		
		// Update player hand window
		var playerHand = cardProcess.GetPlayerHand();
		_playerHandWindow?.DisplayCards(playerHand, "Your Hand:");
		
		// Update AI hand windows
		foreach (var (playerId, window) in _aiHandWindows)
		{
			var aiHand = cardProcess.GetHand(playerId);
			var playerScore = cardProcess.GetScore(playerId);
			var requiredTricks = cardProcess.GetRequiredTricks();
			window?.DisplayCards(aiHand, $"AI {playerId} (Tricks: {playerScore}/{requiredTricks})");
		}
		
		// Update table cards window without preview
		if (!_isShowingPreview)
		{
			var tableCards = cardProcess.GetTableCards();
			_tableWindow?.DisplayCards(tableCards, "Cards on Table:");
		}
		
		// Update status label using stored reference
		if (_statusLabel != null && _statusLabel.IsInsideTree())
		{
			var playerScore = cardProcess.GetPlayerScore();
			var requiredTricks = cardProcess.GetRequiredTricks();
			var currentPlayer = cardProcess.GetCurrentPlayer() == 0 ? "Player" : $"AI {cardProcess.GetCurrentPlayer()}";
			
			_statusLabel.Text = $"Required Tricks: {playerScore}/{requiredTricks}\n" +
							   $"Current Turn: {currentPlayer}";
		}
	}
	
	private void OnCardSelected(Card card)
	{
		var cardProcess = (CardGameProcess)Process;
		if (cardProcess.PlayCard(card))
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
		var cardProcess = (CardGameProcess)Process;
		
		// Get AI responses
		_currentPreviews = cardProcess.PreviewPlay(card);
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
		var cardProcess = (CardGameProcess)Process;
		
		// Add the current table cards
		tableCards.AddRange(cardProcess.GetTableCards());
		
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
		var cardProcess = (CardGameProcess)Process;
		if (cardProcess.Undo())
		{
			_isShowingPreview = false;
			UpdateDisplays();
		}
	}
	
	private void OnAITurnPressed()
	{
		var cardProcess = (CardGameProcess)Process;
		if (cardProcess.PlayAITurns())
		{
			_isShowingPreview = false;
			UpdateDisplays();
		}
	}
	
	protected override void OnProcessStateChanged(Dictionary<string, object> state)
	{
		base.OnProcessStateChanged(state);
		_isShowingPreview = false;
		UpdateDisplays();
	}
}
