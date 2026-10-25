using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace NixieClock;

/// <summary>Small helpers for building the scene out of WPF shapes.</summary>
static class Draw
{
    public static Color C(string hex) => (Color)ColorConverter.ConvertFromString(hex);

    public static SolidColorBrush Solid(string hex, double alpha = 1)
    {
        var c = C(hex);
        c.A = (byte)(alpha * 255);
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }

    public static GradientStop Stop(string hex, double offset, double alpha = 1)
    {
        var c = C(hex);
        c.A = (byte)(alpha * 255);
        return new GradientStop(c, offset);
    }

    public static LinearGradientBrush Linear(Point start, Point end, params GradientStop[] stops)
    {
        var b = new LinearGradientBrush(new GradientStopCollection(stops), start, end);
        b.Freeze();
        return b;
    }

    public static RadialGradientBrush Radial(Point origin, Point center, double radius, params GradientStop[] stops)
    {
        var b = new RadialGradientBrush(new GradientStopCollection(stops))
        {
            GradientOrigin = origin, Center = center, RadiusX = radius, RadiusY = radius,
        };
        b.Freeze();
        return b;
    }

    public static RadialGradientBrush Radial(params GradientStop[] stops) =>
        Radial(new Point(0.5, 0.5), new Point(0.5, 0.5), 0.5, stops);

    public static Path Path(Panel parent, Geometry data, Brush? fill, Brush? stroke = null, double thickness = 0)
    {
        var p = new Path
        {
            Data = data, Fill = fill, Stroke = stroke, StrokeThickness = thickness,
            StrokeLineJoin = PenLineJoin.Round, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round,
        };
        parent.Children.Add(p);
        return p;
    }

    public static Path Path(Panel parent, string data, Brush? fill, Brush? stroke = null, double thickness = 0) =>
        Path(parent, Geometry.Parse(data), fill, stroke, thickness);

    public static EllipseGeometry Circle(double cx, double cy, double r) => new(new Point(cx, cy), r, r);

    public static Path Disc(Panel parent, double cx, double cy, double r, Brush? fill, Brush? stroke = null, double thickness = 0) =>
        Path(parent, Circle(cx, cy, r), fill, stroke, thickness);

    /// <summary>The sliver of circle (cx,cy,r) not covered by the same circle shifted by (dx,dy).</summary>
    public static Geometry Crescent(double cx, double cy, double r, double dx, double dy, double shrink = 0) =>
        new CombinedGeometry(GeometryCombineMode.Exclude, Circle(cx, cy, r), Circle(cx + dx, cy + dy, r - shrink));

    public static T Blur<T>(T e, double radius) where T : UIElement
    {
        e.Effect = new BlurEffect { Radius = radius, KernelType = KernelType.Gaussian, RenderingBias = RenderingBias.Performance };
        return e;
    }

    public static T Shadow<T>(T e, double blur, double depth, double opacity, double direction = 290) where T : UIElement
    {
        e.Effect = new DropShadowEffect
        {
            BlurRadius = blur, ShadowDepth = depth, Opacity = opacity, Direction = direction, Color = Colors.Black,
        };
        return e;
    }
}
