// src/Cards/Card.cs

using Godot;

namespace Trivale.Cards;

public enum Suit
{
    Data,    // Hearts    - Information flow
    Network, // Diamonds  - Infrastructure
    System,  // Clubs    - Process control
    Crypto   // Spades   - Security/encryption
}

public enum Value
{
    Two = 2,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,    // System access
    Queen,   // Admin privileges
    King,    // Root access
    Ace      // Master key
}

public partial class Card : Node2D
{
    [Export]
    public Suit Suit { get; set; }
    
    [Export]
    public Value Value { get; set; }
    
    private ShaderMaterial _effectMaterial;
    private Area2D _clickArea;
    
    public override void _Ready()
    {
        SetupVisuals();
        SetupInteraction();
    }
    
    private void SetupVisuals()
    {
        _effectMaterial = new ShaderMaterial();
        // TODO: Load and configure shader
    }
    
    private void SetupInteraction()
    {
        _clickArea = new Area2D();
        AddChild(_clickArea);
        
        var collisionShape = new CollisionShape2D();
        // TODO: Set up collision shape for card
        _clickArea.AddChild(collisionShape);
        
        _clickArea.InputEvent += OnInputEvent;
    }
    
    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        // TODO: Handle card interactions
    }
}