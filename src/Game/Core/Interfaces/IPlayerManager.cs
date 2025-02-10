// src/Game/Core/Interfaces/IPlayerManager.cs:
using System.Collections.Generic;
using Trivale.Cards;

namespace Trivale.Game.Core.Interfaces
{
    public interface IPlayerManager
    {
        int PlayerCount { get; }
        List<Card> GetPlayerHand(int playerId);
        int GetPlayerScore(int playerId);
        bool IsHuman(int playerId);
        void DealCards(Dictionary<int, List<Card>> hands);
        void AddCardToHand(int playerId, Card card);
        void RemoveCardFromHand(int playerId, Card card);
        void IncrementScore(int playerId);
    }
}