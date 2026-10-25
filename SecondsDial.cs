using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static NixieClock.Draw;

namespace NixieClock;

/// <summary>Centre dial: oak ring printed 0-55, ring of dots, dark domed glass eye, sweeping seconds marker.</summary>
sealed class SecondsDial : Canvas
{
    const double R = 92;            // local centre is (R, R)
    const double RingR = 88, NumberR = 78.5, DotR = 65.5, DomeR = 55;
    const int Trail = 14;

    readonly Ellipse _marker, _halo;
    readonly Ellipse[] _trail = new Ellipse[Trail];

    public SecondsDial(double cx, double cy)
    {
        Width = Height = 2 * R;
        SetLeft(this, cx - R);
        SetTop(this, cy - R);

        // Oak ring with darker edge.
        Shadow(Disc(this, R, R, RingR + 2.5, Solid("#3a1c0a")), 12, 4, 0.6);
        var oak = Wood.Brush(176, 176, C("#e2ae76"), C("#a86e3c"), seed: 41, ringFreq: 0.06);
        Disc(this, R, R, RingR, oak, Solid("#5a2e10"), 1.2);
        Disc(this, R, R, RingR, Radial(new Point(0.35, 0.3), new Point(0.5, 0.5), 0.5,
            Stop("#ffffff", 0, 0.12), Stop("#000000", 0.78, 0), Stop("#000000", 1, 0.32)));
        Disc(this, R, R, RingR - 2, null, Linear(new Point(0.2, 0), new Point(0.8, 1),
            Stop("#fff0d8", 0, 0.5), Stop("#000000", 1, 0.35)), 1);
        Disc(this, R, R, 71.5, null, Solid("#6a3c18", 0.45), 0.8);

        // Printed seconds.
        for (int s = 0; s < 60; s += 5)
        {
            double a = s * Math.PI / 30;
            var tb = new TextBlock
            {
                Text = s.ToString(), Width = 30, TextAlignment = TextAlignment.Center,
                FontFamily = new FontFamily("Bahnschrift SemiCondensed, Arial Narrow, Segoe UI"),
                FontSize = 12.5, FontWeight = FontWeights.SemiBold, Foreground = Solid("#3a1a08", 0.88),
            };
            SetLeft(tb, R + NumberR * Math.Sin(a) - 15);
            SetTop(tb, R - NumberR * Math.Cos(a) - 8.5);
            Children.Add(tb);
        }

        // Burned dots.
        for (int s = 0; s < 60; s++)
        {
            double a = s * Math.PI / 30;
            Disc(this, R + DotR * Math.Sin(a), R - DotR * Math.Cos(a), s % 5 == 0 ? 2.3 : 1.6, Solid("#341608", 0.9));
        }

        // Seconds marker: a fading comet trail and a glowing bead on the dot ring.
        for (int i = Trail - 1; i >= 0; i--)
        {
            _trail[i] = Bead(2.4, Solid("#ff6a18"));
            _trail[i].Opacity = 0.55 * (1 - i / (double)Trail);
        }
        _halo = Blur(Bead(9, Solid("#ff4800", 0.85)), 8);
        _marker = Bead(3.2, Radial(Stop("#fff4dc", 0), Stop("#ffb060", 0.5), Stop("#ff6a10", 1)));

        // The domed glass eye.
        Disc(this, R, R, DomeR + 4, Solid("#120904"), Linear(new Point(0.2, 0), new Point(0.8, 1),
            Stop("#000000", 0, 0.9), Stop("#d0a070", 1, 0.55)), 1.4);
        Disc(this, R, R, DomeR, Radial(new Point(0.36, 0.28), new Point(0.45, 0.42), 0.62,
            Stop("#77777c", 0), Stop("#323236", 0.22), Stop("#0e0e10", 0.55), Stop("#050506", 0.85), Stop("#26262a", 1)));
        // Faint inner structure seen through the glass.
        Path(this, $"M {R - 9},{R + 30} L {R - 6},{R - 18} L {R + 6},{R - 18} L {R + 9},{R + 30}", null, Solid("#3a3a40", 0.35), 1.5);
        Blur(Disc(this, R, R - 18, 7, Solid("#2a4a3a", 0.25)), 4);
        // Orange glints of the neighbouring tubes.
        Blur(Path(this, new EllipseGeometry(new Point(R - 44, R + 12), 4, 9), Solid("#ff7a20", 0.55)), 4);
        Blur(Path(this, new EllipseGeometry(new Point(R + 44, R + 12), 4, 9), Solid("#ff7a20", 0.55)), 4);
        Blur(Path(this, new EllipseGeometry(new Point(R + 18, R + 36), 9, 4), Solid("#ff8a30", 0.35)), 3);
        // Window reflection and rim light.
        var win = Path(this, new RectangleGeometry(new Rect(R - 40, R - 40, 34, 20), 7, 7), Linear(new Point(0, 0), new Point(1, 1),
            Stop("#ffffff", 0, 0.75), Stop("#ffffff", 1, 0.2)));
        win.RenderTransform = new RotateTransform(-32, R - 23, R - 30);
        Blur(win, 2.5);
        Path(this, Crescent(R, R, DomeR - 0.5, 0, -5, 0.5), Linear(new Point(0, 0), new Point(0, 1),
            Stop("#ffffff", 0.6, 0), Stop("#ffffff", 1, 0.28)));
        Path(this, Crescent(R, R, DomeR - 0.5, 4, 5, 0.5), Solid("#ffffff", 0.12));
    }

    Ellipse Bead(double r, Brush fill)
    {
        var e = new Ellipse { Width = 2 * r, Height = 2 * r, Fill = fill, IsHitTestVisible = false };
        Children.Add(e);
        return e;
    }

    static void Place(Ellipse e, double seconds)
    {
        double a = seconds * Math.PI / 30;
        SetLeft(e, R + DotR * Math.Sin(a) - e.Width / 2);
        SetTop(e, R - DotR * Math.Cos(a) - e.Height / 2);
    }

    /// <summary>Seconds within the minute, fractional for a smooth sweep.</summary>
    public void SetSeconds(double seconds)
    {
        Place(_marker, seconds);
        Place(_halo, seconds);
        for (int i = 0; i < Trail; i++) Place(_trail[i], seconds - i * 0.22);
    }
}
