// src/Utils/NodeExtensions.cs

using Godot;
using System.Collections.Generic;

namespace Trivale.Utils;

public static class NodeExtensions
{
    public static void QueueFreeChildren(this Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            child.QueueFree();
        }
    }
}