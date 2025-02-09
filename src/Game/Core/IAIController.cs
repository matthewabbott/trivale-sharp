// src/Game/Core/IAIController.cs
using System.Collections.Generic;
using Trivale.Cards;

namespace Trivale.Game.Core
{
    public interface IAIController
    {
        void SetBehavior(int playerId, AIBehavior behavior);
        Card GetNextPlay(int playerId, List<Card> validPlays);
        Dictionary<Card, List<Card>> PreviewResponses(int playerId, Card playerCard);
    }
}