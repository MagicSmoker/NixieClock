using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static NixieClock.Draw;

namespace NixieClock;

/// <summary>One IN-14 tube in its wooden bezel: ghost cathodes, glowing digit, hex anode mesh, glass.</summary>
sealed class NixieTube : Canvas
{
    const double R = 64;            // bezel radius; local centre is (R, R)
    const double Glass = 55.5;
    const double DigitScale = 1.32;

    readonly Geometry[] _digits = new Geometry[10];
    readonly Path[] _ghosts = new Path[10];
    readonly Canvas _lit = new();
    readonly Path[] _litPaths = new Path[4];
    readonly Path _ambient;
    int _digit = -2;

    public NixieTube(double cx, double cy)
    {
        Width = Height = 2 * R;
        SetLeft(this, cx - R);
        SetTop(this, cy - R);
        for (int d = 0; d < 10; d++) _digits[d] = Digits.Get(d, R, R + 1, DigitScale);

        // Bezel: dark wood ring with bevel, then the black socket ring.
        Shadow(Disc(this, R, R, R, Radial(new Point(0.35, 0.3), new Point(0.5, 0.5), 0.6,
            Stop("#7a4724", 0), Stop("#4a2711", 0.6), Stop("#241105", 1)), Solid("#120802"), 1), 9, 3, 0.65);
        Disc(this, R, R, R - 1.8, null, Linear(new Point(0.2, 0), new Point(0.8, 1),
            Stop("#d09060", 0, 0.8), Stop("#5a3012", 0.5, 0.3), Stop("#000000", 1, 0.6)), 1.6);
        Disc(this, R, R, 59, Solid("#080504"), Linear(new Point(0.2, 0), new Point(0.8, 1),
            Stop("#000000", 0, 0.9), Stop("#c08860", 1, 0.6)), 1.4);

        // Glass interior and the orange light it fills with.
        Disc(this, R, R, Glass, Radial(new Point(0.5, 0.42), new Point(0.5, 0.45), 0.55,
            Stop("#2c2420", 0), Stop("#141010", 0.65), Stop("#050303", 1)));
        _ambient = Disc(this, R, R, Glass - 2, Radial(
            Stop("#ff6a1a", 0, 0.42), Stop("#ff5208", 0.45, 0.16), Stop("#ff4000", 1, 0)));

        // Ghost cathodes, stacked back to front like the real tube.
        for (int rank = 0; rank < 10; rank++)
        {
            int d = Digits.BackToFront[rank];
            var g = Path(this, _digits[d], null, Solid("#9a8678"), 1.15);
            g.Opacity = 0.22 + 0.025 * rank;
            SetZIndex(g, 10 + rank * 2);
            _ghosts[d] = g;
        }

        // The lit cathode: wide bloom, glow, wire, white-hot core.
        (string color, double thick, double blur, double opacity)[] layers =
        {
            ("#ff3c00", 13, 28, 0.9),
            ("#ff6410", 4.5, 6, 0.95),
            ("#ff9640", 2.3, 0, 1),
            ("#ffe4c0", 0.9, 0, 0.85),
        };
        for (int i = 0; i < layers.Length; i++)
        {
            var (color, thick, blur, opacity) = layers[i];
            var p = Path(_lit, (Geometry?)null!, null, Solid(color), thick);
            p.Opacity = opacity;
            if (blur > 0) Blur(p, blur);
            _litPaths[i] = p;
        }
        Children.Add(_lit);

        // Hexagonal anode mesh in front of the cathodes.
        var mesh = Path(this, HexMesh(R, R, 4.6, Glass - 0.5), null, Solid("#c4beb6"), 0.5);
        mesh.Opacity = 0.42;
        mesh.Clip = Circle(R, R, Glass - 0.5);
        SetZIndex(mesh, 100);

        // Glass reflections.
        Add(Path(this, Crescent(R, R, Glass - 1, 6, 7.5, 0.5), Linear(new Point(0, 0), new Point(0.7, 0.7),
            Stop("#ffffff", 0, 0.55), Stop("#ffffff", 0.35, 0.12), Stop("#ffffff", 0.6, 0))), 101);
        Add(Path(this, Crescent(R, R, Glass - 1, -4, -5, 0.5), Linear(new Point(1, 1), new Point(0.4, 0.4),
            Stop("#ffffff", 0, 0.28), Stop("#ffffff", 0.5, 0))), 101);
        var spec = Path(this, new EllipseGeometry(new Point(R - 27, R - 33), 9, 4.2), Solid("#ffffff", 0.7));
        spec.RenderTransform = new RotateTransform(-40, R - 27, R - 33);
        Add(Blur(spec, 3), 102);
        var streak = Path(this, new RectangleGeometry(new Rect(R + 27, R - 34, 5, 34), 2.5, 2.5), Solid("#ffffff", 0.1));
        streak.RenderTransform = new RotateTransform(28, R + 29, R - 17);
        Add(Blur(streak, 1.5), 102);

        // White mica-spacer dots top and bottom.
        foreach (double dy in new[] { -Glass + 2.5, Glass - 2.5 })
            Add(Disc(this, R, R + dy, 4.3, Radial(new Point(0.35, 0.3), new Point(0.5, 0.5), 0.6,
                Stop("#ffffff", 0), Stop("#e6e2dc", 0.6), Stop("#9a948c", 1)), Solid("#5a5450"), 0.5), 103);

        SetDigit(-1);
    }

    static void Add(UIElement e, int z) => SetZIndex(e, z);

    /// <summary>Show digit 0-9, or -1 for blank.</summary>
    public void SetDigit(int d)
    {
        if (d == _digit) return;
        _digit = d;
        foreach (var g in _ghosts) g.Visibility = Visibility.Visible;
        if (d < 0)
        {
            _lit.Visibility = Visibility.Hidden;
            _ambient.Opacity = 0;
            return;
        }
        _lit.Visibility = Visibility.Visible;
        _ambient.Opacity = 1;
        foreach (var p in _litPaths) p.Data = _digits[d];
        _ghosts[d].Visibility = Visibility.Hidden;
        // Sit the lit cathode at its depth so the cathodes in front of it shadow it.
        SetZIndex(_lit, 10 + Array.IndexOf(Digits.BackToFront, d) * 2 + 1);
    }

    /// <summary>Gas-discharge shimmer: tiny brightness changes.</summary>
    public void Flicker(double level)
    {
        _lit.Opacity = level;
        _ambient.Opacity = _digit < 0 ? 0 : level;
    }

    static Geometry HexMesh(double cx, double cy, double s, double radius)
    {
        // Flat-topped hexagons of circumradius s.
        var g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            double w = 1.5 * s, h = Math.Sqrt(3) * s;
            int n = (int)(radius / w) + 2;
            for (int col = -n; col <= n; col++)
            {
                for (int row = -n; row <= n; row++)
                {
                    double x = cx + col * w, y = cy + row * h + ((col & 1) != 0 ? h / 2 : 0);
                    if (Math.Abs(x - cx) > radius + s || Math.Abs(y - cy) > radius + s) continue;
                    ctx.BeginFigure(new Point(x + s, y), false, true);
                    for (int k = 1; k < 6; k++)
                    {
                        double a = k * Math.PI / 3;
                        ctx.LineTo(new Point(x + s * Math.Cos(a), y + s * Math.Sin(a)), true, false);
                    }
                }
            }
        }
        g.Freeze();
        return g;
    }
}
