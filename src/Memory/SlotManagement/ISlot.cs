// src/Memory/SlotManagement/ISlot.cs
using Godot;
using System;
using System.Collections.Generic;
using Trivale.Memory.ProcessManagement;

namespace Trivale.Memory.SlotManagement;

public interface ISlot
{
    string Id { get; }
    SlotStatus Status { get; }
    Vector2I GridPosition { get; }
    float MemoryUsage { get; }
    float CpuUsage { get; }
    IProcess CurrentProcess { get; }
    bool IsUnlocked { get; }
    
    bool CanLoadProcess(IProcess process);
    void LoadProcess(IProcess process);
    void UnloadProcess();
    Dictionary<string, object> GetState();
    void Suspend();
    void Resume();
}