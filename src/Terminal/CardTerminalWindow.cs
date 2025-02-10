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
    private string _currentHeader = "";
    private List<Card> _currentCards = new();
    private bool _isPreviewMode = false;
    
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
    
    [Signal]
    public delegate void CardHoveredEventHandler(Card card);

    [Signal]
    public delegate void CardUnhoveredEventHandler(Card card);

    [Signal]
    public delegate void CardSelectedEventHandler(Card card);

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
        button.MouseEntered += () => EmitSignal(SignalName.CardHovered, card);
        button.MouseExited += () => EmitSignal(SignalName.CardUnhovered, card);
        
        return button;
    }
    
    public void DisplayCards(List<Card> cards, string header = "Available Cards:", bool isPreview = false)
    {
        // If nothing has changed, don't update
        if (_currentHeader == header && CardsMatch(cards) && _isPreviewMode == isPreview)
        {
            return;
        }

        _currentHeader = header;
        _currentCards = new List<Card>(cards);
        _isPreviewMode = isPreview;
        
        CallDeferred(nameof(UpdateCardDisplay));
    }

    private bool CardsMatch(List<Card> newCards)
    {
        if (_currentCards.Count != newCards.Count) return false;
        
        for (int i = 0; i < _currentCards.Count; i++)
        {
            if (_currentCards[i].Id != newCards[i].Id) return false;
        }
        
        return true;
    }
    
    private void UpdateCardDisplay()
    {
        if (_cardContainer == null) return;

        // Clear existing cards except status label
        for (int i = _cardContainer.GetChildCount() - 1; i > 0; i--)
        {
            _cardContainer.GetChild(i).QueueFree();
        }

        // Update header
        _statusLabel.Text = _currentHeader;
        
        // Add each card as a styled button
        foreach (var card in _currentCards)
        {
            _cardContainer.AddChild(CreateCardButton(card));
        }
    }
    
    private string FormatCardText(Card card)
    {
        string text = $"[{card.GetSuitSymbol()}] {card.GetValueName()} of {card.GetSuitName()}";
        if (_isPreviewMode)
        {
            text = "(Preview) " + text;
        }
        return text;
    }
}