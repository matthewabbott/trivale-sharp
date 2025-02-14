// src/UI/Components/MemorySlotDisplay.cs
using Godot;
using Trivale.Memory;
using Trivale.OS;

namespace Trivale.UI.Components;

public partial class MemorySlotDisplay : Control
{
    private Panel _statusBox;
    private Label _processLabel;
    private ColorRect _statusIndicator;
    
    // TODO: Add Node for process thumbnail/preview
    private Control _previewPlaceholder;
    
    private IMemorySlot _slot;
    private bool _isSlot0;
    
    [Signal]
    public delegate void SlotSelectedEventHandler(string slotId);
    
    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(200, 40);
        
        var hbox = new HBoxContainer
        {
            AnchorsPreset = (int)LayoutPreset.FullRect
        };
        AddChild(hbox);
        
        // Status indicator square
        _statusIndicator = new ColorRect
        {
            CustomMinimumSize = new Vector2(20, 20)
        };
        hbox.AddChild(_statusIndicator);
        
        // Process name/status
        _processLabel = new Label
        {
            Text = "EMPTY",
            SizeFlagsHorizontal = SizeFlags.Fill,
            VerticalAlignment = VerticalAlignment.Center,
            Theme = UIThemeManager.Instance.CreateTheme()
        };
        hbox.AddChild(_processLabel);
        
        // Make the whole control clickable
        MouseFilter = MouseFilterEnum.Stop;
        GuiInput += OnGuiInput;
        
        // TODO: Add placeholder for process preview/thumbnail
        // This will be replaced with actual preview rendering later
        _previewPlaceholder = new Control();
    }
    
    public void UpdateSlot(IMemorySlot slot, bool isSlot0 = false)
    {
        _slot = slot;
        _isSlot0 = isSlot0;
        
        if (slot == null)
        {
            _statusIndicator.Color = new Color(0.2f, 0.2f, 0.2f);
            _processLabel.Text = _isSlot0 ? "READY FOR INPUT" : "EMPTY";
            return;
        }
        
        // Get base color for slot status
        var baseColor = slot.Status switch
        {
            SlotStatus.Active => TerminalConfig.Colors.Success,
            SlotStatus.Corrupted => TerminalConfig.Colors.Error,
            SlotStatus.Loading => TerminalConfig.Colors.Warning,
            _ => new Color(0.2f, 0.2f, 0.2f)
        };
        
        // For Slot 0, tint the color towards purple
        if (_isSlot0)
        {
            baseColor = new Color(
                baseColor.R * 0.8f + 0.2f,  // Reduce red slightly
                baseColor.G * 0.5f,         // Reduce green more
                baseColor.B * 0.8f + 0.2f   // Increase blue
            );
        }
        
        _statusIndicator.Color = baseColor;
        
        // Update process label
        string statusText = slot.CurrentProcess?.Type ?? "EMPTY";
        if (_isSlot0)
        {
            statusText = slot.CurrentProcess == null ? "READY FOR INPUT" : $"LOADED: {statusText}";
        }
        _processLabel.Text = statusText;
    }
    
    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton &&
            mouseButton.ButtonIndex == MouseButton.Left &&
            mouseButton.Pressed)
        {
            EmitSignal(SignalName.SlotSelected, _slot?.Id ?? "");
        }
    }
}