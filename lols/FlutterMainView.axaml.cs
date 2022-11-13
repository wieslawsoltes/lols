using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;

namespace lols;

public partial class FlutterMainView : UserControl
{
    int count = 0;
    readonly System.Timers.Timer timer = new System.Timers.Timer(500);
    readonly Stopwatch stopwatch = new Stopwatch();
     readonly bool _isBrowser;

    public FlutterMainView()
    {
        InitializeComponent();

        _isBrowser = OperatingSystem.IsBrowser();
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

        Start();
    }

    private void Start()
    {
        timer.Elapsed += OnTimer;

        stopwatch.Start();
        timer.Start();
        _ = _isBrowser
            ? Task.Run(RunTest)
            : Task.Factory.StartNew(RunTest, TaskCreationOptions.LongRunning);
    }

    void OnTimer(object? sender, System.Timers.ElapsedEventArgs e)
    {
        double avg = count / stopwatch.Elapsed.TotalSeconds;
        string text = "LOL/s: " + avg.ToString("0.00", CultureInfo.InvariantCulture);
        Dispatcher.UIThread.Post(() => UpdateText(text));
    }

    void UpdateText(string text) => lols.Text = text;

    async void RunTest()
    {
        var width = canvas.Bounds.Width;
        var height = canvas.Bounds.Height;
        var step = 1_048_576; //_isBrowser ? 256 : 1_048_576;

        while (count < 30_000_000)
        {
            canvas.AddLol(width, height);
            Dispatcher.UIThread.Post(() => canvas.InvalidateVisual());
            count++;

            if (count % step == 0)
            {
                if (_isBrowser)
                {
                    var tcs = new TaskCompletionSource();
                    Dispatcher.UIThread.Post(a => ((TaskCompletionSource)a!).SetResult(), tcs, DispatcherPriority.MinValue);
                    await Task.WhenAll(Task.Delay(1), tcs.Task);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        stopwatch.Stop();
        timer.Stop();
    }
}

