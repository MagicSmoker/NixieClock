using System.Windows;

namespace NixieClock;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // NixieClock.exe --snapshot out.png [HH:mm:ss]  renders one frame to a PNG and exits.
        string? snapshot = null;
        TimeSpan? fixedTime = null;
        for (int i = 0; i < e.Args.Length; i++)
        {
            if (e.Args[i] == "--snapshot" && i + 1 < e.Args.Length)
            {
                snapshot = e.Args[++i];
                if (i + 1 < e.Args.Length && TimeSpan.TryParse(e.Args[i + 1], out var t))
                {
                    fixedTime = t;
                    i++;
                }
            }
        }

        new MainWindow(snapshot, fixedTime).Show();
    }
}
