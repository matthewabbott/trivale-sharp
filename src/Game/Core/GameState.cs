// src/Game/Core/GameState.cs
using Godot;
using System;
using System.Collections.Generic;
using Trivale.Cards;

namespace Trivale.Game.Core;

// TODO: Enhance this class with:
// - Better component lifecycle management
// - More sophisticated event system
// - Resource tracking
// - Cross-encounter state management
public partial class GameState : Node
{
    private readonly ITrickTakingGame _game;
    private readonly IPlayerManager _playerManager;
    private readonly IGameStateManager _stateManager;
    private readonly IAIController _aiController;
    
    public bool IsGameOver => _game.IsGameOver;
    
    [Signal]
    public delegate void GameStateChangedEventHandler();
    
    [Signal]
    public delegate void TrickCompletedEventHandler(int winner);
    
    [Signal]
    public delegate void GameOverEventHandler(int winner);
    
    public GameState()
    {
        _playerManager = new PlayerManager(4); // TODO: Make configurable
        _stateManager = new GameStateManager();
        _aiController = new AIController();
        
        var rules = new GameRules(
            MustFollowSuit: true,
            HasTrumpSuit: false,
            TrumpSuit: Suit.None,
            RequiredTricks: -1
        );
        
        _game = new TrickTakingGame(_playerManager, rules);
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
        return _stateManager.Undo();
    }
    
    public Dictionary<Card, List<Card>> PreviewPlay(Card card)
    {
        return _aiController.PreviewResponses(0, card);
    }
    
    public bool PlayAITurns()
    {
        // TODO: Implement proper AI turn sequence
        return false;
    }
    
    // Public accessors
    public List<Card> GetHand(int playerId) => _playerManager.GetPlayerHand(playerId);
    public List<Card> GetTableCards() => new(); // TODO: Implement
    public int GetCurrentPlayer() => _game.CurrentPlayer;
    public Suit GetLeadSuit() => _game.LeadSuit;
    public bool IsHumanPlayer(int playerId) => _playerManager.IsHuman(playerId);
    public int GetScore(int playerId) => _playerManager.GetPlayerScore(playerId);
    public int GetRequiredTricks(int playerId) => -1; // TODO: Implement per-player requirements
}