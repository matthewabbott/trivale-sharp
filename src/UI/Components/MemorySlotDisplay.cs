// src/UI/Components/MemorySlotDisplay.cs
// NOTE: ON NOTICE: we might want to delete this file or repurpose it as part of the slotmanagement refactor
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
    
    public void UpdateSlot(IMemorySlot slot)
    {
        _slot = slot;
        
        if (slot == null)
        {
            _statusIndicator.Color = new Color(0.2f, 0.2f, 0.2f);
            _processLabel.Text = "EMPTY";
            return;
        }
        
        // Update status indicator
        _statusIndicator.Color = slot.Status switch
        {
            SlotStatus.Active => TerminalConfig.Colors.Success,
            SlotStatus.Corrupted => TerminalConfig.Colors.Error,
            SlotStatus.Loading => TerminalConfig.Colors.Warning,
            _ => new Color(0.2f, 0.2f, 0.2f)
        };
        
        // Update process label
        _processLabel.Text = slot.CurrentProcess?.Type ?? "EMPTY";
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