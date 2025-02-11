// src/Encounters/Scenes/ProcessScene.cs
using Godot;
using System;
using System.Collections.Generic;
using Trivale.Memory;
using Trivale.OS;

namespace Trivale.Encounters.Scenes;

/// <summary>
/// Base class for managing the scene tree representation of a process.
/// This handles the Godot-specific aspects of process visualization and interaction.
/// </summary>
public partial class ProcessScene : Node
{
    protected IProcess Process { get; private set; }
    
    [Signal]
    public delegate void ProcessClosedEventHandler(string processId);
    
    public virtual void Initialize(IProcess process, WindowManager windowManager)
    {
        GD.Print($"Initializing scene for process: {process.Id}");
        Process = process;
        
        // Hook up standard events
        Process.StateChanged += OnProcessStateChanged;
        Process.ProcessEvent += OnProcessEvent;
        
        // Initialize the process
        Process.Initialize(null);
    }
    
    public override void _ExitTree()
    {
        if (Process != null)
        {
            Process.StateChanged -= OnProcessStateChanged;
            Process.ProcessEvent -= OnProcessEvent;
            Process.Cleanup();
        }
    }
    
    protected virtual void OnProcessStateChanged(Dictionary<string, object> state)
    {
        GD.Print($"Process {Process.Id} state changed");
    }
    
    protected virtual void OnProcessEvent(string eventType)
    {
        GD.Print($"Process {Process.Id} event: {eventType}");
    }
    
    public virtual void Close()
    {
        EmitSignal(SignalName.ProcessClosed, Process.Id);
        QueueFree();
    }
}