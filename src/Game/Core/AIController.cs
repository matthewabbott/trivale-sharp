// src/Game/Core/AIController.cs
using System.Collections.Generic;
using System.Linq;
using Trivale.Cards;

namespace Trivale.Game.Core
{
    public class AIController : IAIController
    {
        private readonly Dictionary<int, AIBehavior> _behaviors = new();
        
        public void SetBehavior(int playerId, AIBehavior behavior)
        {
            _behaviors[playerId] = behavior;
        }
        
        public Card GetNextPlay(int playerId, List<Card> validPlays)
        {
            return validPlays.FirstOrDefault();
        }
        
        public Dictionary<Card, List<Card>> PreviewResponses(int playerId, Card playerCard)
        {
            return new Dictionary<Card, List<Card>>();
        }
    }
}