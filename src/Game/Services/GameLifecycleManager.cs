// src/Game/Services/GameLifecycleManager.cs
using System;
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
        
        void Initialize(int numPlayers = 4);
        void InitializeGame(EncounterType encounterType, int numPlayers, int handSize, int requiredTricks);
        void RegisterGameEventHandlers(Action<int> onTrickCompleted, Action<int> onGameOver, Action onGameStateChanged);
        void UnregisterGameEventHandlers(Action<int> onTrickCompleted, Action<int> onGameOver, Action onGameStateChanged);
        void Cleanup();
    }

    public class GameLifecycleManager : IGameLifecycleManager
    {
        public ITrickTakingGame Game { get; private set; }
        public IPlayerManager PlayerManager { get; private set; }
        public IGameStateManager StateManager { get; private set; }
        public IAIController AIController { get; private set; }
        public IDeckManager DeckManager { get; private set; }

        private Action<int> _onTrickCompleted;
        private Action<int> _onGameOver;
        private Action _onGameStateChanged;

        public void Initialize(int numPlayers = 4)
        {
            // Create core components
            PlayerManager = new PlayerManager(numPlayers);
            StateManager = new GameStateManager();
            AIController = new AIController();
            DeckManager = new DeckManager();

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
            _onGameStateChanged?.Invoke();
        }

        public void RegisterGameEventHandlers(
            Action<int> onTrickCompleted,
            Action<int> onGameOver,
            Action onGameStateChanged)
        {
            _onTrickCompleted = onTrickCompleted;
            _onGameOver = onGameOver;
            _onGameStateChanged = onGameStateChanged;
        }

        public void UnregisterGameEventHandlers(
            Action<int> onTrickCompleted,
            Action<int> onGameOver,
            Action onGameStateChanged)
        {
            if (_onTrickCompleted == onTrickCompleted) _onTrickCompleted = null;
            if (_onGameOver == onGameOver) _onGameOver = null;
            if (_onGameStateChanged == onGameStateChanged) _onGameStateChanged = null;
        }

        public void Cleanup()
        {
            _onTrickCompleted = null;
            _onGameOver = null;
            _onGameStateChanged = null;
        }

        // Internal event handlers that components can use
        internal void HandleTrickCompleted(int winner) => _onTrickCompleted?.Invoke(winner);
        internal void HandleGameOver(int winner) => _onGameOver?.Invoke(winner);
        internal void HandleGameStateChanged() => _onGameStateChanged?.Invoke();
    }
}