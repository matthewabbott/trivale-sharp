// src/Memory/SlotManagement/ISlotManager.cs
using System;
using System.Collections.Generic;
using Trivale.Memory.ProcessManagement;

namespace Trivale.Memory.SlotManagement;

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