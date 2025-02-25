// src/OS/MainMenu/Processes/MainMenuProcess.cs
using System.Collections.Generic;
using Trivale.Memory.ProcessManagement;
using Godot;

namespace Trivale.OS.MainMenu.Processes;

public class MainMenuProcess : BaseProcess
{
    public override string Type => "MainMenu";
    
    public override Dictionary<string, float> ResourceRequirements => new Dictionary<string, float>
    {
        ["MEM"] = 0.1f,  // Minimal memory usage for menu
        ["CPU"] = 0.05f  // Minimal CPU usage
    };

    public MainMenuProcess(string id) : base(id) { }

    protected override void OnInitialize()
    {
        // Basic initialization
        GD.Print($"MainMenuProcess {Id} initialized");
    }
}