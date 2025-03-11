// src/Memory/ProcessManagement/ProcessManager.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Trivale.Memory.SlotManagement;
using Trivale.OS.Events;
using Trivale.OS.MainMenu.Processes;

namespace Trivale.Memory.ProcessManagement;

public partial class ProcessManager : Node, IProcessManager
{
    // These events are kept for backward compatibility but should be replaced with the event bus
    public event Action<string, string> ProcessStarted;
    public event Action<string> ProcessEnded;
    public event Action<string> ProcessStateChanged;
   
    private readonly Dictionary<string, IProcess> _processes = new();
    private readonly ISlotManager _slotManager;
    private readonly Dictionary<string, string> _processToSlot = new();
    private readonly SystemEventBus _eventBus;
    private readonly ProcessSlotRegistry _registry;
   
    public ProcessManager(ISlotManager slotManager, ProcessSlotRegistry registry)
    {
        _slotManager = slotManager;
        _registry = registry;
        _eventBus = SystemEventBus.Instance;
    }

    public string CreateMainMenuProcess(string specificId = "mainmenu")
    {
        return CreateProcess("MainMenu", null, specificId);
    }

    public string CreateProcess(string processType, Dictionary<string, object> initParams = null, string specificId = null)
    {
        var processId = specificId ?? $"{processType.ToLower()}_{DateTime.Now.Ticks}";
        
        IProcess newProcess = processType switch
        {
            "CardGame" => new CardGameMenuProcess(processId),
            "Debug" => new DebugMenuProcess(processId),
            "MainMenu" => new MainMenuProcess(processId),
            _ => null
        };
        
        if (newProcess == null)
        {
            GD.PrintErr($"Unknown process type: {processType}");
            return null;
        }

        // Hook up state change events from the process
        newProcess.StateChanged += (state) => OnProcessStateChanged(processId, state);
        _processes[processId] = newProcess;
        
        GD.Print($"Created process: {processId}");
        
        // Publish event through the bus
        _eventBus.PublishProcessCreated(processId);
        
        return processId;
    }
    
    private void InitializeMainMenu()
    {
        GD.Print("Attempting to initialize main menu process...");
        var mainMenuProcessId = CreateMainMenuProcess();
        
        if (mainMenuProcessId == null)
        {
            GD.PrintErr("Failed to create MainMenuProcess");
            return;
        }
        
        GD.Print($"Created MainMenuProcess with ID: {mainMenuProcessId}");
        
        if (StartProcess(mainMenuProcessId, "slot_0_0", out string slotId))
        {
            GD.Print($"Successfully started MainMenuProcess in slot {slotId}");
            
            // Set as active process after starting
            _registry.SetActiveProcess(mainMenuProcessId);
        }
        else
        {
            GD.PrintErr($"Failed to start MainMenuProcess (ID: {mainMenuProcessId})");
        }
    }

    public override void _Ready()
    {
        GD.Print("ProcessManager._Ready called");
        
        // Initialize the main menu process
        InitializeMainMenu();
    }
    public bool StartProcess(string processId, out string slotId)
    {
        // Call the overload with null preferredSlotId
        return StartProcess(processId, null, out slotId);
    }

    public bool StartProcess(string processId, string preferredSlotId, out string slotId)
    {
        slotId = null;
        if (!_processes.TryGetValue(processId, out var process))
        {
            GD.PrintErr($"Process not found: {processId}");
            return false;
        }
        
        // For MainMenu or when a specific slot is requested
        if (process.Type == "MainMenu" || !string.IsNullOrEmpty(preferredSlotId))
        {
            string targetSlotId = preferredSlotId ?? "slot_0_0"; // Use slot_0_0 by default for MainMenu
            
            if (_slotManager.TryLoadProcessIntoSpecificSlot(process, targetSlotId))
            {
                slotId = targetSlotId;
                _processToSlot[processId] = slotId;
                
                // Register the process-slot mapping in the registry
                _registry.RegisterProcessSlot(processId, slotId);
                
                // Publish event through the bus
                _eventBus.PublishProcessStarted(processId, slotId);
                
                // Legacy event invocation
                ProcessStarted?.Invoke(processId, slotId);
                
                GD.Print($"Started process {processId} ({process.Type}) in specific slot {slotId}");
                return true;
            }
            else
            {
                GD.PrintErr($"Failed to load process {processId} into specific slot {targetSlotId}");
                return false;
            }
        }
        // Original behavior for other processes
        else if (_slotManager.TryLoadProcessIntoSlot(process, out slotId))
        {
            _processToSlot[processId] = slotId;
            
            // Register the process-slot mapping in the registry
            _registry.RegisterProcessSlot(processId, slotId);
            
            // Publish event through the bus
            _eventBus.PublishProcessStarted(processId, slotId);
            
            // Legacy event invocation
            ProcessStarted?.Invoke(processId, slotId);
            
            return true;
        }
        
        return false;
    }
   
    public bool UnloadProcess(string processId)
    {
        if (!_processes.TryGetValue(processId, out var process))
        {
            GD.PrintErr($"Process not found: {processId}");
            return false;
        }
        
        if (_processToSlot.TryGetValue(processId, out var slotId))
        {
            _slotManager.FreeSlot(slotId);
            _processToSlot.Remove(processId);
            
            // Unregister from the registry
            _registry.UnregisterProcess(processId);
        }
        
        process.Cleanup();
        _processes.Remove(processId);
        
        // Publish event through the bus
        _eventBus.PublishProcessEnded(processId);
        
        // Legacy event invocation
        ProcessEnded?.Invoke(processId);
        
        return true;
    }
   
    public IProcess GetProcess(string processId)
    {
        return _processes.TryGetValue(processId, out var process) ? process : null;
    }
   
    public IReadOnlyList<string> GetActiveProcessIds()
    {
        return _processes.Keys.ToList();
    }
   
    private void OnProcessStateChanged(string processId, Dictionary<string, object> newState)
    {
        // Publish state change through the bus
        _eventBus.PublishProcessStateChanged(processId, newState);
        
        // Legacy event invocation
        ProcessStateChanged?.Invoke(processId);
    }
    
    public override void _ExitTree()
    {
        // Clean up all processes
        foreach (var processId in GetActiveProcessIds().ToList())
        {
            UnloadProcess(processId);
        }
        
        base._ExitTree();
    }
    
    // Add a helper method to get the slot ID for a process
    public string GetProcessSlotId(string processId)
    {
        return _processToSlot.TryGetValue(processId, out var slotId) ? slotId : null;
    }
}