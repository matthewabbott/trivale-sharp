
## File Structure

trivale-sharp/
├── .godot/          # Godot cache and settings (git ignored)
├── project.godot    # Godot project file
├── Trivale.sln     # C# solution file
├── Trivale.csproj  # C# project file
├── src/            # C# code
│   ├── Cards/      # Card system
│   ├── Terminal/   # Terminal UI system
│   ├── Game/       # Core game logic
│   └── Utils/      # Utilities and helpers
├── Assets/         # Game assets
│   ├── Fonts/
│   ├── Shaders/
│   └── Textures/
└── Scenes/         # Godot scenes
    ├── Main.tscn
    ├── Terminal.tscn
    └── Card.tscn