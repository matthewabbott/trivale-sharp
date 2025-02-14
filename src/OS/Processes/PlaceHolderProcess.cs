// src/OS/Processes/PlaceholderProcess.cs
using System.Collections.Generic;
using Godot;
using Trivale.Memory;

namespace Trivale.OS.Processes;

/// <summary>
/// A simple process that displays a title and message in the viewport.
/// Useful for placeholder/coming soon states.
/// </summary>
public class PlaceholderProcess : BaseProcess
{
    public override string Type => "Placeholder";
    
    private readonly string _title;
    private readonly string _message;
    private Control _display;
    
    public override Dictionary<string, float> ResourceRequirements => new()
    {
        ["MEM"] = 0.1f,
        ["CPU"] = 0.1f
    };
    
    public PlaceholderProcess(string id, string title, string message) : base(id)
    {
        _title = title;
        _message = message;
    }
    
    public override void Initialize(Dictionary<string, object> initialState)
    {
        base.Initialize(initialState);
        
        _display = new Control();
        var container = new VBoxContainer
        {
            AnchorsPreset = (int)Control.LayoutPreset.FullRect
        };
        _display.AddChild(container);
        
        // Add centered title
        var titleLabel = new Label
        {
            Text = _title,
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 50)
        };
        container.AddChild(titleLabel);
        
        // Add message with some padding
        var messageContainer = new MarginContainer();
        messageContainer.AddThemeConstantOverride("margin_left", 20);
        messageContainer.AddThemeConstantOverride("margin_right", 20);
        container.AddChild(messageContainer);
        
        var messageLabel = new Label
        {
            Text = _message,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        messageContainer.AddChild(messageLabel);
    }
    
    public override void Cleanup()
    {
        if (_display != null)
        {
            _display.QueueFree();
            _display = null;
        }
        
        base.Cleanup();
    }
}