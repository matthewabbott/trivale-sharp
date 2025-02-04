// src/OS/ProgramInfo.cs

using System;

namespace Trivale.OS;

public class ProgramInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string AsciiArt { get; set; }
    public string IconPath { get; set; } // For future graphical assets
}