﻿using System.Numerics;
using Gommon;
using Silk.NET.SDL;

namespace Volte.UI;

public static class ColorExtensions
{
    public static System.Drawing.Color AsColor(this Vector3 vec3)
        => System.Drawing.Color.FromArgb(255,
            (int)(vec3.X.CoerceAtMost(1) * 255),
            (int)(vec3.Y.CoerceAtMost(1) * 255),
            (int)(vec3.Z.CoerceAtMost(1) * 255));

    public static Vector3 AsVec3(this System.Drawing.Color color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f);
    
    public static Vector4 AsVec4(this System.Drawing.Color color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    
    public static Vector3 AsVec3(this Color color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f);
    
    public static Vector4 AsVec4(this Color color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
}