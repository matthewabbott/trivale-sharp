// src/Memory/SlotManagement/SlotStatus.cs
namespace Trivale.Memory.SlotManagement;

public enum SlotStatus
{
    Empty,      // No process loaded
    Active,     // Process running normally
    Locked,     // Slot unavailable (not yet unlocked)
    Loading,    // Process is being loaded
    Suspended,  // Process loaded but not running
    Corrupted   // Error state
}