// src/OS/Processes/DebugSandboxProcess.cs
using System.Collections.Generic;
using Godot;
using Trivale.Memory;
using Trivale.Tests;

namespace Trivale.OS.Processes;

public class DebugSandboxProcess : BaseProcess
{
    public override string Type => "Debug";
    
    public override Dictionary<string, float> ResourceRequirements => new()
    {
        ["MEM"] = 0.2f,
        ["CPU"] = 0.2f
    };
    
    private WindowSystemTest _debugScene;
    
    public DebugSandboxProcess(string id) : base(id) { }
    
    public override void Initialize(Dictionary<string, object> initialState)
    {
        base.Initialize(initialState);
        
        // Create the debug test scene
        _debugScene = new WindowSystemTest();
    }
    
    public override void Cleanup()
    {
        if (_debugScene != null)
        {
            _debugScene.QueueFree();
            _debugScene = null;
        }
        
        base.Cleanup();
    }
}