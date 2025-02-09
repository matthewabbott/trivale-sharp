// src/Game/GameState.cs
using Godot;
using System;
using System.Collections.Generic;
using Trivale.Cards;
using Trivale.Game.Core;

namespace Trivale.Game;

/// <summary>
/// GameState acts as a facade between the Godot framework and our core game logic.
/// It coordinates the various components of the game system and manages their lifecycle.
/// </summary>
public partial class GameState : Node
{
    private readonly ITrickTakingGame _game;
    private readonly IPlayerManager _playerManager;
    private readonly IGameStateManager _stateManager;
    private readonly IAIController _aiController;
    
    public bool IsGameOver => _game.IsGameOver;
    public int RequiredTricks { get; private set; }
    public int Winner { get; private set; } = -1;  // -1 indicates no winner yet
    public Suit TrumpSuit => (_game.Rules as GameRules)?.TrumpSuit ?? Suit.None;
    
    [Signal]
    public delegate void GameStateChangedEventHandler();
    
    [Signal]
    public delegate void TrickCompletedEventHandler(int winner);
    
    [Signal]
    public delegate void GameOverEventHandler(int winner);
    
    public GameState()
    {
        // Initialize core components
        _playerManager = new PlayerManager(4);  // Default to 4 players
        _stateManager = new GameStateManager();
        _aiController = new AIController();
        
        var rules = new GameRules(
            MustFollowSuit: true,
            HasTrumpSuit: false,
            TrumpSuit: Suit.None,
            RequiredTricks: -1
        );
        
        _game = new TrickTakingGame(_playerManager, rules);
        
        // Hook up events
        // TODO: Implement proper event handling
    }
    
    public void InitializeGame(EncounterType encounterType = EncounterType.SecuredSystem,
        int numPlayers = 4, int handSize = 5, int requiredTricks = -1)
    {
        RequiredTricks = requiredTricks;
        Winner = -1;  // Reset winner
        
        // Create and deal a deck
        var deck = CreateDeck();
        ShuffleDeck(deck);
        
        var hands = new Dictionary<int, List<Card>>();
        int cardsDealt = 0;
        
        for (int i = 0; i < numPlayers && cardsDealt < deck.Count; i++)
        {
            var hand = new List<Card>();
            for (int j = 0; j < handSize && cardsDealt < deck.Count; j++)
            {
                hand.Add(deck[cardsDealt++]);
            }
            hands[i] = hand;
        }
        
        _playerManager.DealCards(hands);
        
        // Set up AI behaviors
        for (int i = 1; i < numPlayers; i++)
        {
            _aiController.SetBehavior(i, AIBehavior.OrderedPlay);
        }
        
        _stateManager.SaveState();
        EmitSignal(SignalName.GameStateChanged);
    }
    
    public bool PlayCard(int playerId, Card card)
    {
        if (_game.PlayCard(playerId, card))
        {
            _stateManager.SaveState();
            EmitSignal(SignalName.GameStateChanged);
            return true;
        }
        return false;
    }
    
    public bool Undo()
    {
        if (_stateManager.Undo())
        {
            EmitSignal(SignalName.GameStateChanged);
            return true;
        }
        return false;
    }
    
    public Dictionary<Card, List<Card>> PreviewPlay(Card card)
    {
        if (_game.CurrentPlayer != 0) return null;
        if (!_game.IsValidPlay(0, card)) return null;
        
        return _aiController.PreviewResponses(0, card);
    }
    
    public bool PlayAITurns()
    {
        var currentPlayer = _game.CurrentPlayer;
        if (currentPlayer == 0 || IsGameOver) return false;
        
        bool played = false;
        while (currentPlayer != 0 && !IsGameOver)
        {
            var hand = _playerManager.GetPlayerHand(currentPlayer);
            var validPlays = new List<Card>();
            
            foreach (var card in hand)
            {
                if (_game.IsValidPlay(currentPlayer, card))
                {
                    validPlays.Add(card);
                }
            }
            
            if (validPlays.Count > 0)
            {
                var cardToPlay = _aiController.GetNextPlay(currentPlayer, validPlays);
                if (cardToPlay != null)
                {
                    PlayCard(currentPlayer, cardToPlay);
                    played = true;
                }
            }
            
            currentPlayer = _game.CurrentPlayer;
        }
        
        return played;
    }
    
    // Public accessors
    public List<Card> GetHand(int playerId) => _playerManager.GetPlayerHand(playerId);
    public List<Card> GetTableCards() => new(); // TODO: Implement table cards tracking
    public int GetCurrentPlayer() => _game.CurrentPlayer;
    public Suit GetLeadSuit() => _game.LeadSuit;
    public bool IsHumanPlayer(int playerId) => _playerManager.IsHuman(playerId);
    public int GetScore(int playerId) => _playerManager.GetPlayerScore(playerId);
    public int GetRequiredTricks(int playerId) => RequiredTricks;
    public int GetWinner() => Winner;
    public Suit GetTrumpSuit() => TrumpSuit;
    
    private void OnTrickCompleted(int winner)
    {
        EmitSignal(SignalName.TrickCompleted, winner);
    }
    
    private void OnGameOver(int winner)
    {
        Winner = winner;
        EmitSignal(SignalName.GameOver, winner);
    }
    
    // Private helpers
    private List<Card> CreateDeck()
    {
        var deck = new List<Card>();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            if (suit == Suit.None || suit == Suit.NoTrump) continue;
            
            foreach (Value value in Enum.GetValues(typeof(Value)))
            {
                deck.Add(new Card { Suit = suit, Value = value });
            }
        }
        return deck;
    }
    
    private void ShuffleDeck(List<Card> deck)
    {
        var rng = new Random();
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var temp = deck[k];
            deck[k] = deck[n];
            deck[n] = temp;
        }
    }
}