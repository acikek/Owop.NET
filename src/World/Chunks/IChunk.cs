using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using Owop.Util;

namespace Owop.Game;

public interface IChunk : IPositioned
{
    public const int Size = 16;

    ReadOnlyMemory2D<Color> Pixels { get; }
    bool IsProtected { get; }

    ReadOnlySpan<Color> this[int row] { get; }
    Color this[int x, int y] { get; }

    public static Position GetWorldPos(Position pos)
    {
        // OWOP rounds up Y values (since up = negative Y)
        int y = pos.Y / Size;
        if (pos.Y % Size != 0)
        {
            y--;
        }
        return new(pos.X / Size, y);
    }

    public static Position GetChunkPos(Position worldPos) 
        => ((int) Math.Floor((decimal) worldPos.X / Size), (int) Math.Floor((decimal) worldPos.Y / Size));
}
