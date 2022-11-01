using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace lols;

public class MainView : UserControl
{
    private int _count;
    readonly System.Timers.Timer timer = new (500);
    readonly Stopwatch stopwatch = new ();
    private readonly LolsView? absolute;
    private readonly TextBlock? lols;

    public MainView()
    {
        InitializeComponent();

        absolute = this.FindControl<LolsView>("absolute");
        lols = this.FindControl<TextBlock>("lols");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();
        
        Run();
    }

    public void Run()
    {
        timer.Elapsed += OnTimer;

        stopwatch.Start();
        timer.Start();
        
        if (OperatingSystem.IsBrowser())
        {
            _ = Task.Run(RunTest);
        }
        else
        {
            _ = Task.Factory.StartNew(RunTest, TaskCreationOptions.LongRunning);
        }
    }

    void OnTimer(object? sender, System.Timers.ElapsedEventArgs e)
    {
        double avg = _count / stopwatch.Elapsed.TotalSeconds;
        string text = "LOL/s: " + avg.ToString("0.00", CultureInfo.InvariantCulture);
        Dispatcher.UIThread.Post(() => UpdateText(text));
    }

    void UpdateText(string text)
    {
        if (lols is { })
        {
            lols.Text = text;
        }
    }

    async void RunTest()
    {
        if (absolute is null)
        {
            return;
        }

        var width = absolute.Bounds.Width;
        var height = absolute.Bounds.Height;

        while (_count < 100000)
        {
            absolute.AddLol(width, height);
            Dispatcher.UIThread.Post(() => absolute.InvalidateVisual());
            _count++;

            if (_count % 256 == 0)
            {
                if (OperatingSystem.IsBrowser())
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
