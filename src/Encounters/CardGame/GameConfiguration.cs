// src/Encounters/CardGame/GameConfiguration.cs

using Trivale.Cards;
using System;

namespace Trivale.Encounters.CardGame
{
    /// <summary>
    /// Configuration settings for a card game encounter.
    /// Serializable to support state preservation across process lifecycle.
    /// </summary>
    [Serializable]
    public class GameConfiguration
    {
        public int NumPlayers { get; set; } = 4;
        public int HandSize { get; set; } = 5;
        public int RequiredTricks { get; set; } = 2;
        public Suit TrumpSuit { get; set; } = Suit.None;
    }
}