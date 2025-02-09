// src/Encounters/CardGameEncounter.cs
using System;
using System.Collections.Generic;
using Trivale.Cards;
using Trivale.Game;

namespace Trivale.Encounters;

/// <summary>
/// Represents a single card game challenge or puzzle.
/// Handles the core game logic without any UI concerns.
/// 
/// Extension Points & Future Features:
/// - Multi-encounter deck sharing (TODO: Add deck/resource manager interface)
/// - Player abilities and powers (TODO: Add PlayerAbility system)
/// - AI-friendly challenge generation (TODO: Add ChallengeConstraints system)
/// - Variable player counts and configurations (TODO: Add PlayerConfig system)
/// - Custom win conditions (TODO: Add WinCondition system)
/// - Special card effects (TODO: Add CardEffect system)
/// </summary>
public class CardGameEncounter : BaseEncounter
{
    public override string Type => "CardGame";
    
    public override Dictionary<string, float> ResourceRequirements => new()
    {
        ["MEM"] = 1.0f,  // Base memory cost
        ["CPU"] = 0.5f   // Base processing cost
    };
    
    // Core game state
    protected GameState GameState { get; private set; }
    protected GameConfiguration Config { get; private set; }
    
    // Events specific to card games
    public event Action<Card> CardPlayed;
    public event Action<int> TrickWon;
    public event Action<Card> SpecialEffectTriggered;
    
    public CardGameEncounter(string id, GameConfiguration config = null) : base(id)
    {
        Config = config ?? GameConfiguration.Default;
    }
    
    public override void Initialize(Dictionary<string, object> initialState)
    {
        base.Initialize(initialState);
        
        // Create game state
        GameState = new GameState();
        
        // Apply configuration
        ApplyConfiguration();
        
        // Hook up game state events
        GameState.GameStateChanged += HandleGameStateChanged;
        GameState.TrickCompleted += OnTrickCompleted;
        GameState.GameOver += OnGameOver;
        
        // Initialize the game with our config
        GameState.InitializeGame(
            EncounterType.SecuredSystem, 
            Config.NumPlayers, 
            Config.HandSize,
            Config.RequiredTricks
        );
        
        // Store initial state
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
    public List<Card> GetTableCards() => GameState.GetTableCards();
    public int GetCurrentPlayer() => GameState.GetCurrentPlayer();
    public int GetPlayerScore() => GameState.GetScore(0);
    public int GetRequiredTricks() => GameState.RequiredTricks;
    public Dictionary<Card, List<Card>> PreviewPlay(Card card) => GameState.PreviewPlay(card);
    public bool Undo() => GameState.Undo();
    public bool PlayAITurns() => GameState.PlayAITurns();
    public virtual bool PlayCard(Card card) => GameState.PlayCard(0, card);
    
    // Configuration System
    protected virtual void ApplyConfiguration()
    {
        // Apply basic game settings
        // Derived classes can override to add more configuration options
    }
    
    // State Management
    protected void SaveCurrentState()
    {
        if (GameState == null) return;
        
        State["hand"] = GameState.GetHand(0);
        State["tricks"] = GameState.GetScore(0);
        State["isComplete"] = GameState.IsGameOver;
        State["table"] = GameState.GetTableCards();
        
        EmitStateChanged();
    }
    
    // Event Handlers
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