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
    
    private Button CreateCardButton(Card card)
    {
        var button = new Button
        {
            Text = FormatCardText(card),
            Modulate = BorderColor
        };
        
        // Style the button
        var normalStyle = new StyleBoxFlat();
        normalStyle.BgColor = new Color(0, 0, 0, 0.8f);
        normalStyle.BorderColor = BorderColor;
        normalStyle.BorderWidthBottom = normalStyle.BorderWidthLeft = 
        normalStyle.BorderWidthRight = normalStyle.BorderWidthTop = 1;
        normalStyle.ContentMarginLeft = 10;
        normalStyle.ContentMarginRight = 10;
        
        var hoverStyle = normalStyle.Duplicate() as StyleBoxFlat;
        hoverStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        var pressedStyle = hoverStyle.Duplicate() as StyleBoxFlat;
        pressedStyle.BgColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        button.AddThemeStyleboxOverride("normal", normalStyle);
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
        
        button.Pressed += () => EmitSignal(SignalName.CardSelected, card);
        
        return button;
    }
    
    public void DisplayCards(List<Card> cards, string header = "Available Cards:")
    {
        _statusLabel.Text = header;
        
        // Clear existing cards except status label
        for (int i = _cardContainer.GetChildCount() - 1; i > 0; i--)
        {
            _cardContainer.GetChild(i).QueueFree();
        }
        
        // Add each card as a styled button
        foreach (var card in cards)
        {
            _cardContainer.AddChild(CreateCardButton(card));
        }
    }
    
    private string FormatCardText(Card card)
    {
        return $"[{card.GetSuitSymbol()}] {card.GetValueName()} of {card.GetSuitName()}";
    }
    
    [Signal]
    public delegate void CardSelectedEventHandler(Card card);
}