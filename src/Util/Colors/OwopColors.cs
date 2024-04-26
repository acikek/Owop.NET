using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Owop.Util;

/// <summary>
/// Contains colors used in the default OWOP client.
/// All colors are from the <a href="https://lospec.com/palette-list/endesga-16">ENDSEGA 16</a> palette.
/// </summary>
public static class OwopColors
{
    /// <summary>A color with an RGB value of <c>#E4A572</c>.</summary>
    public static readonly Color Sand = Color.FromArgb(228, 166, 114);

    /// <summary>A color with an RGB value of <c>#B86F50</c>.</summary>
    public static readonly Color Tan = Color.FromArgb(184, 111, 80);

    /// <summary>A color with an RGB value of <c>#743F39</c>.</summary>
    public static readonly Color Brown = Color.FromArgb(116, 63, 57);

    /// <summary>A color with an RGB value of <c>#3F2832</c>.</summary>
    public static readonly Color DarkBrown = Color.FromArgb(63, 40, 50);

    /// <summary>A color with an RGB value of <c>#9E2836</c>.</summary>
    public static readonly Color DarkRed = Color.FromArgb(158, 40, 53);

    /// <summary>A color with an RGB value of <c>#E53B44</c>.</summary>
    public static readonly Color Red = Color.FromArgb(229, 59, 68);

    /// <summary>A color with an RGB value of <c>#FB932B</c>.</summary>
    public static readonly Color Gold = Color.FromArgb(251, 146, 43);

    /// <summary>A color with an RGB value of <c>#FFE762</c>.</summary>
    public static readonly Color Yellow = Color.FromArgb(255, 231, 98);

    /// <summary>A color with an RGB value of <c>#63C64D</c>.</summary>
    public static readonly Color Lime = Color.FromArgb(99, 198, 77);

    /// <summary>A color with an RGB value of <c>#327345</c>.</summary>
    public static readonly Color Green = Color.FromArgb(50, 115, 69);

    /// <summary>A color with an RGB value of <c>#193D3F</c>.</summary>
    public static readonly Color DarkGreen = Color.FromArgb(25, 61, 63);

    /// <summary>A color with an RGB value of <c>#4F6781</c>.</summary>
    public static readonly Color Gray = Color.FromArgb(79, 103, 129);

    /// <summary>A color with an RGB value of <c>#AFBFD2</c>.</summary>
    public static readonly Color LightGray = Color.FromArgb(175, 191, 210);

    /// <summary>A color with an RGB value of <c>#FFFFFF</c>.</summary>
    public static readonly Color White = Color.White;

    /// <summary>A color with an RGB value of <c>#2CE8F4</c>.</summary>
    public static readonly Color Aqua = Color.FromArgb(44, 232, 244);

    /// <summary>A color with an RGB value of <c>#0484D1</c>.</summary>
    public static readonly Color Blue = Color.FromArgb(4, 132, 209);
}
