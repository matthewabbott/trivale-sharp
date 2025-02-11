// src/OS/TerminalConfig.cs
using Godot;

namespace Trivale.OS;

/// <summary>
/// Central configuration for terminal visual effects and behavior.
/// Provides consistent settings across all terminal instances.
/// </summary>
public static class TerminalConfig
{
    public static class CRTEffect
    {
        // Scan line configuration
        public const float ScanLineCount = 60.0f;
        public const float ScanLineOpacity = 0.1f;
        
        // Color and brightness
        public static readonly Color BaseColor = new(0, 1, 0); // Green
        public const float Brightness = 0.6f;
        
        // Effects
        public const float FlickerIntensity = 0.03f;
        
        /// <summary>
        /// Applies standard CRT effect settings to a ShaderMaterial
        /// </summary>
        public static void ApplyToMaterial(ShaderMaterial material)
        {
            material.SetShaderParameter("scan_line_count", ScanLineCount);
            material.SetShaderParameter("scan_line_opacity", ScanLineOpacity);
            material.SetShaderParameter("base_color", BaseColor);
            material.SetShaderParameter("brightness", Brightness);
            material.SetShaderParameter("flicker_intensity", FlickerIntensity);
        }
    }
    
    public static class Colors
    {
        // Standard terminal colors
        public static readonly Color Border = new(0, 1, 0);      // Bright green
        public static readonly Color DimBorder = new(0, 0.3f, 0); // Dimmed green
        public static readonly Color Background = new(0, 0.05f, 0);
        
        // Status colors
        public static readonly Color Warning = new(1, 0.5f, 0);  // Orange
        public static readonly Color Error = new(1, 0, 0);       // Red
        public static readonly Color Success = new(0, 1, 0);     // Green
    }
    
    public static class Layout
    {
        // Standard sizes and margins
        public const int WindowMargin = 20;
        public const int BorderWidth = 2;
        public const int MinWindowWidth = 400;
        public const int MinWindowHeight = 300;
        
        // Grid layout
        public const int MemSlotColumns = 2;
        public const int MemSlotWidth = 250;
    }
}