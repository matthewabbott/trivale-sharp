// src/OS/MainMenu/IOrchestratableScene.cs
using Godot;

namespace Trivale.OS.MainMenu;

/// <summary>
/// Defines a contract for scenes that can be managed by the SceneOrchestrator.
/// </summary>
public interface IOrchestratableScene
{
    void SetOrchestrator(SceneOrchestrator orchestrator);
    string GetProcessId();
}