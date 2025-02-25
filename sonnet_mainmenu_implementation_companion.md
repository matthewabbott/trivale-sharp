# Trivale MainMenu Implementation Companion

## Current State Overview

The Trivale project has recently undergone significant architectural improvements focused on the main menu system and core infrastructure. These changes have created a more maintainable, robust system with proper separation of concerns that resolves the "Cannot access disposed object" errors previously encountered.

## Core Architecture

The current architecture follows a clean separation of concerns:

1. **UI Components** - Focus only on visual representation
2. **Managers** - Handle business logic without UI dependencies
3. **Orchestrators** - Coordinate between UI and logic layers
4. **Event Bus** - Facilitates decoupled communication

### Key Components

1. **SimpleMainMenu** - Core layout manager that sets up the three-panel UI
2. **MainMenuScene** - Handles menu options and button interactions
3. **SceneOrchestrator** - Manages scene loading/unloading and display
4. **ProcessManager** - Handles process lifecycle (create, start, end)
5. **SlotManager** - Manages memory slots and their state
6. **SystemEventBus** - Central event system for decoupled communication
7. **SlotGridSystem/Display** - UI components for memory slot visualization

## Communication Flow

1. User actions → MainMenuScene → SceneOrchestrator → ProcessManager
2. Process/slot changes → SystemEventBus → UI components
3. Scene transitions → SceneOrchestrator → SystemEventBus → UI updates

## Key Files and Their Roles

| File | Purpose |
|------|---------|
| `SimpleMainMenu.cs` | Three-panel layout manager and initialization |
| `MainMenuScene.cs` | Menu UI with buttons for process selection |
| `SceneOrchestrator.cs` | Manages scene loading/unloading and visibility |
| `ProcessManager.cs` | Handles process lifecycle |
| `SlotManager.cs` | Manages memory slots and resources |
| `SystemEventBus.cs` | Central event communication system |
| `SlotGridSystem.cs` | UI representation of memory slots |
| `DebugScene.cs` | Test implementation for process/slot interaction |

## Signal Management Pattern

A key insight is the proper management of signals to prevent "disposed object" errors:

1. **Use named methods** instead of lambdas for signal connections
2. **Explicitly disconnect** signals in `_ExitTree()` methods
3. **Set references to null** after cleanup
4. **Check for null/validity** before accessing objects

## Implementation Progress

Against the `o3_main_menu_implementation_plan.txt`, we've completed:

- ✅ Phase 1: Basic Main Menu - Complete
- ✅ Phase 2: Process Management - Complete
- ⏳ Phase 3: Advanced Features - Partial (structure ready, implementation needed)
- ⏳ Phase 4: Testing and Refinement - In progress

## Remaining Tasks

1. **Process Previews** - Add visual previews in memory slots
2. **Drag-and-Drop** - Implement process drag-drop between viewport and slots
3. **Dynamic Slot Expansion** - Animate slot unlocking/expansion
4. **Resource Visualization** - Enhanced visualization of system resources
5. **Testing** - Add automated tests for core components

## Tech Design Alignment

The implementation follows the specs in `o1_mainmenu_tech_spec.txt` with some enhancements:

1. Uses a more robust event-based communication system
2. Better prepares for future expansion
3. More thoroughly separates UI from logic

## Extension Points

Key areas prepared for future extensions:

1. **Multiple Processes** - System can now handle multiple concurrent processes
2. **Resource Sharing** - Framework ready for cross-process resource sharing
3. **Process State Preservation** - Architecture supports state preservation/restoration
4. **Different Process Types** - Easy to add new process types

## Common Pitfalls to Avoid

1. **Never** do blind tree searches (`GetNode`, `FindNode`, etc.)
2. **Always** disconnect signals in `_ExitTree()`
3. **Always** null out references after cleanup
4. **Avoid** mixing UI and logic concerns
5. **Use** the event bus for cross-component communication

## Code Examples

### Proper Signal Handling
```csharp
// Connecting with named methods
_processManager.ProcessStarted += OnProcessStarted;

// Proper disconnection in _ExitTree
public override void _ExitTree()
{
    if (_processManager != null)
    {
        _processManager.ProcessStarted -= OnProcessStarted;
    }
    
    _processManager = null;
    base._ExitTree();
}
```

### Event Bus Usage
```csharp
// Publishing events
_eventBus.PublishProcessStarted(processId, slotId);

// Subscribing to events
_eventBus.ProcessStarted += (processId, slotId) => 
    GD.Print($"Process started: {processId} in slot {slotId}");
```

### Scene Management
```csharp
// Loading a scene
var scene = LoadSceneInstance(scenePath);
_loadedScenes[processId] = scene;
ShowScene(scene);

// Scene visibility toggling
private void ShowScene(Control scene)
{
    foreach (var child in _mainContent.GetChildren().OfType<Control>())
    {
        child.Visible = false;
    }
    
    scene.Visible = true;
}
```

## Conclusion

The codebase has been refactored to follow best practices for Godot game development with a focus on maintainability, separation of concerns, and robust reference management. The architecture now provides a solid foundation for implementing the remaining features while avoiding the common pitfalls that led to previous issues.