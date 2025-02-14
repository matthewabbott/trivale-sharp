// src/OS/Processes/MenuProcess.cs
using System;
using System.Collections.Generic;
using Godot;
using Trivale.Memory;
using Trivale.OS.UI;

namespace Trivale.OS.Processes;

public class MenuProcess : BaseProcess
{
    public override string Type => "MainMenu";
    
    public override Dictionary<string, float> ResourceRequirements => new()
    {
        ["MEM"] = 0.1f,  // Minimal memory requirement
        ["CPU"] = 0.1f   // Minimal CPU requirement
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
    }
    
    private void OnMenuOptionSelected(string option)
    {
        switch (option)
        {
            case "CardGame":
                LoadCardGame();
                break;
            case "Debug":
                LoadDebugSandbox();
                break;
        }
        
        // Notify that state has changed
        EmitStateChanged();
        
        // Emit a process event that can be used to update the UI
        EmitProcessEvent($"option_selected_{option}");
    }
    
    private void LoadCardGame()
    {
        var cardGame = new Encounters.CardGameProcess($"card_game_{DateTime.Now.Ticks}");
        if (_processManager.StartProcess(cardGame))
        {
            IsComplete = true;  // Menu process is done once we load a game
        }
    }
    
    private void LoadDebugSandbox()
    {
        // For now, we'll treat the debug sandbox similarly to a process
        var debugProcess = new DebugSandboxProcess($"debug_{DateTime.Now.Ticks}");
        if (_processManager.StartProcess(debugProcess))
        {
            IsComplete = true;
        }
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