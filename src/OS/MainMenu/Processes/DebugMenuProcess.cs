// src/OS/MainMenu/Processes/DebugMenuProcess.cs
using System.Collections.Generic;
using Trivale.Memory.ProcessManagement;
using Godot;

namespace Trivale.OS.MainMenu.Processes;

public class DebugMenuProcess : BaseProcess
{
    public override string Type => "Debug";
    
    public override Dictionary<string, float> ResourceRequirements => new()
    {
        ["MEM"] = 0.1f,  // Even lighter resource usage for debug
        ["CPU"] = 0.1f
    };

    public DebugMenuProcess(string id) : base(id) { }

    protected override void OnInitialize()
    {
        // Basic initialization
        GD.Print($"DebugMenuProcess {Id} initialized");
    }
}