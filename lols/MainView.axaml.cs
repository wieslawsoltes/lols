using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace lols;

public partial class MainView : UserControl
{
    const int Max = 500;
    int count = 0;
    readonly System.Timers.Timer timer = new System.Timers.Timer(500);
    readonly Stopwatch stopwatch = new Stopwatch();
    private readonly LolsView? absolute;
    private readonly Label? lols;

    public MainView()
    {
        InitializeComponent();
        
        absolute = this.FindControl<LolsView>("absolute");
        lols = this.FindControl<Label>("lols");
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
        _ = Task.Run(RunTest);
    }

    void OnTimer(object? sender, System.Timers.ElapsedEventArgs e)
    {
        double avg = count / stopwatch.Elapsed.TotalSeconds;
        string text = "LOL/s: " + avg.ToString("0.00", CultureInfo.InvariantCulture);
        Dispatcher.UIThread.Post(() => UpdateText(text));
    }

    void UpdateText(string text) => lols.Content = text;

    async void RunTest()
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
                var tcs = new TaskCompletionSource();
                Dispatcher.UIThread.Post(a => ((TaskCompletionSource)a).SetResult(), tcs, DispatcherPriority.MinValue);
                await Task.WhenAll(Task.Delay(1), tcs.Task);
            }
        }

        stopwatch.Stop();
        timer.Stop();
    }
}

