
## File Structure

```
trivale-sharp/
├── .godot/          # Godot cache and settings
├── Assets/          # Game assets
│   ├── Fonts/
│   │   └── JetBrainsMono-Regular.ttf
│   └── Shaders/
│       ├── ascii_border.gdshader
│       └── crt_effect.gdshader
├── src/            # C# source code
│   ├── Cards/      # Card game system
│   │   └── Card.cs
│   ├── Game/       # Core game logic
│   │   ├── GameState.cs
│   │   └── GameConfiguration.cs
│   ├── Encounters/ # Encounter system
│   │   ├── IEncounter.cs
│   │   ├── BaseEncounter.cs
│   │   ├── CardGameEncounter.cs
│   │   ├── EncounterManager.cs
│   │   └── Scenes/
│   │       ├── EncounterScene.cs
│   │       └── CardEncounterScene.cs
│   ├── OS/         # Operating system interface
│   │   ├── ProgramInfo.cs
│   │   ├── SystemDesktop.cs
│   │   ├── UIThemeManager.cs
│   │   └── WindowManager.cs
│   ├── Tests/      # Test scenes
│   │   ├── GameTestScene.cs
│   │   ├── TerminalTestScene.cs
│   │   └── EncounterTestScene.cs
│   ├── UI/         # UI components and styles
│   │   ├── Components/
│   │   │   └── TerminalButton.cs
│   │   └── Styles/
│   │       └── TerminalStyles.cs
│   └── Utils/      # Utility classes
│       └── NodeExtensions.cs
├── project.godot    # Godot project file
├── Trivale.sln     # C# solution file
└── Trivale.csproj  # C# project file
```