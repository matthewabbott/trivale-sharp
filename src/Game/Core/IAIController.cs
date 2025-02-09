// src/Game/Core/IAIController.cs
public interface IAIController
{
    void SetBehavior(int playerId, AIBehavior behavior);
    Card GetNextPlay(int playerId, List<Card> validPlays);
    Dictionary<Card, List<Card>> PreviewResponses(int playerId, Card playerCard);
}