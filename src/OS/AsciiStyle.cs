// src/OS/AsciiStyle.cs

namespace Trivale.OS;

public static class AsciiStyle
{
    // Box drawing characters
    public const string TOP_LEFT = "╔";
    public const string TOP_RIGHT = "╗";
    public const string BOTTOM_LEFT = "╚";
    public const string BOTTOM_RIGHT = "╝";
    public const string HORIZONTAL = "═";
    public const string VERTICAL = "║";
    public const string T_RIGHT = "╠";
    public const string T_LEFT = "╣";
    public const string T_DOWN = "╦";
    public const string T_UP = "╩";
    public const string CROSS = "╬";
    
    // Program Icons (can be replaced with custom ASCII art later)
    public const string PROGRAM_ICON = @"
┌──────┐
│ PRG  │
│ .EXE │
└──────┘";
    
    public const string SETTINGS_ICON = @"
┌──────┐
│ CFG  │
│ .SYS │
└──────┘";
    
    // Status bar decorators
    public const string BAR_EMPTY = "□";
    public const string BAR_FULL = "■";
    
    // Helper to create ASCII boxes
    public static string[] CreateBox(int width, int height, string title = "")
    {
        var box = new string[height];
        
        // Top border with title
        string titleBar = TOP_LEFT + HORIZONTAL.PadRight(width - 2, '═') + TOP_RIGHT;
        if (!string.IsNullOrEmpty(title))
        {
            int titlePos = (width - title.Length - 2) / 2;
            titleBar = TOP_LEFT + 
                      HORIZONTAL.PadRight(titlePos, '═') +
                      $" {title} " +
                      HORIZONTAL.PadRight(width - titlePos - title.Length - 4, '═') +
                      TOP_RIGHT;
        }
        box[0] = titleBar;
        
        // Middle rows
        for (int i = 1; i < height - 1; i++)
        {
            box[i] = VERTICAL + " ".PadRight(width - 2) + VERTICAL;
        }
        
        // Bottom border
        box[height - 1] = BOTTOM_LEFT + HORIZONTAL.PadRight(width - 2, '═') + BOTTOM_RIGHT;
        
        return box;
    }
    
    // Helper to create progress bars
    public static string CreateBar(float percentage, int width)
    {
        int filledCount = (int)(percentage * width);
        string bar = "";
        for (int i = 0; i < width; i++)
        {
            bar += (i < filledCount) ? BAR_FULL : BAR_EMPTY;
        }
        return bar;
    }
}