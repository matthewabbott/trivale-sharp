// src/Game/Core/PlayerManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Trivale.Cards;

namespace Trivale.Game.Core;

public class PlayerManager : IPlayerManager
{
    private class PlayerState
    {
        public List<Card> Hand { get; } = new();
        public int Score { get; set; }
        public bool IsHuman { get; set; }
    }
    
    private readonly Dictionary<int, PlayerState> _players = new();
    
    public int PlayerCount => _players.Count;
    
    public PlayerManager(int numPlayers, int humanPlayerId = 0)
    {
        if (numPlayers <= 0)
            throw new ArgumentException("Must have at least one player", nameof(numPlayers));
            
        if (humanPlayerId >= numPlayers)
            throw new ArgumentException("Human player ID must be less than total players", nameof(humanPlayerId));
            
        // Initialize players
        for (int i = 0; i < numPlayers; i++)
        {
            _players[i] = new PlayerState
            {
                IsHuman = (i == humanPlayerId)
            };
        }
    }
    
    public List<Card> GetPlayerHand(int playerId)
    {
        ValidatePlayerId(playerId);
        return new List<Card>(_players[playerId].Hand);
    }
    
    public int GetPlayerScore(int playerId)
    {
        ValidatePlayerId(playerId);
        return _players[playerId].Score;
    }
    
    public bool IsHuman(int playerId)
    {
        ValidatePlayerId(playerId);
        return _players[playerId].IsHuman;
    }
    
    public void DealCards(Dictionary<int, List<Card>> hands)
    {
        // Clear existing hands
        foreach (var state in _players.Values)
        {
            state.Hand.Clear();
        }
        
        // Deal new cards
        foreach (var (playerId, cards) in hands)
        {
            ValidatePlayerId(playerId);
            _players[playerId].Hand.AddRange(cards);
            
            // Set card ownership
            foreach (var card in cards)
            {
                card.CardOwner = playerId;
            }
        }
    }
    
    public void AddCardToHand(int playerId, Card card)
    {
        ValidatePlayerId(playerId);
        card.CardOwner = playerId;
        _players[playerId].Hand.Add(card);
    }
    
    public void RemoveCardFromHand(int playerId, Card card)
    {
        ValidatePlayerId(playerId);
        if (!_players[playerId].Hand.Remove(card))
        {
            throw new InvalidOperationException($"Card {card.GetFullName()} not found in player {playerId}'s hand");
        }
        card.CardOwner = -1; // Card is no longer owned
    }
    
    public void IncrementScore(int playerId)
    {
        ValidatePlayerId(playerId);
        _players[playerId].Score++;
    }
    
    // Helper method for copying state (useful for undo/redo)
    public Dictionary<int, (List<Card> Hand, int Score)> GetAllPlayerStates()
    {
        return _players.ToDictionary(
            kvp => kvp.Key,
            kvp => (
                Hand: new List<Card>(kvp.Value.Hand),
                Score: kvp.Value.Score
            )
        );
    }
    
    // Helper method for restoring state (useful for undo/redo)
    public void RestorePlayerStates(Dictionary<int, (List<Card> Hand, int Score)> states)
    {
        foreach (var (playerId, (hand, score)) in states)
        {
            ValidatePlayerId(playerId);
            var playerState = _players[playerId];
            playerState.Hand.Clear();
            playerState.Hand.AddRange(hand);
            playerState.Score = score;
        }
    }
    
    private void ValidatePlayerId(int playerId)
    {
        if (!_players.ContainsKey(playerId))
        {
            throw new ArgumentException($"Invalid player ID: {playerId}");
        }
    }
}