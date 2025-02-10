// src/Game/Services/GameLifecycleManager.cs
using System;
using Godot;
using Trivale.Cards;
using Trivale.Game.Core;
using Trivale.Game.Core.Interfaces;
using Trivale.Game.Implementation;

namespace Trivale.Game.Services
{
    public interface IGameLifecycleManager
    {
        ITrickTakingGame Game { get; }
        IPlayerManager PlayerManager { get; }
        IGameStateManager StateManager { get; }
        IAIController AIController { get; }
        IDeckManager DeckManager { get; }
        IGameEventCoordinator EventCoordinator { get; }
        
        void Initialize(Node signalSource);
        void InitializeGame(EncounterType encounterType, int numPlayers, int handSize, int requiredTricks);
        void Cleanup();
    }

    public class GameLifecycleManager : IGameLifecycleManager
    {
        public ITrickTakingGame Game { get; private set; }
        public IPlayerManager PlayerManager { get; private set; }
        public IGameStateManager StateManager { get; private set; }
        public IAIController AIController { get; private set; }
        public IDeckManager DeckManager { get; private set; }
        public IGameEventCoordinator EventCoordinator { get; private set; }

        public void Initialize(Node signalSource)
        {
            // Create core components
            PlayerManager = new PlayerManager(4);  // Default to 4 players
            StateManager = new GameStateManager();
            AIController = new AIController();
            DeckManager = new DeckManager();
            EventCoordinator = new GameEventCoordinator();
            EventCoordinator.Initialize(signalSource);

            var rules = new GameRules(
                MustFollowSuit: true,
                HasTrumpSuit: false,
                TrumpSuit: Suit.None,
                RequiredTricks: -1
            );

            Game = new TrickTakingGame(PlayerManager, rules);
        }

        public void InitializeGame(EncounterType encounterType, int numPlayers, int handSize, int requiredTricks)
        {
            // Create and deal cards
            var deck = DeckManager.CreateDeck();
            DeckManager.ShuffleDeck(deck);
            var hands = DeckManager.DealCards(deck, numPlayers, handSize);
            PlayerManager.DealCards(hands);

            // Set up AI behaviors
            for (int i = 1; i < numPlayers; i++)
            {
                AIController.SetBehavior(i, AIBehavior.OrderedPlay);
            }

            StateManager.SaveState();
            EventCoordinator.HandleGameStateChanged();
        }

        public void Cleanup()
        {
            EventCoordinator?.Cleanup();
            EventCoordinator = null;
        }
    }
}