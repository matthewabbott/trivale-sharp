// src/Game/Services/GameStateManager.cs:
using System.Collections.Generic;
using Trivale.Game.Core.Interfaces;

namespace Trivale.Game.Services;

// TODO: Enhance this class with:
// - More sophisticated state diffing
// - Memory usage optimization
// - State compression for long games
// - Branching state support for puzzle solving
public class GameStateManager : IGameStateManager
{
    private readonly Stack<Dictionary<string, object>> _stateStack = new();
    
    public bool CanUndo => _stateStack.Count > 1;
    
    public void SaveState()
    {
        // TODO: Implement proper state capture
        _stateStack.Push(new Dictionary<string, object>());
    }
    
    public bool Undo()
    {
        if (!CanUndo) return false;
        
        _stateStack.Pop();
        return true;
    }
    
    public void Clear()
    {
        _stateStack.Clear();
    }
}