// src/Game/GameState.cs

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Trivale.Cards;

namespace Trivale.Game;

public class Player
{
    public bool IsHuman { get; set; }
    public string AIType { get; set; } = "simple";
    public List<Card> Hand { get; set; } = new();
    public int Score { get; set; } = 0;
    public int RequiredTricks { get; set; } = -1; // -1 means no requirement
}

public enum EncounterType
{
    SecuredSystem,  // Dealt hand only
    Backdoor,       // Choose hand
    Firewall       // See and modify hand
}

public partial class GameState : Node
{
    private List<Player> _players;
    private List<Card> _cardsOnTable;
    private int _currentPlayer = 0;
    private Suit _leadSuit = Suit.None;
    private Suit _trumpSuit = Suit.None;
    
    public int NumPlayers { get; private set; } = 4;
    public int HandSize { get; private set; } = 5;
    public EncounterType CurrentEncounter { get; private set; } = EncounterType.SecuredSystem;
    
    public bool IsGameOver { get; private set; } = false;
    public int Winner { get; private set; } = -1;
    public string StatusMessage { get; private set; } = "";

    [Signal]
    public delegate void GameStateChangedEventHandler();
    
    [Signal]
    public delegate void TrickCompletedEventHandler(int winner);
    
    [Signal]
    public delegate void GameOverEventHandler(int winner);
    
    public override void _Ready()
    {
        InitializeGame();
    }
    
    public void InitializeGame(EncounterType encounterType = EncounterType.SecuredSystem)
    {
        // Initialize collections
        _players = new List<Player>();
        _cardsOnTable = new List<Card>();
        
        CurrentEncounter = encounterType;
        _currentPlayer = 0;
        _leadSuit = Suit.None;
        _trumpSuit = Suit.None;
        IsGameOver = false;
        Winner = -1;
        
        // Initialize players (first player is human)
        for (int i = 0; i < NumPlayers; i++)
        {
            _players.Add(new Player { IsHuman = i == 0 });
        }
        
        switch (CurrentEncounter)
        {
            case EncounterType.SecuredSystem:
                DealRandomHands();
                break;
            case EncounterType.Backdoor:
                // Will be populated by player choice
                break;
            case EncounterType.Firewall:
                DealRandomHands();
                // Allow modifications later
                break;
        }
        
        EmitSignal(SignalName.GameStateChanged);
    }
    
    private void DealRandomHands()
    {
        var deck = CreateDeck();
        ShuffleDeck(deck);
        
        for (int i = 0; i < NumPlayers; i++)
        {
            var playerHand = new List<Card>();
            for (int j = 0; j < HandSize; j++)
            {
                if (deck.Count > 0)
                {
                    var card = deck[deck.Count - 1];
                    deck.RemoveAt(deck.Count - 1);
                    card.CardOwner = i;
                    playerHand.Add(card);
                }
            }
            _players[i].Hand = playerHand;
        }
    }
    
    public bool PlayCard(int playerIndex, Card card)
    {
        if (!IsValidPlay(playerIndex, card))
            return false;
            
        var player = _players[playerIndex];
        player.Hand.Remove(card);
        _cardsOnTable.Add(card);
        
        if (_leadSuit == Suit.None)
            _leadSuit = card.Suit;
        
        // Check if trick is complete
        if (_cardsOnTable.Count == NumPlayers)
        {
            ResolveTrick();
        }
        else
        {
            _currentPlayer = (_currentPlayer + 1) % NumPlayers;
        }
        
        EmitSignal(SignalName.GameStateChanged);
        return true;
    }
    
    private bool IsValidPlay(int playerIndex, Card card)
    {
        if (playerIndex != _currentPlayer)
            return false;
            
        var player = _players[playerIndex];
        if (!player.Hand.Contains(card))
            return false;
            
        // If there's a lead suit, must follow if possible
        if (_leadSuit != Suit.None && card.Suit != _leadSuit)
        {
            if (player.Hand.Any(c => c.Suit == _leadSuit))
                return false;
        }
        
        return true;
    }
    
    private void ResolveTrick()
    {
        int winnerOffset = 0;
        var winningCard = _cardsOnTable[0];
        
        // Find winning card
        for (int i = 1; i < _cardsOnTable.Count; i++)
        {
            if (Card.CompareCards(_cardsOnTable[i], winningCard, _trumpSuit) > 0)
            {
                winningCard = _cardsOnTable[i];
                winnerOffset = i;
            }
        }
        
        int winner = (_currentPlayer - (_cardsOnTable.Count - 1) + winnerOffset) % NumPlayers;
        if (winner < 0) winner += NumPlayers; // Handle negative modulo
        
        _players[winner].Score++;
        
        EmitSignal(SignalName.TrickCompleted, winner);
        
        // Check if game is over
        if (_players.All(p => p.Hand.Count == 0))
        {
            CheckGameOver();
        }
        else
        {
            // Set up next trick
            _cardsOnTable.Clear();
            _leadSuit = Suit.None;
            _currentPlayer = winner;
        }
        
        EmitSignal(SignalName.GameStateChanged);
    }
    
    private void CheckGameOver()
    {
        // Handle different win conditions based on encounter type
        switch (CurrentEncounter)
        {
            case EncounterType.SecuredSystem:
                // Basic win condition - most tricks
                Winner = GetPlayerWithHighestScore();
                break;
                
            default:
                // Other encounter types might have special win conditions
                Winner = GetPlayerWithHighestScore();
                break;
        }
        
        IsGameOver = true;
        EmitSignal(SignalName.GameOver, Winner);
    }
    
    private int GetPlayerWithHighestScore()
    {
        int maxScore = _players.Max(p => p.Score);
        return _players.FindIndex(p => p.Score == maxScore);
    }
    
    // Helper methods
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
    
    // Public accessors
    public List<Card> GetHand(int playerIndex) => _players[playerIndex].Hand;
    public List<Card> GetTableCards() => new List<Card>(_cardsOnTable);
    public int GetCurrentPlayer() => _currentPlayer;
    public Suit GetLeadSuit() => _leadSuit;
    public Suit GetTrumpSuit() => _trumpSuit;
    public bool IsHumanPlayer(int playerIndex) => _players[playerIndex].IsHuman;
    public int GetScore(int playerIndex) => _players[playerIndex].Score;
    public int GetRequiredTricks(int playerIndex) => _players[playerIndex].RequiredTricks;
}