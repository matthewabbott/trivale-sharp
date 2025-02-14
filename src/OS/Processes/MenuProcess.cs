// src/OS/Processes/MenuProcess.cs
using System;
using System.Collections.Generic;
using Godot;
using Trivale.Memory;
using Trivale.OS.UI;

namespace Trivale.OS.Processes;

/// <summary>
/// MenuProcess handles the main menu UI and facilitates loading other processes
/// into Slot 0. This process itself doesn't go into a memory slot.
/// </summary>
public class MenuProcess : BaseProcess
{
    public override string Type => "MainMenu";
    
    public override Dictionary<string, float> ResourceRequirements => new()
    {
        ["MEM"] = 0.1f,
        ["CPU"] = 0.1f
    };
    
    private MainMenu _menuScene;
    private ProcessManager _processManager;
    
    public MenuProcess(string id, ProcessManager processManager) : base(id)
    {
        _processManager = processManager;
    }
    
    public override void Initialize(Dictionary<string, object> initialState)
    {
        base.Initialize(initialState);
        
        // Create the menu scene
        _menuScene = new MainMenu();
        _menuScene.OptionSelected += OnMenuOptionSelected;
        State["menuScene"] = _menuScene;
    }
    
    private void OnMenuOptionSelected(string option)
    {
        // Create the appropriate process based on selection
        IProcess processToLoad = option switch
        {
            "CardGame" => CreateCardGameMetaProcess(),
            "Debug" => CreateDebugSandboxProcess(),
            _ => null
        };
        
        if (processToLoad != null)
        {
            // Try to load the process into Slot 0
            if (_processManager.StartProcess(processToLoad))
            {
                GD.Print($"Successfully loaded {option} into Slot 0");
                EmitProcessEvent($"loaded_{option.ToLower()}");
            }
            else
            {
                GD.PrintErr($"Failed to load {option} into Slot 0");
            }
        }
        
        EmitStateChanged();
    }
    
    private IProcess CreateCardGameMetaProcess()
    {
        // TODO: Implement proper CardGameMetaProcess
        // For now, return a placeholder that just shows "Card Game Meta Menu Coming Soon"
        return new PlaceholderProcess(
            "card_meta",
            "Card Game Configuration", 
            "Card Game meta menu functionality coming soon!"
        );
    }
    
    private IProcess CreateDebugSandboxProcess()
    {
        // TODO: Implement proper DebugSandboxProcess
        // For now, return a placeholder that shows some debug info
        return new PlaceholderProcess(
            "debug_sandbox",
            "Debug Sandbox", 
            "Debug sandbox functionality coming soon!\nWill include:\n- Window tests\n- Process tests\n- Memory management tests"
        );
    }
    
    public override void Cleanup()
    {
        if (_menuScene != null)
        {
            _menuScene.OptionSelected -= OnMenuOptionSelected;
            _menuScene.QueueFree();
            _menuScene = null;
        }
        
        base.Cleanup();
    }
}