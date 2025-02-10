// src/Game/Services/DeckManager.cs
using System;
using System.Collections.Generic;
using Trivale.Cards;
using Trivale.Game.Core.Interfaces;

namespace Trivale.Game.Services
{
    public interface IDeckManager
    {
        List<Card> CreateDeck();
        void ShuffleDeck(List<Card> deck);
        Dictionary<int, List<Card>> DealCards(List<Card> deck, int numPlayers, int cardsPerPlayer);
    }

    public class DeckManager : IDeckManager
    {
        private readonly Random _rng;

        public DeckManager(Random? rng = null)
        {
            _rng = rng ?? new Random();
        }

        public List<Card> CreateDeck()
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

        public void ShuffleDeck(List<Card> deck)
        {
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                var temp = deck[k];
                deck[k] = deck[n];
                deck[n] = temp;
            }
        }

        public Dictionary<int, List<Card>> DealCards(List<Card> deck, int numPlayers, int cardsPerPlayer)
        {
            var hands = new Dictionary<int, List<Card>>();
            int cardsDealt = 0;

            for (int i = 0; i < numPlayers && cardsDealt < deck.Count; i++)
            {
                var hand = new List<Card>();
                for (int j = 0; j < cardsPerPlayer && cardsDealt < deck.Count; j++)
                {
                    var card = deck[cardsDealt++];
                    card.CardOwner = i;
                    hand.Add(card);
                }
                hands[i] = hand;
            }

            return hands;
        }
    }
}