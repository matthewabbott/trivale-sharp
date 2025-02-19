// src/OS/MainMenu/Processes/CardGameMenuProcess.cs
using System.Collections.Generic;
using Trivale.Memory.ProcessManagement;
using Godot;

namespace Trivale.OS.MainMenu.Processes;

public class CardGameMenuProcess : BaseProcess
{
    public override string Type => "CardGame";
    
    public override Dictionary<string, float> ResourceRequirements => new Dictionary<string, float>
    {
        ["MEM"] = 0.2f,  // Minimal memory usage for now
        ["CPU"] = 0.1f   // Minimal CPU usage for now
    };

    public CardGameMenuProcess(string id) : base(id) { }

    protected override void OnInitialize()
    {
        // For now, just a basic initialization
        GD.Print($"CardGameMenuProcess {Id} initialized");
    }
}