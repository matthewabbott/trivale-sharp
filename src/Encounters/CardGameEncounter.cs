// src/Encounters/CardGameEncounter.cs
using System;
using System.Collections.Generic;
using Trivale.Cards;
using Trivale.Game;
using Trivale.Game.Core;
using Trivale.Memory;

namespace Trivale.Encounters;

public class CardGameEncounter : IProcess
{
    public string Id { get; }
    public string Type => "CardGame";
    public bool IsComplete { get; protected set; }
    public IMemorySlot Slot { get; private set; }

    public Dictionary<string, float> ResourceRequirements => new()
    {
        ["MEM"] = 1.0f,
        ["CPU"] = 0.5f
    };

    protected GameState GameState { get; private set; }
    protected GameConfiguration Config { get; private set; }
    
    public event Action<Dictionary<string, object>> StateChanged;
    public event Action<string> ProcessEvent;  // renamed from EncounterEvent
    public event Action<Card> CardPlayed;
    public event Action<int> TrickWon;
    
    public CardGameEncounter(string id, GameConfiguration config = null)
    {
        Id = id;
        Config = config ?? GameConfiguration.Default;
    }
    
    public void Initialize(Dictionary<string, object> initialState)
    {
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

        // Restore state if provided
        if (initialState != null)
        {
            RestoreState(initialState);
        }
        
        SaveCurrentState();
    }

    public void Update(float delta)
    {
        // Add any per-frame update logic here if needed
        // For example, AI thinking time, animations, etc.
    }

    public void Cleanup()
    {
        if (GameState != null)
        {
            GameState.GameStateChanged -= HandleGameStateChanged;
            GameState.TrickCompleted -= OnTrickCompleted;
            GameState.GameOver -= OnGameOver;
            GameState = null;
        }
    }
    
    public Dictionary<string, object> GetState()
    {
        var state = new Dictionary<string, object>();
        
        if (GameState != null)
        {
            state["hand"] = GameState.GetHand(0);
            state["tricks"] = GameState.GetScore(0);
            state["isComplete"] = GameState.IsGameOver;
            state["table"] = GameState.GetTableCards();
            state["currentPlayer"] = GameState.GetCurrentPlayer();
            
            // Save scores for all players
            var scores = new Dictionary<int, int>();
            for (int i = 0; i < Config.NumPlayers; i++)
            {
                scores[i] = GameState.GetScore(i);
            }
            state["scores"] = scores;
        }
        
        return state;
    }

    protected void RestoreState(Dictionary<string, object> state)
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
    
    protected void SaveCurrentState()
    {
        if (GameState == null) return;
        StateChanged?.Invoke(GetState());
    }
    
    protected virtual void HandleGameStateChanged()
    {
        SaveCurrentState();
    }
    
    private void OnTrickCompleted(int winner)
    {
        TrickWon?.Invoke(winner);
        ProcessEvent?.Invoke($"trick_won_{winner}");
    }
    
    private void OnGameOver(int winner)
    {
        IsComplete = true;
        ProcessEvent?.Invoke($"game_over_{winner}");
    }
}