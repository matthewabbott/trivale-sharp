// src/Memory/SlotManagement/ISlotManager.cs
using System;
using System.Collections.Generic;
using Trivale.Memory.ProcessManagement;

namespace Trivale.Memory.SlotManagement;

/// <summary>
/// Manages the system's memory slots and resource allocation. Handles slot state,
/// unlocking, and resource tracking while remaining process-agnostic.
/// 
/// Communicates slot state changes via events, allowing the UI and process manager
/// to react accordingly. Only the first slot starts unlocked; others are unlocked
/// through gameplay.
/// </summary>

public interface ISlotManager
{
    event Action<string, SlotStatus> SlotStatusChanged;
    event Action<string> SlotUnlocked;
    event Action<string> SlotLocked;
    
    bool TryLoadProcessIntoSlot(IProcess process, out string slotId);
    void FreeSlot(string slotId);
    bool UnlockSlot(string slotId);
    IReadOnlyList<ISlot> GetAllSlots();
    ISlot GetSlot(string slotId);
    bool CanAllocateProcess(IProcess process);
    float GetAvailableMemory();
    float GetAvailableCpu();
}