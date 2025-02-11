// src/Encounters/CardGameProcess.cs
using System;
using System.Collections.Generic;
using Trivale.Cards;
using Trivale.Game;
using Trivale.Game.Core;

namespace Trivale.Encounters;

public class CardGameProcess : BaseProcess
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
    
    public CardGameProcess(string id, GameConfiguration config = null) : base(id)
    {
        Config = config ?? GameConfiguration.Default;
    }
    
    public override void Initialize(Dictionary<string, object> initialState)
    {
        GameState = new GameState();
        
        // Hook up game state events
        GameState.GameStateChanged += HandleGameStateChanged;
        GameState.TrickCompleted += OnTrickCompleted;
        GameState.GameOver += OnGameOver;
        
        // Initialize the game
        GameState.InitializeGame(
            encounterType: EncounterType.SecuredSystem,
            numPlayers: Config.NumPlayers,
            handSize: Config.HandSize,
            requiredTricks: Config.RequiredTricks
        );

        base.Initialize(initialState);
    }

    public override void Cleanup()
    {
        if (GameState != null)
        {
            GameState.GameStateChanged -= HandleGameStateChanged;
            GameState.TrickCompleted -= OnTrickCompleted;
            GameState.GameOver -= OnGameOver;
            GameState = null;
        }
        base.Cleanup();
    }
    
    protected override void OnInitialize()
    {
        // Restore state if provided
        if (State.Count > 0)
        {
            RestoreState(State);
        }
        
        SaveCurrentState();
    }

    private void RestoreState(Dictionary<string, object> state)
    {
        // TODO: Implement state restoration
        // This will need careful consideration of how to restore
        // the game state, including all player hands, scores, etc.
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
    
    private void SaveCurrentState()
    {
        if (GameState == null) return;
        
        var state = new Dictionary<string, object>
        {
            ["hand"] = GameState.GetHand(0),
            ["tricks"] = GameState.GetScore(0),
            ["isComplete"] = GameState.IsGameOver,
            ["table"] = GameState.GetTableCards(),
            ["currentPlayer"] = GameState.GetCurrentPlayer()
        };
        
        // Save scores for all players
        var scores = new Dictionary<int, int>();
        for (int i = 0; i < Config.NumPlayers; i++)
        {
            scores[i] = GameState.GetScore(i);
        }
        state["scores"] = scores;
        
        EmitStateChanged();
    }
    
    protected virtual void HandleGameStateChanged()
    {
        SaveCurrentState();
    }
    
    private void OnTrickCompleted(int winner)
    {
        TrickWon?.Invoke(winner);
        EmitProcessEvent($"trick_won_{winner}");
    }
    
    private void OnGameOver(int winner)
    {
        IsComplete = true;
        EmitProcessEvent($"game_over_{winner}");
    }
}
