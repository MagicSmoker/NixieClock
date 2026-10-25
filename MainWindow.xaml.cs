using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NixieClock;

public partial class MainWindow : Window
{
    static readonly double[] TubeX = { 140, 290, 650, 800 };
    const double TubeY = 167, DialX = 470;
    static readonly TimeSpan CycleLength = TimeSpan.FromSeconds(6);

    readonly NixieTube[] _tubes = new NixieTube[4];
    readonly SecondsDial _dial;
    readonly Settings _settings = Settings.Load();
    readonly Random _rng = new();
    readonly string? _snapshot;
    readonly TimeSpan? _fixedTime;
    DateTime? _cycleStart;
    DateTime _lastCycleHour = DateTime.MinValue;
    int _frame;
    const int FrameMs = 33;
    readonly System.Diagnostics.Stopwatch _frameClock = System.Diagnostics.Stopwatch.StartNew();

    public MainWindow(string? snapshot = null, TimeSpan? fixedTime = null)
    {
        InitializeComponent();
        _snapshot = snapshot;
        _fixedTime = fixedTime;

        CaseBuilder.Build(Root);
        for (int i = 0; i < 4; i++) Root.Children.Add(_tubes[i] = new NixieTube(TubeX[i], TubeY));
        Root.Children.Add(_dial = new SecondsDial(DialX, TubeY));

        ApplyScale();
        Topmost = _settings.Topmost && _snapshot == null;
        if (_settings.Left is double left && _settings.Top is double top && IsOnScreen(left, top))
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = left;
            Top = top;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        BuildMenu();
        MouseLeftButtonDown += (_, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };
        LocationChanged += (_, _) => { _settings.Left = Left; _settings.Top = Top; };
        Closing += (_, _) => { if (_snapshot == null) _settings.Save(); };

        Tick();
        if (_snapshot != null)
            Loaded += (_, _) => Dispatcher.BeginInvoke(SaveSnapshot, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        else
            CompositionTarget.Rendering += (_, _) =>
            {
                // A transparent (layered) window repaints in full every frame, so cap the rate.
                if (_frameClock.ElapsedMilliseconds < FrameMs) return;
                _frameClock.Restart();
                Tick();
            };
    }

    DateTime Now => _fixedTime is TimeSpan t ? DateTime.Today + t : DateTime.Now;

    void Tick()
    {
        var now = Now;

        // Hourly cathode-poisoning prevention: on the hour, spin every digit through every cathode.
        var hour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        if (now.Minute == 0 && hour != _lastCycleHour && _snapshot == null)
        {
            _lastCycleHour = hour;
            _cycleStart = now;
        }

        int[] digits;
        if (_cycleStart is DateTime start && now - start < CycleLength)
        {
            double e = (now - start).TotalSeconds;
            digits = new int[4];
            for (int i = 0; i < 4; i++) digits[i] = ((int)(e * 12) + i * 3) % 10;
        }
        else
        {
            _cycleStart = null;
            int h = now.Hour;
            if (!_settings.Use24Hour) h = h % 12 == 0 ? 12 : h % 12;
            digits = new[] { h / 10, h % 10, now.Minute / 10, now.Minute % 10 };
        }

        for (int i = 0; i < 4; i++) _tubes[i].SetDigit(digits[i]);
        _dial.SetSeconds(now.Second + now.Millisecond / 1000.0);

        if (_snapshot == null && ++_frame % 3 == 0)
            foreach (var t in _tubes) t.Flicker(0.96 + _rng.NextDouble() * 0.04);
    }

    void StartCycle() => _cycleStart = DateTime.Now;

    void ApplyScale()
    {
        Width = CaseBuilder.Width * _settings.Scale;
        Height = CaseBuilder.Height * _settings.Scale;
    }

    static bool IsOnScreen(double left, double top) =>
        left > SystemParameters.VirtualScreenLeft - 100 && top > SystemParameters.VirtualScreenTop - 50 &&
        left < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 100 &&
        top < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 50;

    void BuildMenu()
    {
        var menu = new ContextMenu();

        var onTop = new MenuItem { Header = "Always on top", IsCheckable = true, IsChecked = _settings.Topmost };
        onTop.Click += (_, _) => { _settings.Topmost = Topmost = onTop.IsChecked; _settings.Save(); };
        menu.Items.Add(onTop);

        var h24 = new MenuItem { Header = "24-hour time", IsCheckable = true, IsChecked = _settings.Use24Hour };
        h24.Click += (_, _) => { _settings.Use24Hour = h24.IsChecked; _settings.Save(); Tick(); };
        menu.Items.Add(h24);

        var size = new MenuItem { Header = "Size" };
        foreach (double s in new[] { 0.5, 0.65, 0.8, 1.0, 1.25, 1.5 })
        {
            var item = new MenuItem { Header = $"{s * 100:0}%", IsCheckable = true, IsChecked = Math.Abs(s - _settings.Scale) < 0.01 };
            item.Click += (_, _) =>
            {
                _settings.Scale = s;
                foreach (MenuItem other in size.Items) other.IsChecked = other == item;
                ApplyScale();
                _settings.Save();
            };
            size.Items.Add(item);
        }
        menu.Items.Add(size);

        var cycle = new MenuItem { Header = "Run cathode cycle now" };
        cycle.Click += (_, _) => StartCycle();
        menu.Items.Add(cycle);

        menu.Items.Add(new Separator());
        var exit = new MenuItem { Header = "Exit" };
        exit.Click += (_, _) => Close();
        menu.Items.Add(exit);

        ContextMenu = menu;
    }

    void SaveSnapshot()
    {
        var bmp = new RenderTargetBitmap((int)(CaseBuilder.Width * 2), (int)(CaseBuilder.Height * 2), 192, 192, PixelFormats.Pbgra32);
        var backdrop = new DrawingVisual();     // the photo's white tabletop, so the shadow shows
        using (var dc = backdrop.RenderOpen())
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, CaseBuilder.Width, CaseBuilder.Height));
        bmp.Render(backdrop);
        bmp.Render(Root);
        var enc = new PngBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(bmp));
        using (var fs = File.Create(_snapshot!)) enc.Save(fs);
        Application.Current.Shutdown();
    }
}
