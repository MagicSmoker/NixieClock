using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NixieClock;

/// <summary>Procedural wood-grain textures: value-noise fBm warped into horizontal grain lines.</summary>
static class Wood
{
    static double Hash(int x, int y, int seed)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 144269504);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (double)0xFFFFFF;
        }
    }

    static double Noise(double x, double y, int seed)
    {
        int xi = (int)Math.Floor(x), yi = (int)Math.Floor(y);
        double xf = x - xi, yf = y - yi;
        double u = xf * xf * (3 - 2 * xf), v = yf * yf * (3 - 2 * yf);
        double a = Hash(xi, yi, seed), b = Hash(xi + 1, yi, seed);
        double c = Hash(xi, yi + 1, seed), d = Hash(xi + 1, yi + 1, seed);
        return a + (b - a) * u + (c - a) * v + (a - b - c + d) * u * v;
    }

    static double Fbm(double x, double y, int seed, int octaves = 4)
    {
        double sum = 0, amp = 0.5, norm = 0;
        for (int i = 0; i < octaves; i++)
        {
            sum += Noise(x, y, seed + i * 31) * amp;
            norm += amp;
            amp *= 0.5;
            x *= 2.03;
            y *= 2.03;
        }
        return sum / norm;
    }

    /// <summary>
    /// A wood texture of width x height DIPs (rendered at 2x for crispness). Grain runs along X.
    /// ringFreq is grain lines per DIP; vertical=true turns the grain to run along Y.
    /// </summary>
    public static ImageBrush Brush(int width, int height, Color light, Color dark, int seed,
                                   double ringFreq = 0.09, bool vertical = false)
    {
        const double scale = 2;
        int w = (int)(width * scale), h = (int)(height * scale);
        var px = new byte[w * h * 4];
        Parallel.For(0, h, y =>
        {
            for (int x = 0; x < w; x++)
            {
                double lx = x / scale, ly = y / scale;
                if (vertical) (lx, ly) = (ly, lx);
                double warp = Fbm(lx * 0.005, ly * 0.018, seed) * 18;          // slow wander of grain lines
                double ring = 0.5 + 0.5 * Math.Sin((ly + warp) * ringFreq * Math.PI * 2);
                ring = Math.Pow(ring, 5);                                     // thin dark late-wood lines
                double fibre = Fbm(lx * 0.04, ly * 0.9, seed + 7, 3);         // fine streaks
                double blotch = Fbm(lx * 0.008, ly * 0.012, seed + 99, 3);    // broad tonal variation
                double t = 0.3 + 0.32 * ring + 0.32 * (fibre - 0.5) + 0.55 * (blotch - 0.5);
                t = Math.Clamp(t, 0, 1);
                int i = (y * w + x) * 4;
                px[i + 0] = (byte)(light.B + (dark.B - light.B) * t);
                px[i + 1] = (byte)(light.G + (dark.G - light.G) * t);
                px[i + 2] = (byte)(light.R + (dark.R - light.R) * t);
                px[i + 3] = 255;
            }
        });
        var bmp = BitmapSource.Create(w, h, 96 * scale, 96 * scale, PixelFormats.Bgra32, null, px, w * 4);
        bmp.Freeze();
        var brush = new ImageBrush(bmp) { Stretch = Stretch.Fill };
        brush.Freeze();
        return brush;
    }
}
