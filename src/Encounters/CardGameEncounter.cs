// src/Encounters/CardGameEncounter.cs
using System;
using System.Collections.Generic;
using Trivale.Cards;
using Trivale.Game;
using Trivale.Game.Core;

namespace Trivale.Encounters;

public class CardGameEncounter : BaseEncounter
{
    public override string Type => "CardGame";
    
    public override Dictionary<string, float> ResourceRequirements => new()
    {
        ["MEM"] = 1.0f,
        ["CPU"] = 0.5f
    };
    
    protected GameState GameState { get; private set; }
    protected GameConfiguration Config { get; private set; }
    
    public event Action<Card> CardPlayed;
    public event Action<int> TrickWon;
    
    public CardGameEncounter(string id, GameConfiguration config = null) : base(id)
    {
        Config = config ?? GameConfiguration.Default;
    }
    
    public override void Initialize(Dictionary<string, object> initialState)
    {
        base.Initialize(initialState);
        
        GameState = new GameState();
        
        // Hook up game state events
        GameState.GameStateChanged += HandleGameStateChanged;
        GameState.TrickCompleted += OnTrickCompleted;
        GameState.GameOver += OnGameOver;
        
        // Initialize the game
        GameState.InitializeGame(
            encounterType: Game.Core.EncounterType.SecuredSystem,
            numPlayers: Config.NumPlayers,
            handSize: Config.HandSize,
            requiredTricks: Config.RequiredTricks
        );
        
        SaveCurrentState();
    }
    
    public override void Cleanup()
    {
        if (GameState != null)
        {
            GameState.GameStateChanged -= HandleGameStateChanged;
            GameState.TrickCompleted -= OnTrickCompleted;
            GameState.GameOver -= OnGameOver;
        }
    }
    
    // Public interface methods
    public List<Card> GetPlayerHand() => GameState.GetHand(0);
    public List<Card> GetHand(int playerId) => GameState.GetHand(playerId);
    public List<Card> GetTableCards() => GameState.GetTableCards();
    public int GetCurrentPlayer() => GameState.GetCurrentPlayer();
    public int GetPlayerScore() => GameState.GetScore(0);
    public int GetScore(int playerId) => GameState.GetScore(playerId);
    public int GetRequiredTricks() => GameState.GetRequiredTricks(0);
    public int GetPlayerCount() => Config.NumPlayers;
    public Dictionary<Card, List<Card>> PreviewPlay(Card card) => GameState.PreviewPlay(card);
    public bool Undo() => GameState.Undo();
    public bool PlayAITurns() => GameState.PlayAITurns();
    public virtual bool PlayCard(Card card) => GameState.PlayCard(0, card);
    
    protected void SaveCurrentState()
    {
        if (GameState == null) return;
        
        State["hand"] = GameState.GetHand(0);
        State["tricks"] = GameState.GetScore(0);
        State["isComplete"] = GameState.IsGameOver;
        State["table"] = GameState.GetTableCards();
        
        EmitStateChanged();
    }
    
    protected virtual void HandleGameStateChanged()
    {
        SaveCurrentState();
    }
    
    private void OnTrickCompleted(int winner)
    {
        TrickWon?.Invoke(winner);
        EmitEncounterEvent($"trick_won_{winner}");
    }
    
    private void OnGameOver(int winner)
    {
        IsComplete = true;
        EmitEncounterEvent($"game_over_{winner}");
    }
}