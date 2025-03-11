// src/OS/MainMenu/Processes/MainMenuProcess.cs
using Godot;
using System.Collections.Generic;
using Trivale.Memory.ProcessManagement;
using Trivale.OS;

namespace Trivale.OS.MainMenu.Processes;

/// <summary>
/// Represents the Main Menu as a standard process in the process-slot system.
/// This process is special as it's automatically loaded on startup and is
/// placed in slot_0_0 by default.
/// 
/// The MainMenuProcess serves as the entry point to other game modes, and the
/// user can always return to it by selecting its slot in the MEM slot grid.
/// </summary>
public class MainMenuProcess : BaseProcess
{
    public override string Type => "MainMenu";
    
    public override Dictionary<string, float> ResourceRequirements => new Dictionary<string, float>
    {
        ["MEM"] = 0.1f,  // Minimal memory usage for menu
        ["CPU"] = 0.05f  // Minimal CPU usage
    };
    
    public string ScenePath => "res://Scenes/MainMenu/MainMenuScene.tscn";
    
    public MainMenuProcess(string id) : base(id) { }
    
    public override void Initialize(Dictionary<string, object> initialState)
    {
        base.Initialize(initialState);
        State["scenePath"] = ScenePath;
        GD.Print($"MainMenuProcess {Id} initialized");
    }
    
    protected override void OnUpdate(float delta)
    {
        // No continuous updates needed for the main menu
    }
    
    protected override void OnCleanup()
    {
        GD.Print($"MainMenuProcess {Id} cleaned up");
    }
}