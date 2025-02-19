// src/Memory/ProcessManagement/IProcessManager.cs
using System;
using System.Collections.Generic;

namespace Trivale.Memory.ProcessManagement;

/// <summary>
/// Manages process lifecycle and coordinates with the slot system. Acts as the
/// central authority for process creation, loading, and cleanup.
/// 
/// The ProcessManager owns the relationship between processes and slots, delegating
/// slot management to the SlotManager while maintaining the process-to-slot mapping
/// and handling resource cleanup.
/// </summary>

public interface IProcessManager
{
    event Action<string, string> ProcessStarted;  // processId, slotId
    event Action<string> ProcessEnded;           // processId
    event Action<string> ProcessStateChanged;    // processId
    
    string CreateProcess(string processType, Dictionary<string, object> initParams = null);
    bool StartProcess(string processId, out string slotId);
    bool UnloadProcess(string processId);
    IProcess GetProcess(string processId);
    IReadOnlyList<string> GetActiveProcessIds();
}