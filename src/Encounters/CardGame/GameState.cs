using Godot;
using System;
using System.Collections.Generic;
using Trivale.Cards;
using Trivale.Encounters.Core;

namespace Trivale.Encounters.CardGame
{
    /// <summary>
    /// Manages the state for a contract whist card game encounter.
    /// Tracks players, cards, tricks, and game rules.
    /// </summary>
    public class GameState : Node
    {
        // Game configuration
        private int _numPlayers = 4;
        private int _handSize = 5;
        private int _requiredTricks = 2;
        private Suit _trumpSuit = Suit.None;
        
        // Game state
        private List<List<Card>> _playerHands = new();
        private List<Card> _currentTrick = new();
        private List<int> _tricksTaken = new();
        private int _currentPlayer = 0;
        private int _leadPlayer = 0;
        private Suit _leadSuit = Suit.None;
        private bool _isGameOver = false;
        private int _winner = -1;
        
        // Signals
        [Signal] public delegate void GameStateChangedEventHandler();
        [Signal] public delegate void TrickCompletedEventHandler(int winner);
        [Signal] public delegate void GameOverEventHandler(int winner);
        
        /// <summary>
        /// Whether the game has ended.
        /// </summary>
        public bool IsGameOver => _isGameOver;
        
        /// <summary>
        /// The index of the winning player, or -1 if the game is not over.
        /// </summary>
        public int Winner => _winner;
        
        /// <summary>
        /// The current trump suit for the game.
        /// </summary>
        public Suit TrumpSuit => _trumpSuit;
        
        /// <summary>
        /// Initializes a new game with the given parameters.
        /// </summary>
        public void InitializeGame(EncounterType encounterType, int numPlayers, int handSize, int requiredTricks)
        {
            _numPlayers = numPlayers;
            _handSize = handSize;
            _requiredTricks = requiredTricks;
            _trumpSuit = GetRandomSuit();
            
            // Reset game state
            _playerHands.Clear();
            _currentTrick.Clear();
            _tricksTaken = new List<int>(new int[numPlayers]);
            _currentPlayer = 0;
            _leadPlayer = 0;
            _leadSuit = Suit.None;
            _isGameOver = false;
            _winner = -1;
            
            // Deal cards
            DealCards();
            
            // Notify listeners
            EmitSignal(SignalName.GameStateChanged);
        }
        
        /// <summary>
        /// Deals cards to all players.
        /// </summary>
        private void DealCards()
        {
            // Create deck
            var deck = CreateDeck();
            
            // Shuffle
            ShuffleDeck(deck);
            
            // Deal to players
            for (int i = 0; i < _numPlayers; i++)
            {
                _playerHands.Add(new List<Card>());
            }
            
            for (int i = 0; i < _handSize; i++)
            {
                for (int j = 0; j < _numPlayers; j++)
                {
                    if (deck.Count > 0)
                    {
                        var card = deck[0];
                        deck.RemoveAt(0);
                        card.CardOwner = j;
                        _playerHands[j].Add(card);
                    }
                }
            }
        }
        
        /// <summary>
        /// Creates a standard deck of cards.
        /// </summary>
        private List<Card> CreateDeck()
        {
            var deck = new List<Card>();
            
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                if (suit == Suit.None || suit == Suit.NoTrump) continue;
                
                foreach (Value value in Enum.GetValues(typeof(Value)))
                {
                    deck.Add(new Card { Suit = suit, Value = value });
                }
            }
            
            return deck;
        }
        
        /// <summary>
        /// Shuffles a deck of cards.
        /// </summary>
        private void ShuffleDeck(List<Card> deck)
        {
            var random = new System.Random();
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }
        
        /// <summary>
        /// Gets a random suit.
        /// </summary>
        private Suit GetRandomSuit()
        {
            var suits = new[] { Suit.DataFlow, Suit.Crypto, Suit.Infra, Suit.Process };
            return suits[new System.Random().Next(suits.Length)];
        }
        
        /// <summary>
        /// Gets the hand for a player.
        /// </summary>
        public List<Card> GetHand(int playerId)
        {
            if (playerId < 0 || playerId >= _playerHands.Count)
                return new List<Card>();
                
            return new List<Card>(_playerHands[playerId]);
        }
        
        /// <summary>
        /// Gets the cards currently in play.
        /// </summary>
        public List<Card> GetTableCards()
        {
            return new List<Card>(_currentTrick);
        }
        
        /// <summary>
        /// Gets the current player.
        /// </summary>
        public int GetCurrentPlayer()
        {
            return _currentPlayer;
        }
        
        /// <summary>
        /// Gets the lead suit for the current trick.
        /// </summary>
        public Suit GetLeadSuit()
        {
            return _leadSuit;
        }
        
        /// <summary>
        /// Gets whether a player is human.
        /// </summary>
        public bool IsHumanPlayer(int playerId)
        {
            // For now, only player 0 is human
            return playerId == 0;
        }
        
        /// <summary>
        /// Gets the number of tricks taken by a player.
        /// </summary>
        public int GetScore(int playerId)
        {
            if (playerId < 0 || playerId >= _tricksTaken.Count)
                return 0;
                
            return _tricksTaken[playerId];
        }
        
        /// <summary>
        /// Gets the required number of tricks for a player to win.
        /// </summary>
        public int GetRequiredTricks(int playerId)
        {
            // For now, all players have the same required tricks
            return _requiredTricks;
        }
        
        /// <summary>
        /// Checks if a play is valid.
        /// </summary>
        public bool IsValidPlay(int playerId, Card card)
        {
            // Not current player's turn
            if (playerId != _currentPlayer)
                return false;
                
            // Card not in player's hand
            if (!_playerHands[playerId].Contains(card))
                return false;
                
            // First card in trick - any card is valid
            if (_currentTrick.Count == 0)
                return true;
                
            // Must follow suit if possible
            if (card.Suit != _leadSuit)
            {
                foreach (var c in _playerHands[playerId])
                {
                    if (c.Suit == _leadSuit)
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Plays a card for a player.
        /// </summary>
        public bool PlayCard(int playerId, Card card)
        {
            if (!IsValidPlay(playerId, card))
                return false;
                
            // Remove from hand
            _playerHands[playerId].Remove(card);
            
            // Add to trick
            _currentTrick.Add(card);
            
            // Set lead suit if first card
            if (_currentTrick.Count == 1)
                _leadSuit = card.Suit;
                
            // Check if trick is complete
            if (_currentTrick.Count == _numPlayers)
            {
                ResolveTrick();
            }
            else
            {
                // Move to next player
                _currentPlayer = (_currentPlayer + 1) % _numPlayers;
            }
            
            // Notify state change
            EmitSignal(SignalName.GameStateChanged);
            
            return true;
        }
        
        /// <summary>
        /// Resolves the current trick.
        /// </summary>
        private void ResolveTrick()
        {
            // Find winning card
            int winnerIndex = 0;
            var winningCard = _currentTrick[0];
            
            for (int i = 1; i < _currentTrick.Count; i++)
            {
                var card = _currentTrick[i];
                
                // Trump beats non-trump
                if (card.Suit == _trumpSuit && winningCard.Suit != _trumpSuit)
                {
                    winnerIndex = i;
                    winningCard = card;
                }
                // Higher card of same suit wins
                else if (card.Suit == winningCard.Suit && card.Value > winningCard.Value)
                {
                    winnerIndex = i;
                    winningCard = card;
                }
            }
            
            // Calculate actual winner based on lead player
            int winner = (_leadPlayer + winnerIndex) % _numPlayers;
            
            // Award trick
            _tricksTaken[winner]++;
            
            // Notify trick completion
            EmitSignal(SignalName.TrickCompleted, winner);
            
            // Check for game over (all cards played)
            if (_playerHands[0].Count == 0)
            {
                EndGame();
            }
            else
            {
                // Set up for next trick
                _currentTrick.Clear();
                _leadPlayer = winner;
                _currentPlayer = winner;
                _leadSuit = Suit.None;
            }
        }
        
        /// <summary>
        /// Ends the game and determines the winner.
        /// </summary>
        private void EndGame()
        {
            _isGameOver = true;
            
            // Determine winner based on meeting their required tricks
            _winner = -1;
            for (int i = 0; i < _numPlayers; i++)
            {
                if (_tricksTaken[i] == _requiredTricks)
                {
                    _winner = i;
                    break;
                }
            }
            
            // If no one met their exact requirement, player with most tricks wins
            if (_winner < 0)
            {
                int maxTricks = -1;
                for (int i = 0; i < _numPlayers; i++)
                {
                    if (_tricksTaken[i] > maxTricks)
                    {
                        maxTricks = _tricksTaken[i];
                        _winner = i;
                    }
                }
            }
            
            // Notify game over
            EmitSignal(SignalName.GameOver, _winner);
        }
        
        /// <summary>
        /// Has AI players take their turns.
        /// </summary>
        public bool PlayAITurns()
        {
            if (_isGameOver || _currentPlayer == 0)
                return false;
                
            bool played = false;
            
            while (_currentPlayer != 0 && !_isGameOver)
            {
                var hand = _playerHands[_currentPlayer];
                var validCards = new List<Card>();
                
                foreach (var card in hand)
                {
                    if (IsValidPlay(_currentPlayer, card))
                        validCards.Add(card);
                }
                
                if (validCards.Count > 0)
                {
                    // Simple AI - play first valid card
                    PlayCard(_currentPlayer, validCards[0]);
                    played = true;
                }
                else
                {
                    // No valid cards - should never happen, but just in case
                    _currentPlayer = (_currentPlayer + 1) % _numPlayers;
                }
            }
            
            return played;
        }
    }
}