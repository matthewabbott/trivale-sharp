// src/Memory/ProcessManagement/IProcessManager.cs
using System;
using System.Collections.Generic;

namespace Trivale.Memory.ProcessManagement;

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