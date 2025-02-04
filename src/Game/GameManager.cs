// src/Game/GameManager.cs

using Godot;
using System.Collections.Generic;
using Trivale.Cards;

namespace Trivale.Game;

public partial class GameManager : Node
{
    private List<Card> _deck = new();
    private List<List<Card>> _playerHands = new();
    private int _currentPlayer = 0;
    private int _numPlayers = 4;
    
    public override void _Ready()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        CreateDeck();
        ShuffleDeck();
        DealCards();
    }
    
    private void CreateDeck()
    {
        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            foreach (Value value in System.Enum.GetValues(typeof(Value)))
            {
                var card = new Card
                {
                    Suit = suit,
                    Value = value
                };
                _deck.Add(card);
            }
        }
    }
    
    private void ShuffleDeck()
    {
        var random = new System.Random();
        int n = _deck.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (_deck[k], _deck[n]) = (_deck[n], _deck[k]);
        }
    }
    
    private void DealCards()
    {
        _playerHands.Clear();
        for (int i = 0; i < _numPlayers; i++)
        {
            _playerHands.Add(new List<Card>());
        }
        
        int cardsPerPlayer = 13; // For a standard game
        for (int i = 0; i < cardsPerPlayer * _numPlayers; i++)
        {
            _playerHands[i % _numPlayers].Add(_deck[i]);
        }
    }
}