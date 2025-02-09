// src/Game/Core/Types.cs
using System.Collections.Generic;
using Trivale.Cards;

namespace Trivale.Game.Core
{
    public record TrickResult(int Winner, List<Card> Cards);

    public record GameRules(
        bool MustFollowSuit,
        bool HasTrumpSuit,
        Suit TrumpSuit,
        int RequiredTricks
    );
}