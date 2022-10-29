using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;

namespace lols;

public partial class MainWindow : Window
{
    const int Max = 500;
    int count = 0;
    readonly System.Timers.Timer timer = new System.Timers.Timer(500);
    readonly Stopwatch stopwatch = new Stopwatch();
    
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        timer.Elapsed += OnTimer;

        stopwatch.Start();
        timer.Start();
        _ = Task.Factory.StartNew(RunTest, TaskCreationOptions.LongRunning);
    }

    void OnTimer(object? sender, System.Timers.ElapsedEventArgs e)
    {
        double avg = count / stopwatch.Elapsed.TotalSeconds;
        string text = "LOL/s: " + avg.ToString("0.00", CultureInfo.InvariantCulture);
        Dispatcher.UIThread.Post(() => UpdateText(text));
    }

    void UpdateText(string text) => lols.Content = text;

    void RunTest()
    {
        var random = Random.Shared;
        var width = absolute.Bounds.Width;
        var height = absolute.Bounds.Height;

        while (count < 100000)
        {
            absolute.AddLol(width, height);
            Dispatcher.UIThread.Post(() => absolute.InvalidateVisual());
            count++;

            if (count % 256 == 0)
            {
                Thread.Sleep(1);
            }
        }

        stopwatch.Stop();
        timer.Stop();
    }
}
