// src/Terminal/DesktopTerminal.cs

using Godot;
using System;
using System.Collections.Generic;

namespace Trivale.Terminal;

public partial class DesktopTerminal : TerminalWindow
{
    private RichTextLabel _outputText;
    private LineEdit _inputLine;
    private Dictionary<string, Action<string[]>> _commands;
    
    [Signal]
    public delegate void GameStartRequestedEventHandler(int numPlayers, int handSize);
    
    public override void _Ready()
    {
        base._Ready();
        WindowTitle = "Desktop";
        BorderColor = new Color(0, 1, 0.5f); // Cyan-green
        MinSize = new Vector2(400, 300);
        SetupTerminal();
        InitializeCommands();
    }
    
    private void SetupTerminal()
    {
        var container = new VBoxContainer();
        
        // Output text area
        _outputText = new RichTextLabel
        {
            BbcodeEnabled = true,
            CustomMinimumSize = new Vector2(0, 200),
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        container.AddChild(_outputText);
        
        // Input line
        _inputLine = new LineEdit();
        _inputLine.TextSubmitted += OnCommandEntered;
        container.AddChild(_inputLine);
        
        AddContent(container);
        
        // Welcome message
        PrintLine("Welcome to TrivalOS v0.1");
        PrintLine("Type 'help' for available commands.");
    }
    
    private void InitializeCommands()
    {
        _commands = new Dictionary<string, Action<string[]>>
        {
            ["help"] = _ => ShowHelp(),
            ["clear"] = _ => ClearOutput(),
            ["start"] = args => StartGame(args),
            ["exit"] = _ => GetTree().Quit()
        };
    }
    
    private void OnCommandEntered(string command)
    {
        PrintLine($"> {command}");
        _inputLine.Clear();
        
        var parts = command.Trim().ToLower().Split(' ');
        if (parts.Length == 0) return;
        
        var cmd = parts[0];
        var args = new string[parts.Length - 1];
        Array.Copy(parts, 1, args, 0, parts.Length - 1);
        
        if (_commands.TryGetValue(cmd, out var action))
        {
            action(args);
        }
        else
        {
            PrintLine($"Unknown command: {cmd}");
        }
    }
    
    private void ShowHelp()
    {
        PrintLine("Available commands:");
        PrintLine("  help         - Show this help");
        PrintLine("  clear        - Clear terminal output");
        PrintLine("  start [p] [h]- Start game with [p] players and [h] cards per hand");
        PrintLine("  exit         - Exit the game");
    }
    
    private void StartGame(string[] args)
    {
        int numPlayers = 4;
        int handSize = 5;
        
        if (args.Length >= 1 && int.TryParse(args[0], out int p))
            numPlayers = Math.Clamp(p, 2, 6);
            
        if (args.Length >= 2 && int.TryParse(args[1], out int h))
            handSize = Math.Clamp(h, 3, 13);
        
        PrintLine($"Starting game with {numPlayers} players, {handSize} cards each...");
        EmitSignal(SignalName.GameStartRequested, numPlayers, handSize);
    }
    
    private void ClearOutput()
    {
        _outputText.Clear();
    }
    
    public void PrintLine(string text)
    {
        _outputText.AppendText(text + "\n");
    }
}