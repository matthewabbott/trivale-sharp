// src/Game/Implementation/TrickTakingGame.cs:
using System;
using System.Collections.Generic;
using System.Linq;
using Trivale.Cards;
using Trivale.Game.Core;
using Trivale.Game.Core.Interfaces;

namespace Trivale.Game.Implementation
{
    public class TrickTakingGame : ITrickTakingGame
    {
        private readonly IPlayerManager _playerManager;
        private readonly List<Card> _cardsInTrick = new();
        private int _currentPlayer;
        private Suit _leadSuit = Suit.None;
        private readonly GameRules _rules;
        
        GameRules ITrickTakingGame.Rules => _rules;
        public bool IsGameOver { get; private set; }
        public Suit LeadSuit => _leadSuit;
        public int CurrentPlayer => _currentPlayer;
        
        public event Action<TrickResult> TrickCompleted;
        public event Action<int> GameOver;
        
        public TrickTakingGame(IPlayerManager playerManager, GameRules rules)
        {
            _playerManager = playerManager;
            _rules = rules;
            _currentPlayer = 0;
        }
        
        public bool IsValidPlay(int playerId, Card card)
        {
            if (playerId != _currentPlayer) return false;
            
            var playerHand = _playerManager.GetPlayerHand(playerId);
            if (!playerHand.Contains(card)) return false;
            
            // If there's a lead suit and we must follow suit, check if the player can follow
            if (_rules.MustFollowSuit && _leadSuit != Suit.None && card.Suit != _leadSuit)
            {
                if (playerHand.Any(c => c.Suit == _leadSuit))
                    return false;
            }
            
            return true;
        }
        
        public bool PlayCard(int playerId, Card card)
        {
            if (!IsValidPlay(playerId, card))
                return false;
                
            _playerManager.RemoveCardFromHand(playerId, card);
            _cardsInTrick.Add(card);
            
            // Set lead suit if this is the first card
            if (_leadSuit == Suit.None)
                _leadSuit = card.Suit;
                
            // Check if trick is complete
            if (_cardsInTrick.Count == _playerManager.PlayerCount)
            {
                var result = ResolveTrick();
                TrickCompleted?.Invoke(result);
                
                // Check for game over
                CheckGameOver();
            }
            else
            {
                // Move to next player
                _currentPlayer = (_currentPlayer + 1) % _playerManager.PlayerCount;
            }
            
            return true;
        }
        
        public TrickResult ResolveTrick()
        {
            int winnerOffset = 0;
            var winningCard = _cardsInTrick[0];
            
            // Find winning card
            for (int i = 1; i < _cardsInTrick.Count; i++)
            {
                var card = _cardsInTrick[i];
                bool wins = false;
                
                if (_rules.HasTrumpSuit)
                {
                    if (card.Suit == _rules.TrumpSuit && winningCard.Suit != _rules.TrumpSuit)
                        wins = true;
                    else if (card.Suit == _rules.TrumpSuit && winningCard.Suit == _rules.TrumpSuit)
                        wins = card.Value > winningCard.Value;
                    else if (card.Suit == winningCard.Suit)
                        wins = card.Value > winningCard.Value;
                }
                else
                {
                    if (card.Suit == winningCard.Suit)
                        wins = card.Value > winningCard.Value;
                }
                
                if (wins)
                {
                    winningCard = card;
                    winnerOffset = i;
                }
            }
            
            // Calculate winner accounting for trick starting player
            int firstPlayer = (_currentPlayer - (_cardsInTrick.Count - 1));
            if (firstPlayer < 0) firstPlayer += _playerManager.PlayerCount;
            int winner = (firstPlayer + winnerOffset) % _playerManager.PlayerCount;
            
            // Award trick
            _playerManager.IncrementScore(winner);
            
            // Prepare for next trick
            var result = new TrickResult(winner, new List<Card>(_cardsInTrick));
            _cardsInTrick.Clear();
            _leadSuit = Suit.None;
            _currentPlayer = winner;
            
            return result;
        }
        
        private void CheckGameOver()
        {
            // Game is over if any player is out of cards
            if (_playerManager.GetPlayerHand(_currentPlayer).Count == 0)
            {
                IsGameOver = true;
                
                // Find winner (player who met their required tricks)
                int winner = -1;
                if (_rules.RequiredTricks > 0)
                {
                    for (int i = 0; i < _playerManager.PlayerCount; i++)
                    {
                        if (_playerManager.GetPlayerScore(i) >= _rules.RequiredTricks)
                        {
                            winner = i;
                            break;
                        }
                    }
                }
                else
                {
                    // If no required tricks, winner is player with most tricks
                    int maxScore = -1;
                    for (int i = 0; i < _playerManager.PlayerCount; i++)
                    {
                        int score = _playerManager.GetPlayerScore(i);
                        if (score > maxScore)
                        {
                            maxScore = score;
                            winner = i;
                        }
                    }
                }
                
                GameOver?.Invoke(winner);
            }
        }
    }
}