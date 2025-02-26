## README

# Trivale (Working Title)

A **terminal-based cyberpunk card game** built in Godot 4 (C#).  
This project is currently a **work in progress**—most of the codebase is either placeholder logic or partially implemented systems (especially the memory slots and main menu).

### Features (Planned & In Progress)

- **Terminal UI & CRT Effects**  
  Retro-futuristic hacking interface with scanlines, phosphor glow, and ASCII-inspired visuals.
- **Memory Slot System**  
  Each encounter or “process” runs inside a memory slot with resource constraints.  
- **Card Game Mechanics**  
  Prototype of a trick-taking card system integrated into the hacking “narrative.”
- **Scene Orchestration**  
  A scene manager that loads/unloads “processes” (sub-games or debug windows) into memory slots.
- **Modular Architecture**  
  Service-based architecture for easy extension, though many services are still stubs or incomplete.

### Current Status

- **Minimal Main Menu**: Just a simple placeholder UI that can load a “CardGameScene” or “DebugScene.”  
- **Memory Management**: Early prototype of slot-based resource usage (locking/unlocking, partial UI).  
- **Card Mechanics**: Basic trick-taking logic in place, but not yet integrated with final UI.  
- **Processes**: The code includes `ProcessManager`, `SlotManager`, etc., but they’re partially wired up.  
- **Design Docs**: Architecture/design docs, theming notes, and assorted WIP docs are present in the repo.

### File Structure

```
trivale-sharp/
├── .godot/               # Godot cache and settings
├── Assets/               # Game assets (fonts, shaders)
│   ├── Fonts/
│   │   └── JetBrainsMono-Regular.ttf
│   └── Shaders/
│       ├── ascii_border.gdshader
│       └── crt_effect.gdshader
├── Scenes/               # Godot scene files
│   └── MainMenu/
│       ├── MainMenuScene.tscn
│       ├── DebugScene.tscn
│       └── SimpleMainMenu.tscn
├── src/                  # C# source code (core logic, OS interface, UI, etc.)
│   ├── Game/             # Card game domain & services
│   ├── Memory/           # Process & slot management
│   ├── OS/               # System/desktop simulation & scene orchestration
│   ├── Terminal/         # Terminal windows and UI
│   ├── Tests/            # Test scenes & partial integration tests
│   └── UI/               # UI components (SlotGrid, ResourcePanel, etc.)
├── project.godot         # Godot project file
├── Trivale.sln           # C# solution file
└── Trivale.csproj        # C# project file
```

### Getting Started

1. **Requirements**  
   - [Godot 4.x](https://godotengine.org/) with C# support  
   - .NET 6.0 SDK (if you want to build from the command line)

2. **Running the Project**  
   - Clone or download this repo.  
   - Open the project folder in Godot.  
   - Run the `SimpleMainMenu.tscn` or `MainMenuScene.tscn`.  
   - Expect placeholders and incomplete features!