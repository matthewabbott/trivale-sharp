// src/Memory/SlotStatus.cs
namespace Trivale.Memory;

public enum SlotStatus
{
    Empty,      // No process loaded
    Loading,    // Process is being loaded
    Active,     // Process is running normally
    Suspended,  // Process is loaded but not running
    Corrupted,  // Process has encountered an error
    Locked      // Slot is unavailable (e.g., not yet unlocked)
}