// src/Game/GameConfiguration.cs
using System.Collections.Generic;

namespace Trivale.Game;

/// <summary>
/// Configuration for a card game encounter.
/// This will be expanded as we add more features.
/// </summary>
public class GameConfiguration
{
    public int NumPlayers { get; set; } = 4;
    public int HandSize { get; set; } = 13;
    public int RequiredTricks { get; set; } = -1;  // -1 means no requirement
    public bool AllowSpecialCards { get; set; } = false;
    public List<string> EnabledPowers { get; set; } = new();
    
    // Additional configuration options to be added:
    // - Custom deck composition
    // - Special rules
    // - AI behavior settings
    // - Resource limitations
    // - Win conditions
    
    public static GameConfiguration Default => new();
}