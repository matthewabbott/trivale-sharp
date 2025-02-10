// src/Game/Services/IGameStateManager.cs
public interface IGameStateManager
{
    void SaveState();
    bool CanUndo { get; }
    bool Undo();
    void Clear();
}