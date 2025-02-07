// src/Encounters/CardGameEncounter.cs

using System;
using System.Collections.Generic;
using Trivale.Cards;
using Trivale.Game;

namespace Trivale.Encounters;

/// <summary>
/// CardGameEncounter represents a single card game challenge or puzzle.
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
    
    // Resource costs - can be overridden by derived encounters
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
    
    protected override void OnInitialize()
    {
        // Create and set up game state
        GameState = new GameState();
        
        // Apply configuration
        ApplyConfiguration();
        
        // Hook up game state events
        GameState.GameStateChanged += HandleGameStateChanged;
        GameState.TrickCompleted += OnTrickCompleted;
        GameState.GameOver += OnGameOver;
        
        // Store initial state
        SaveCurrentState();
    }
    
    protected override void OnUpdate(float delta)
    {
        // Handle any time-based effects or animations
    }
    
    protected override void OnCleanup()
    {
        if (GameState != null)
        {
            // Unhook events
            GameState.GameStateChanged -= HandleGameStateChanged;
            GameState.TrickCompleted -= OnTrickCompleted;
            GameState.GameOver -= OnGameOver;
        }
    }
    
    // Configuration System
    protected virtual void ApplyConfiguration()
    {
        // Apply basic game settings
        // Derived classes can override to add more configuration options
    }
    
    // State Management
    protected void SaveCurrentState()
    {
        State["hand"] = GameState.GetHand(0);
        State["tricks"] = GameState.GetScore(0);
        State["isComplete"] = GameState.IsGameOver;
        
        EmitStateChanged();
    }
    
    protected override void OnStateRestored()
    {
        // Restore game state from saved state
        // This allows encounters to be serialized/deserialized
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
    
    // Public Interface
    public virtual bool PlayCard(Card card)
    {
        if (GameState.PlayCard(0, card))
        {
            CardPlayed?.Invoke(card);
            return true;
        }
        return false;
    }
}

/// <summary>
/// Configuration for a card game encounter.
/// This will be expanded as we add more features.
/// </summary>
public class GameConfiguration
{
    public int NumPlayers { get; set; } = 4;
    public int HandSize { get; set; } = 13;
    public bool AllowSpecialCards { get; set; } = false;
    public List<string> EnabledPowers { get; set; } = new();
    
    // Additional configuration options to be added:
    // - Custom deck composition
    // - Special rules
    // - AI behavior settings
    // - Resource limitations
    // - Win conditions
    
    public static GameConfiguration Default => new();
}