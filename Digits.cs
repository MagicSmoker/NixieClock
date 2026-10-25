using System.Windows.Media;

namespace NixieClock;

/// <summary>IN-14 style wire cathode shapes, drawn in a 36 x 60 box.</summary>
static class Digits
{
    public const double BoxW = 36, BoxH = 60;

    static readonly string[] Data =
    {
        /* 0 */ "M 18,0 A 18,30 0 1 0 18,60 A 18,30 0 1 0 18,0 Z",
        /* 1 */ "M 9,11 L 20,0 L 20,60",
        /* 2 */ "M 1,15 C 1,5 9,0 18,0 C 28,0 35,6 35,15 C 35,28 22,37 1,60 L 36,60",
        /* 3 */ "M 2,0 L 34,0 L 15,24 C 28,23 36,31 36,42 C 36,53 28,60 18,60 C 9,60 2,55 0,48",
        /* 4 */ "M 28,60 L 28,0 L 0,42 L 36,42",
        /* 5 */ "M 34,0 L 6,0 L 3,27 C 7,23 12,21 18,21 C 29,21 36,29 36,40 C 36,52 28,60 18,60 C 9,60 3,55 0,48",
        /* 6 */ "M 32,4 C 28,1 24,0 19,0 C 7,0 0,14 0,38 C 0,52 8,60 18,60 C 29,60 36,52 36,41 C 36,30 29,22 18,22 C 8,22 1,29 0,38",
        /* 7 */ "M 0,0 L 36,0 L 12,60",
        /* 8 */ "M 18,27 C 9,27 3,21 3,13.5 C 3,6 10,0 18,0 C 26,0 33,6 33,13.5 C 33,21 27,27 18,27 " +
                "C 8,27 0,34 0,43.5 C 0,53 8,60 18,60 C 28,60 36,53 36,43.5 C 36,34 28,27 18,27 Z",
        /* 9 */ "M 4,56 C 8,59 12,60 17,60 C 29,60 36,46 36,22 C 36,8 28,0 18,0 C 7,0 0,8 0,19 C 0,30 7,38 18,38 C 28,38 35,31 36,22",
    };

    /// <summary>Real IN-14 cathode stack, back to front.</summary>
    public static readonly int[] BackToFront = { 3, 8, 9, 4, 0, 5, 7, 2, 6, 1 };

    /// <summary>Digit d centred on (cx, cy) at the given scale.</summary>
    public static Geometry Get(int d, double cx, double cy, double scale)
    {
        var g = Geometry.Parse(Data[d]).Clone();
        g.Transform = new MatrixTransform(scale, 0, 0, scale, cx - BoxW / 2 * scale, cy - BoxH / 2 * scale);
        g.Freeze();
        return g;
    }
}
