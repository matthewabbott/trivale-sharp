// src/Encounters/CardGame/CardGameProcess.cs

using System.Collections.Generic;
using Trivale.Memory.ProcessManagement;
using Godot;
using Trivale.Cards;

namespace Trivale.Encounters.CardGame
{
    /// <summary>
    /// Process implementation for a card game encounter.
    /// Handles state tracking, game logic, and resource management.
    /// 
    /// This represents a single "encounter" in the game world - a trick-taking
    /// card game challenge that the player must overcome.
    /// </summary>
    public class CardGameProcess : BaseProcess
    {
        public override string Type => "CardGame";
        
        /// <summary>
        /// Resource requirements for the card game process.
        /// </summary>
        public override Dictionary<string, float> ResourceRequirements => new Dictionary<string, float>
        {
            ["MEM"] = 0.3f,  // Memory usage for game state and cards
            ["CPU"] = 0.2f   // Processing for game logic and AI
        };
        
        // Game configuration 
        private GameConfiguration _config;
        
        /// <summary>
        /// Creates a new CardGameProcess with the specified ID.
        /// </summary>
        /// <param name="id">Unique identifier for this encounter</param>
        public CardGameProcess(string id) : base(id) { }
        
        /// <summary>
        /// Initializes the card game with default or provided configuration.
        /// </summary>
        protected override void OnInitialize()
        {
            GD.Print($"CardGameProcess {Id} initializing");
            
            // Initialize or restore configuration
            if (State.Count == 0)
            {
                // New process, create default configuration
                _config = new GameConfiguration
                {
                    NumPlayers = 4,
                    HandSize = 5,
                    RequiredTricks = 2,
                    TrumpSuit = GetRandomSuit()
                };
                
                // Store configuration in state
                State["config"] = _config;
                
                GD.Print($"CardGameProcess {Id} initialized with default configuration");
            }
            else
            {
                // Existing process, load configuration from state
                _config = (GameConfiguration)State["config"];
                GD.Print($"CardGameProcess {Id} restored with existing configuration");
            }
            
            EmitStateChanged();
        }
        
        /// <summary>
        /// Selects a random suit for the trump suit.
        /// </summary>
        private Suit GetRandomSuit()
        {
            // Get a random suit (excluding None and NoTrump)
            var suits = new[] { Suit.DataFlow, Suit.Crypto, Suit.Infra, Suit.Process };
            return suits[new System.Random().Next(suits.Length)];
        }
    }
}