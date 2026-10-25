using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static NixieClock.Draw;

namespace NixieClock;

/// <summary>The wooden case: floor shadow, legs, walnut body with kerf-bent corners, oak front panel.</summary>
static class CaseBuilder
{
    public const double Width = 940, Height = 440;

    // Outline of the walnut body; the top flares out over the sides like a hood.
    const string BodyData = "M 46,12 L 894,12 Q 930,12 932,50 L 920,316 L 20,316 L 8,50 Q 10,12 46,12 Z";
    const string PanelData = "M 54,32 L 886,32 Q 906,32 906,54 L 900,288 L 40,288 L 34,54 Q 34,32 54,32 Z";

    public static void Build(Canvas c)
    {
        // Soft shadow on the "table".
        var shadow = new System.Windows.Shapes.Ellipse
        {
            Width = 900, Height = 46,
            Fill = Radial(Stop("#000000", 0, 0.55), Stop("#000000", 0.6, 0.25), Stop("#000000", 1, 0)),
        };
        Canvas.SetLeft(shadow, 20);
        Canvas.SetTop(shadow, 372);
        c.Children.Add(Blur(shadow, 14));

        BuildLegs(c);

        // Walnut body.
        var walnut = Wood.Brush(924, 304, C("#6e3d1f"), C("#2a1408"), seed: 11, ringFreq: 0.11);
        var body = Path(c, BodyData, walnut, Solid("#160a03"), 1.2);
        Shadow(body, 16, 6, 0.45);
        Path(c, BodyData, Linear(new Point(0, 0), new Point(0, 1),
            Stop("#ffffff", 0, 0.14), Stop("#ffffff", 0.06, 0), Stop("#000000", 0.7, 0), Stop("#000000", 1, 0.35)));
        Path(c, BodyData, Linear(new Point(0, 0), new Point(1, 0),
            Stop("#000000", 0, 0.45), Stop("#000000", 0.035, 0), Stop("#000000", 0.965, 0), Stop("#000000", 1, 0.5)));
        Path(c, "M 46,13.2 L 894,13.2", null, Solid("#c08050", 0.45), 1);

        BuildFins(c, new Point(8, 50), new Point(10, 12), new Point(46, 12));
        BuildFins(c, new Point(932, 50), new Point(930, 12), new Point(894, 12));

        // Bottom trim groove.
        Path(c, "M 26,301 L 914,301", null, Solid("#0e0602", 0.8), 1.2);
        Path(c, "M 26,302.5 L 914,302.5", null, Solid("#a06a40", 0.35), 0.8);

        // Oak front panel, recessed under the hood.
        var oak = Wood.Brush(872, 256, C("#d29c66"), C("#8a5428"), seed: 5, ringFreq: 0.075);
        var panel = Path(c, PanelData, oak, Solid("#2a1206"), 1);
        Path(c, PanelData, Linear(new Point(0, 0), new Point(0, 1),
            Stop("#000000", 0, 0.55), Stop("#000000", 0.07, 0.08), Stop("#000000", 0.5, 0),
            Stop("#000000", 0.93, 0.05), Stop("#000000", 1, 0.3)));
        Path(c, PanelData, Linear(new Point(0, 0), new Point(1, 0),
            Stop("#000000", 0, 0.4), Stop("#000000", 0.04, 0), Stop("#000000", 0.96, 0), Stop("#000000", 1, 0.4)));
        Path(c, "M 41,286.6 L 899,286.6", null, Solid("#f0c090", 0.35), 1);

        // Routed groove framing the tubes: dark cut with a light lower lip.
        var groove = new RectangleGeometry(new Rect(64, 70, 812, 194), 16, 16);
        Path(c, groove, Solid("#000000", 0.05), Solid("#3c1e0a", 0.85), 1.8);
        var lip = groove.Clone();
        lip.Transform = new TranslateTransform(0.4, 1.6);
        Path(c, lip, null, Solid("#f4c898", 0.3), 0.9);
    }

    static void BuildLegs(Canvas c)
    {
        var oak = Wood.Brush(120, 110, C("#d6a068"), C("#9a6232"), seed: 23, ringFreq: 0.08, vertical: true);
        var side = Solid("#7a4a22");
        var back = Solid("#1a0a00", 0.5);

        foreach (bool mirror in new[] { false, true })
        {
            string M(string d) => mirror ? Mirror(d) : d;

            // Rear leg (further away, ends higher, in shade).
            Path(c, M("M 236,296 L 262,296 L 302,376 L 278,376 Z"), oak);
            Path(c, M("M 236,296 L 262,296 L 302,376 L 278,376 Z"), back);
            // Front leg: face plus its visible edge.
            Path(c, M("M 242,296 L 252,296 L 178,404 L 168,404 Z"), side);
            Path(c, M("M 206,296 L 242,296 L 168,404 L 130,404 Z"), oak, Solid("#5a3214", 0.7), 0.8);
            Path(c, M("M 206,296 L 242,296 L 168,404 L 130,404 Z"), Linear(new Point(0, 0), new Point(0, 1),
                Stop("#000000", 0, 0.45), Stop("#000000", 0.3, 0.05), Stop("#ffffff", 1, 0.08)));
            Path(c, M("M 131,403.5 L 167,403.5"), null, Solid("#f8d0a0", 0.5), 1);
        }
    }

    /// <summary>Mirror an absolute "M x,y L x,y ..." path across the clock's centre line.</summary>
    static string Mirror(string d) =>
        System.Text.RegularExpressions.Regex.Replace(d, @"(\d+(?:\.\d+)?),", m =>
            (Width - double.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture))
                .ToString(System.Globalization.CultureInfo.InvariantCulture) + ",");

    /// <summary>Kerf-cut slats where the plywood bends round a top corner (quadratic p0-p1-p2).</summary>
    static void BuildFins(Canvas c, Point p0, Point p1, Point p2)
    {
        var dark = Solid("#0c0502", 0.9);
        var light = Solid("#b07444", 0.55);
        var center = new Point(Width / 2, 170);
        const int n = 20;
        for (int i = 0; i <= n; i++)
        {
            double t = i / (double)n, u = 1 - t;
            var p = new Point(u * u * p0.X + 2 * u * t * p1.X + t * t * p2.X, u * u * p0.Y + 2 * u * t * p1.Y + t * t * p2.Y);
            var tan = new Vector(2 * u * (p1.X - p0.X) + 2 * t * (p2.X - p1.X), 2 * u * (p1.Y - p0.Y) + 2 * t * (p2.Y - p1.Y));
            tan.Normalize();
            var nrm = new Vector(-tan.Y, tan.X);
            if (Vector.Multiply(nrm, center - p) < 0) nrm = -nrm;   // point inward
            double len = 14 + 6 * Math.Sin(t * Math.PI);
            var a = p + nrm * 0.6;
            var b = p + nrm * len;
            Path(c, new LineGeometry(a, b), null, dark, 1.7);
            Path(c, new LineGeometry(a + tan * 1.6, b + tan * 1.6), null, light, 0.9);
        }
    }
}
