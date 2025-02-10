// src/Game/GameState.cs
using Godot;
using System;
using System.Collections.Generic;
using Trivale.Cards;
using Trivale.Game.Core;
using Trivale.Game.Core.Interfaces;
using Trivale.Game.Services;

namespace Trivale.Game;

/// <summary>
/// GameState acts as a facade between the Godot framework and our core game logic.
/// It coordinates the various components of the game system and manages their lifecycle.
/// </summary>
public partial class GameState : Node
{
    private readonly IGameLifecycleManager _lifecycleManager;
    
    public bool IsGameOver => _lifecycleManager.Game.IsGameOver;
    public int RequiredTricks { get; private set; }
    public int Winner { get; private set; } = -1;
    public Suit TrumpSuit => (_lifecycleManager.Game.Rules as GameRules)?.TrumpSuit ?? Suit.None;
    
    [Signal]
    public delegate void GameStateChangedEventHandler();
    
    [Signal]
    public delegate void TrickCompletedEventHandler(int winner);
    
    [Signal]
    public delegate void GameOverEventHandler(int winner);
    
    public GameState()
    {
        _lifecycleManager = new GameLifecycleManager();
        _lifecycleManager.Initialize();
        
        // Register event handlers
        _lifecycleManager.RegisterGameEventHandlers(
            OnTrickCompleted,
            OnGameOver,
            () => EmitSignal(SignalName.GameStateChanged)
        );
    }
    
    public override void _ExitTree()
    {
        _lifecycleManager.Cleanup();
        base._ExitTree();
    }
    
    public void InitializeGame(EncounterType encounterType = EncounterType.SecuredSystem,
        int numPlayers = 4, int handSize = 5, int requiredTricks = -1)
    {
        RequiredTricks = requiredTricks;
        Winner = -1;
        
        _lifecycleManager.InitializeGame(encounterType, numPlayers, handSize, requiredTricks);
    }
    
    public bool PlayCard(int playerId, Card card)
    {
        if (_lifecycleManager.Game.PlayCard(playerId, card))
        {
            _lifecycleManager.StateManager.SaveState();
            EmitSignal(SignalName.GameStateChanged);
            return true;
        }
        return false;
    }
    
    public bool Undo()
    {
        if (_lifecycleManager.StateManager.Undo())
        {
            EmitSignal(SignalName.GameStateChanged);
            return true;
        }
        return false;
    }
    
    public Dictionary<Card, List<Card>> PreviewPlay(Card card)
    {
        if (_lifecycleManager.Game.CurrentPlayer != 0) return null;
        if (!_lifecycleManager.Game.IsValidPlay(0, card)) return null;
        
        return _lifecycleManager.AIController.PreviewResponses(0, card);
    }
    
    public bool PlayAITurns()
    {
        var currentPlayer = _lifecycleManager.Game.CurrentPlayer;
        if (currentPlayer == 0 || IsGameOver) return false;
        
        bool played = false;
        while (currentPlayer != 0 && !IsGameOver)
        {
            var hand = _lifecycleManager.PlayerManager.GetPlayerHand(currentPlayer);
            var validPlays = new List<Card>();
            
            foreach (var card in hand)
            {
                if (_lifecycleManager.Game.IsValidPlay(currentPlayer, card))
                {
                    validPlays.Add(card);
                }
            }
            
            if (validPlays.Count > 0)
            {
                var cardToPlay = _lifecycleManager.AIController.GetNextPlay(currentPlayer, validPlays);
                if (cardToPlay != null)
                {
                    PlayCard(currentPlayer, cardToPlay);
                    played = true;
                }
            }
            
            currentPlayer = _lifecycleManager.Game.CurrentPlayer;
        }
        
        return played;
    }
    
    private void OnTrickCompleted(int winner)
    {
        EmitSignal(SignalName.TrickCompleted, winner);
    }
    
    private void OnGameOver(int winner)
    {
        Winner = winner;
        EmitSignal(SignalName.GameOver, winner);
    }
    
    // Public accessors
    public List<Card> GetHand(int playerId) => _lifecycleManager.PlayerManager.GetPlayerHand(playerId);
    public List<Card> GetTableCards() => new(); // TODO: Implement table cards tracking
    public int GetCurrentPlayer() => _lifecycleManager.Game.CurrentPlayer;
    public Suit GetLeadSuit() => _lifecycleManager.Game.LeadSuit;
    public bool IsHumanPlayer(int playerId) => _lifecycleManager.PlayerManager.IsHuman(playerId);
    public int GetScore(int playerId) => _lifecycleManager.PlayerManager.GetPlayerScore(playerId);
    public int GetRequiredTricks(int playerId) => RequiredTricks;
    public int GetWinner() => Winner;
    public Suit GetTrumpSuit() => TrumpSuit;
}