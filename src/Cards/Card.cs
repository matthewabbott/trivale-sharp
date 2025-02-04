// src/Cards/Card.cs

using Godot;
using System;

namespace Trivale.Cards;

public enum Suit
{
    DataFlow,    // Hearts    - Information streams and memory
    Crypto,      // Diamonds  - Digital currency and resources
    Infra,       // Clubs     - System backbone and architecture
    Process,     // Spades    - Program execution and logic
    None,        // Used for special cases
    NoTrump      // Used for game state
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

public enum AIBehavior
{
    None,           // No special behavior
    OrderedPlay,    // Plays in a specific order
    TrickTaker,     // Tries to win tricks
    TrickDodger    // Tries to avoid winning
}

public partial class Card : Node2D
{
    [Export]
    public Suit Suit { get; set; }
    
    [Export]
    public Value Value { get; set; }
    
    [Export]
    public int Owner { get; set; } = -1; // -1 means unowned
    
    [Export]
    public AIBehavior Behavior { get; set; } = AIBehavior.None;
    
    // For OrderedPlay behavior
    [Export]
    public int PlayOrder { get; set; } = -1;
    
    public string Id => $"{GetValueName().Substring(0, 1)}{GetSuitName().Substring(0, 1)}";
    
    private static readonly string[] ValueNames = 
    {
        "", "", "2", "3", "4", "5", "6", "7", "8", "9", "10", 
        "Jack", "Queen", "King", "Ace"
    };
    
    private static readonly string[] SuitNames = 
    {
        "Data Flow", "Cryptocurrency", "Infrastructure", "Process Control", 
        "None", "No-Trump"
    };
    
    private static readonly string[] SuitSymbols =
    {
        "♥", "♦", "♣", "♠", "", ""
    };
    
    private ShaderMaterial _effectMaterial;
    private Area2D _clickArea;
    private Sprite2D _sprite;
    private Label _cardText;
    
    // Signals
    [Signal]
    public delegate void CardClickedEventHandler(Card card);
    
    public override void _Ready()
    {
        SetupVisuals();
        SetupInteraction();
        UpdateCardDisplay();
    }
    
    private void SetupVisuals()
    {
        _effectMaterial = new ShaderMaterial();
        
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _cardText = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        AddChild(_cardText);
        
        // TODO: Load and set up suit-specific shader effects
        SetupSuitEffects();
    }
    
    private void SetupSuitEffects()
    {
        // Each suit gets its own visual effects
        switch (Suit)
        {
            case Suit.DataFlow:
                // Data stream particles, memory core glow
                break;
            case Suit.Crypto:
                // Market data overlays, transaction effects
                break;
            case Suit.Infra:
                // Blueprint grid, network lines
                break;
            case Suit.Process:
                // Command line overlay, execution visualization
                break;
        }
    }
    
    private void SetupInteraction()
    {
        _clickArea = new Area2D();
        AddChild(_clickArea);
        
        var collisionShape = new CollisionShape2D();
        var shape = new RectangleShape2D();
        shape.Size = new Vector2(200, 280); // Standard card size
        collisionShape.Shape = shape;
        _clickArea.AddChild(collisionShape);
        
        _clickArea.InputEvent += OnInputEvent;
    }
    
    public void UpdateCardDisplay()
    {
        if (_cardText != null)
        {
            _cardText.Text = GetDisplayText();
        }
    }
    
    private string GetDisplayText()
    {
        string baseText = $"{GetValueName()} of {GetSuitName()}";
        if (Behavior != AIBehavior.None)
        {
            baseText += $"\n[{Behavior}]";
            if (Behavior == AIBehavior.OrderedPlay && PlayOrder >= 0)
            {
                baseText += $" ({PlayOrder})";
            }
        }
        return baseText;
    }
    
    public string GetValueName() => ValueNames[(int)Value];
    public string GetSuitName() => SuitNames[(int)Suit];
    public string GetSuitSymbol() => SuitSymbols[(int)Suit];
    public string GetFullName() => $"{GetValueName()} of {GetSuitName()}";
    
    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && 
            mouseEvent.ButtonIndex == MouseButton.Left && 
            mouseEvent.Pressed)
        {
            EmitSignal(SignalName.CardClicked, this);
        }
    }
    
    public static int CompareCards(Card card1, Card card2, Suit trumpSuit)
    {
        if (card1.Suit == card2.Suit)
        {
            return ((int)card1.Value).CompareTo((int)card2.Value);
        }
        
        if (trumpSuit != Suit.None && trumpSuit != Suit.NoTrump)
        {
            if (card1.Suit == trumpSuit) return 1;
            if (card2.Suit == trumpSuit) return -1;
        }
        
        return 0;  // Different non-trump suits
    }
    
    public Card Duplicate()
    {
        var card = new Card
        {
            Suit = Suit,
            Value = Value,
            Owner = Owner,
            Behavior = Behavior,
            PlayOrder = PlayOrder
        };
        return card;
    }
}