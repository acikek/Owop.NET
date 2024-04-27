// Adapted from https://github.com/FreneticLLC/FreneticGameEngine/blob/master/FGECore/MathHelpers/Vector2i.cs

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Owop.Game;

namespace Owop.Util;

/// <summary>
/// Represents a position within the world, or a 2D vector of integers.
/// Occupies 8 bytes, calculated as 4 * 2, as it has 2 fields (X, Y) each occupying 4 bytes (an integer).
/// </summary>
/// <param name="x">X coordinate.</param>
/// <param name="y">Y coordinate.</param>
[StructLayout(LayoutKind.Explicit)]
public struct Position(int x, int y) : IEquatable<Position>
{
    /// <summary>The origin (zero) position.</summary>
    public static readonly Position Origin = new(0, 0);

    /// <summary>The x coordinate.</summary>
    [FieldOffset(0)]
    public int X = x;

    /// <summary>The y coordinate.</summary>
    [FieldOffset(4)]
    public int Y = y;

    /// <summary>Gets a cheap hash code.</summary>
    /// <returns>The hash code.</returns>
    public override readonly int GetHashCode() => X * 23 + Y;

    /// <summary>Compares equality between this and another position.</summary>
    /// <param name="other">The other position.</param>
    /// <returns>Whether they are equal.</returns>
    public override readonly bool Equals(object? other)
    {
        if (other is not Position pos)
        {
            return false;
        }
        return Equals(pos);
    }

    /// <summary>Compares equality between this and another position.</summary>
    /// <param name="other">The other position.</param>
    /// <returns>Whether they are equal.</returns>
    public readonly bool Equals(Position other) => other.X == X && other.Y == Y;

    /// <summary>Converts this position to a floating point vector.</summary>
    /// <returns>The vector.</returns>
    public readonly Vector2 ToVector() => new(X, Y);

    /// <summary>Deconstructs this position to a 2D tuple of integers.</summary>
    /// <returns>The tuple.</returns>
    public readonly (int, int) ToTuple() => (X, Y);

    /// <summary>Converts this position to a drawing point.</summary>
    /// <returns>The point.</returns>
    public readonly Point ToPoint() => new(X, Y);

    /// <summary>Converts a <b>precise</b> position to a pixel-based world position.</summary>
    /// <returns>The world position.</returns>
    public readonly Position ToWorldPos()
    {
        // OWOP rounds up Y values (since up = negative Y)
        int y = Y / IChunk.Width;
        if (Y % IChunk.Width != 0)
        {
            y--;
        }
        return new(X / IChunk.Width, y);
    }

    /// <summary>Converts a pixel-based world position to its chunk's position.</summary>
    /// <returns>The chunk position.</returns>
    public readonly Position ToChunkPos() => ((int)Math.Floor((decimal)X / IChunk.Width), (int)Math.Floor((decimal)Y / IChunk.Width));

    /// <summary>Whether a <b>chunk</b> position is within the world border.</summary>
    public readonly bool IsWithinBorder => X <= IWorld.Radius && Y <= IWorld.Radius && X >= ~IWorld.Radius && Y >= ~IWorld.Radius;

    /// <summary>Clamps a <b>chunk</b> position to be within the world border.</summary>
    /// <returns>The clamped position.</returns>
    public readonly Position ClampToBorder() => new(Math.Clamp(X, ~IWorld.Radius, IWorld.Radius), Math.Clamp(Y, ~IWorld.Radius, IWorld.Radius));

    /// <summary>Gets a simple string of the position.</summary>
    /// <returns>The string.</returns>
    public override readonly string ToString() => $"({X}, {Y})";

    // TODO: make this work regardless of position order
    /// <summary>Iterates rectangularly from this top-left position to another bottom-right position.</summary>
    /// <param name="other">The other position.</param>
    /// <returns>An enumeration of positions within the rectangle bounded by the two positions.</returns>
    public readonly IEnumerable<Position> IterateRect(Position other)
    {
        for (int x = X; x <= other.X; x++)
        {
            for (int y = Y; y <= other.Y; y++)
            {
                yield return (x, y);
            }
        }
    }

    /// <summary>Iterates linearly from this position to another position.</summary>
    /// <param name="other">The other position.</param>
    /// <returns>An enumeration of positions on the line between the two positions.</returns>
    public readonly IEnumerable<Position> IterateLine(Position other)
    {
        int x1 = X, x2 = other.X, y1 = Y, y2 = other.Y;
        int dx = Math.Abs(x2 - x1), sx = x1 < x2 ? 1 : -1;
        int dy = -Math.Abs(y2 - y1), sy = y1 < y2 ? 1 : -1;
        int err = dx + dy;
        while (true)
        {
            yield return (x1, y1);
            if (x1 == x2 && y1 == y2) break;
            int e2 = err * 2;
            if (e2 >= dy)
            {
                err += dy;
                x1 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y1 += sy;
            }
        }
    }

    /// <summary>Logical comparison.</summary>
    /// <param name="one">First position.</param>
    /// <param name="two">Second position.</param>
    /// <returns>Result.</returns>
    public static bool operator !=(Position one, Position two) => !one.Equals(two);

    /// <summary>Logical comparison.</summary>
    /// <param name="one">First position.</param>
    /// <param name="two">Second position.</param>
    /// <returns>Result.</returns>
    public static bool operator ==(Position one, Position two) => one.Equals(two);

    /// <summary>Mathematical comparison.</summary>
    /// <param name="one">First position.</param>
    /// <param name="two">Second position.</param>
    /// <returns>Result.</returns>
    public static Position operator +(Position one, Position two) => new(one.X + two.X, one.Y + two.Y);

    /// <summary>Mathematical comparison.</summary>
    /// <param name="one">First position.</param>
    /// <param name="two">Second position.</param>
    /// <returns>Result.</returns>
    public static Position operator -(Position one, Position two) => new(one.X - two.X, one.Y - two.Y);

    /// <summary>Mathematical comparison.</summary>
    /// <param name="one">First position.</param>
    /// <param name="two">Int scalar.</param>
    /// <returns>Result.</returns>
    public static Position operator *(Position one, int two) => new(one.X * two, one.Y * two);

    /// <summary>Mathematical comparison.</summary>
    /// <param name="one">First position.</param>
    /// <param name="two">Int scalar.</param>
    /// <returns>Result.</returns>
    public static Position operator /(Position one, int two) => new(one.X / two, one.Y / two);

    /// <summary>Mathematical comparison.</summary>
    /// <param name="one">First position.</param>
    /// <param name="two">Int scalar.</param>
    /// <returns>Result.</returns>
    public static Position operator %(Position one, int two) => new(one.X % two, one.Y % two);

    /// <summary>See <see cref="ToVector"/>.</summary>
    public static implicit operator Vector2(Position pos) => pos.ToVector();

    /// <summary>Converts a floating-point vector to a position.</summary>
    /// <param name="vec">The vector.</param>
    public static implicit operator Position(Vector2 vec) => new((int)vec.X, (int)vec.Y);

    /// <summary>See <see cref="ToTuple"/>.</summary>
    public static implicit operator (int, int)(Position pos) => (pos.X, pos.Y);

    /// <summary>Constructs a position from a 2D tuple of integers.</summary>
    /// <param name="pos">The position tuple.</param>
    public static implicit operator Position((int, int) pos) => new(pos.Item1, pos.Item2);
}
