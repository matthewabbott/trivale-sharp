// src/Terminal/CardTerminalWindow.cs

using Godot;
using System;
using System.Collections.Generic;
using Trivale.Cards;

namespace Trivale.Terminal;

public partial class CardTerminalWindow : TerminalWindow
{
    private VBoxContainer _cardContainer;
    private Label _statusLabel;
    
    public override void _Ready()
    {
        base._Ready();
        WindowTitle = "Card Terminal";
        SetupCardDisplay();
    }
    
    private void SetupCardDisplay()
    {
        _cardContainer = new VBoxContainer();
        
        _statusLabel = new Label
        {
            Text = "Card Status:",
            Modulate = BorderColor
        };
        _cardContainer.AddChild(_statusLabel);
        
        AddContent(_cardContainer);
    }
    
    public void DisplayCards(List<Card> cards, string header = "Available Cards:")
    {
        _statusLabel.Text = header;
        
        // Clear existing cards except status label
        for (int i = _cardContainer.GetChildCount() - 1; i > 0; i--)
        {
            _cardContainer.GetChild(i).QueueFree();
        }
        
        // Add each card as a button
        foreach (var card in cards)
        {
            var cardButton = new Button
            {
                Text = FormatCardText(card),
                Modulate = BorderColor
            };
            cardButton.Pressed += () => EmitSignal(SignalName.CardSelected, card);
            _cardContainer.AddChild(cardButton);
        }
    }
    
    private string FormatCardText(Card card)
    {
        return $"[{card.GetSuitSymbol()}] {card.GetValueName()} of {card.GetSuitName()}";
    }
    
    [Signal]
    public delegate void CardSelectedEventHandler(Card card);
}