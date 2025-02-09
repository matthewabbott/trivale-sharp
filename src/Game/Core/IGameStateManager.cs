// src/Game/Core/IGameStateManager.cs
public interface IGameStateManager
{
    void SaveState();
    bool CanUndo { get; }
    bool Undo();
    void Clear();
}