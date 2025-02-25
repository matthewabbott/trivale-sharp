// src/OS/Events/SystemEventBus.cs
using System;
using System.Collections.Generic;
using Godot;
using Trivale.Memory.SlotManagement;

namespace Trivale.OS.Events;

/// <summary>
/// Central event bus system that decouples processes and UI components.
/// 
/// This class follows the observer pattern to allow different parts of the system
/// to communicate without direct references to each other. Components can 
/// publish events and subscribe to them without knowing about other components.
/// 
/// Key event categories:
/// - Process lifecycle events (created, started, ended)
/// - Slot state events (allocation, deallocation, status changes)
/// - Resource events (memory usage, CPU usage)
/// - System events (startup, shutdown, mode changes)
/// </summary>
public class SystemEventBus
{
    private static SystemEventBus _instance;
    
    public static SystemEventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new SystemEventBus();
            }
            return _instance;
        }
    }
    
    // Process events
    public event Action<string> ProcessCreated;
    public event Action<string, string> ProcessStarted;
    public event Action<string> ProcessEnded;
    public event Action<string, Dictionary<string, object>> ProcessStateChanged;
    
    // Slot events
    public event Action<string, SlotStatus> SlotStatusChanged;
    public event Action<string> SlotUnlocked;
    public event Action<string> SlotLocked;
    public event Action<string, string> SlotParentChanged;
    
    // Resource events
    public event Action<float, float> SystemResourcesChanged; // memory, cpu
    public event Action<string, float, float> SlotResourcesChanged; // slotId, memory, cpu
    
    // Scene events
    public event Action<string> SceneLoaded;
    public event Action<string, bool> SceneUnloaded; // scenePath, returningToMainMenu
    
    // System events
    public event Action SystemStarted;
    public event Action SystemShutdown;
    public event Action<SystemMode> SystemModeChanged;
    
    // Publish methods for process events
    public void PublishProcessCreated(string processId)
    {
        ProcessCreated?.Invoke(processId);
    }
    
    public void PublishProcessStarted(string processId, string slotId)
    {
        ProcessStarted?.Invoke(processId, slotId);
    }
    
    public void PublishProcessEnded(string processId)
    {
        ProcessEnded?.Invoke(processId);
    }
    
    public void PublishProcessStateChanged(string processId, Dictionary<string, object> state)
    {
        ProcessStateChanged?.Invoke(processId, state);
    }
    
    // Publish methods for slot events
    public void PublishSlotStatusChanged(string slotId, SlotStatus status)
    {
        SlotStatusChanged?.Invoke(slotId, status);
    }
    
    public void PublishSlotUnlocked(string slotId)
    {
        SlotUnlocked?.Invoke(slotId);
    }
    
    public void PublishSlotLocked(string slotId)
    {
        SlotLocked?.Invoke(slotId);
    }
    
    public void PublishSlotParentChanged(string childSlotId, string parentSlotId)
    {
        SlotParentChanged?.Invoke(childSlotId, parentSlotId);
    }
    
    // Publish methods for resource events
    public void PublishSystemResourcesChanged(float memory, float cpu)
    {
        SystemResourcesChanged?.Invoke(memory, cpu);
    }
    
    public void PublishSlotResourcesChanged(string slotId, float memory, float cpu)
    {
        SlotResourcesChanged?.Invoke(slotId, memory, cpu);
    }
    
    // Publish methods for scene events
    public void PublishSceneLoaded(string scenePath)
    {
        SceneLoaded?.Invoke(scenePath);
    }
    
    public void PublishSceneUnloaded(string scenePath, bool returningToMainMenu)
    {
        SceneUnloaded?.Invoke(scenePath, returningToMainMenu);
    }
    
    // Publish methods for system events
    public void PublishSystemStarted()
    {
        SystemStarted?.Invoke();
    }
    
    public void PublishSystemShutdown()
    {
        SystemShutdown?.Invoke();
    }
    
    public void PublishSystemModeChanged(SystemMode mode)
    {
        SystemModeChanged?.Invoke(mode);
    }
}

/// <summary>
/// Represents the overall mode of the system.
/// </summary>
public enum SystemMode
{
    MainMenu,
    GameSession,
    Debug,
    Settings
}