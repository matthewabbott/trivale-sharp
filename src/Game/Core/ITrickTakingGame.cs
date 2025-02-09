// src/Game/Core/ITrickTakingGame.cs
public interface ITrickTakingGame
{
    GameRules Rules { get; }
    bool IsGameOver { get; }
    Suit LeadSuit { get; }
    int CurrentPlayer { get; }
    
    bool IsValidPlay(int playerId, Card card);
    bool PlayCard(int playerId, Card card);
    TrickResult ResolveTrick();
}