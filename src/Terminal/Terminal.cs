// src/Terminal/Terminal.cs

using Godot;
using System.Collections.Generic;

namespace Trivale.Terminal;

public partial class Terminal : Control
{
    [Export]
    public string Title { get; set; } = "Terminal";
    
    [Export]
    public Color BorderColor { get; set; } = Colors.Green;
    
    private RichTextLabel _contentLabel;
    private LineEdit _inputLine;
    private Panel _titleBar;
    private Label _titleLabel;
    private ShaderMaterial _crtEffect;
    private List<string> _commandHistory = new();
    private int _historyPosition = -1;
    
    public override void _Ready()
    {
        SetupLayout();
        SetupEffects();
        ConnectSignals();
    }
    
    private void SetupLayout()
    {
        // Create title bar
        _titleBar = new Panel();
        AddChild(_titleBar);
        
        _titleLabel = new Label
        {
            Text = Title,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        _titleBar.AddChild(_titleLabel);
        
        // Create content area
        _contentLabel = new RichTextLabel
        {
            BbcodeEnabled = true,
            ScrollFollowing = true
        };
        AddChild(_contentLabel);
        
        // Create input line
        _inputLine = new LineEdit();
        AddChild(_inputLine);
    }
    
    private void SetupEffects()
    {
        _crtEffect = new ShaderMaterial();
        // TODO: Load and configure CRT shader
    }
    
    private void ConnectSignals()
    {
        _inputLine.TextSubmitted += OnInputSubmitted;
    }
    
    private void OnInputSubmitted(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        
        _commandHistory.Add(text);
        _historyPosition = _commandHistory.Count;
        _inputLine.Clear();
        
        ProcessCommand(text);
    }
    
    private void ProcessCommand(string command)
    {
        // TODO: Implement command processing
        AppendText($"> {command}\n");
    }
    
    public void AppendText(string text)
    {
        _contentLabel.AppendText(text);
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Up:
                    NavigateHistory(-1);
                    break;
                case Key.Down:
                    NavigateHistory(1);
                    break;
            }
        }
    }
    
    private void NavigateHistory(int direction)
    {
        if (_commandHistory.Count == 0) return;
        
        _historyPosition = Mathf.Clamp(
            _historyPosition + direction,
            0,
            _commandHistory.Count
        );
        
        if (_historyPosition < _commandHistory.Count)
        {
            _inputLine.Text = _commandHistory[_historyPosition];
            _inputLine.CaretColumn = _inputLine.Text.Length;
        }
        else
        {
            _inputLine.Clear();
        }
    }
}