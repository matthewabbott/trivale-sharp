// src/UI/Components/ResourcePanel.cs
using Godot;
using System.Linq;
using Trivale.Memory.SlotManagement;

namespace Trivale.UI.Components;

/// <summary>
/// Displays system-wide resource usage information based on active slots.
/// Shows aggregate MEM and CPU usage, as well as available resources.
/// </summary>
public partial class ResourcePanel : Control
{
    private ISlotManager _slotManager;
    private RichTextLabel _resourceInfo;
    
    // Resource thresholds (can be made customizable later)
    private const float LOW_USAGE = 0.3f;     // 0-30% - Green
    private const float MEDIUM_USAGE = 0.7f;  // 31-70% - Yellow
    // 71-100% - Red
    
    public void Initialize(ISlotManager slotManager)
    {
        _slotManager = slotManager;
        _slotManager.SlotStatusChanged += OnSlotStatusChanged;
    }
    
    public override void _Ready()
    {
        // Set up container with title
        var vbox = new VBoxContainer
        {
            AnchorRight = 1,
            AnchorBottom = 1,
            SizeFlagsVertical = SizeFlags.Fill,
            SizeFlagsHorizontal = SizeFlags.Fill
        };
        AddChild(vbox);
        
        // Title
        var titleLabel = new Label
        {
            Text = "RESOURCES:",
            SizeFlagsHorizontal = SizeFlags.Fill
        };
        vbox.AddChild(titleLabel);
        
        // Separator
        var separator = new HSeparator();
        vbox.AddChild(separator);
        
        // Resource display with rich text for coloring
        _resourceInfo = new RichTextLabel
        {
            BbcodeEnabled = true,
            FitContent = true,
            SizeFlagsVertical = SizeFlags.Fill,
            SizeFlagsHorizontal = SizeFlags.Fill
        };
        vbox.AddChild(_resourceInfo);
        
        UpdateDisplay();
    }
    
    private void OnSlotStatusChanged(string slotId, SlotStatus status)
    {
        UpdateDisplay();
    }
    
    public void UpdateDisplay()
    {
        if (_slotManager == null)
        {
            _resourceInfo.Text = "System Resources Not Available";
            return;
        }
        
        var slots = _slotManager.GetAllSlots();
        
        // Calculate total resources
        float totalMemory = _slotManager.GetAvailableMemory();
        float totalCpu = _slotManager.GetAvailableCpu();
        
        // Calculate used resources
        float usedMemory = slots.Sum(s => s.MemoryUsage);
        float usedCpu = slots.Sum(s => s.CpuUsage);
        
        // Calculate available resources
        float availableMem = totalMemory - usedMemory;
        float availableCpu = totalCpu - usedCpu;
        
        // Calculate usage percentages
        float memPercentage = (usedMemory / totalMemory);
        float cpuPercentage = (usedCpu / totalCpu);
        
        // Get colors based on usage
        string memColor = GetResourceColor(memPercentage);
        string cpuColor = GetResourceColor(cpuPercentage);
        
        // Build the display text
        var text = "[b]SYSTEM USAGE[/b]\n\n";
        
        // Memory usage
        text += $"MEM: [color={memColor}]{usedMemory:F1}/{totalMemory:F1} ({memPercentage:P0})[/color]\n";
        text += BuildResourceBar(memPercentage, 20, memColor) + "\n\n";
        
        // CPU usage
        text += $"CPU: [color={cpuColor}]{usedCpu:F1}/{totalCpu:F1} ({cpuPercentage:P0})[/color]\n";
        text += BuildResourceBar(cpuPercentage, 20, cpuColor) + "\n\n";
        
        // Active process count
        int activeProcessCount = slots.Count(s => s.Status == SlotStatus.Active);
        text += $"ACTIVE PROCESSES: {activeProcessCount}\n\n";
        
        // Available resources
        text += "[b]AVAILABLE RESOURCES[/b]\n\n";
        text += $"MEM: {availableMem:F1}\n";
        text += $"CPU: {availableCpu:F1}\n";
        
        _resourceInfo.Text = text;
    }
    
    private string GetResourceColor(float usage)
    {
        if (usage <= LOW_USAGE)
            return "#00FF00"; // Green for low usage
        else if (usage <= MEDIUM_USAGE)
            return "#FFFF00"; // Yellow for medium usage
        else
            return "#FF0000"; // Red for high usage
    }
    
    private string BuildResourceBar(float percentage, int width, string color)
    {
        int filled = (int)(percentage * width);
        string bar = "[";
        
        for (int i = 0; i < width; i++)
        {
            if (i < filled)
                bar += $"[color={color}]■[/color]";
            else
                bar += "□";
        }
        
        bar += "]";
        return bar;
    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        
        if (_slotManager != null)
        {
            _slotManager.SlotStatusChanged -= OnSlotStatusChanged;
        }
    }
}