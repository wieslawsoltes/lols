using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace lols;

public partial class MainView : UserControl
{
    const int Max = 500;
    int count = 0;
    readonly System.Timers.Timer timer = new System.Timers.Timer(500);
    readonly Stopwatch stopwatch = new Stopwatch();

    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();

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

    void UpdateText(string text) => lols.Text = text;

    void RunTest()
    {
        var random = Random.Shared;
        var width = canvas.Bounds.Width;
        var height = canvas.Bounds.Height;

        while (count < 5000)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var rgb = new byte[3];
                random.NextBytes(rgb);

                var textBlock = new TextBlock
                {
                    Text = "lol?",
                    Foreground = new SolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2])),
                    RenderTransform = new RotateTransform(random.NextDouble() * 360f),
                    Width = 80,
                    Height = 40
                };

                Canvas.SetLeft(textBlock, random.NextDouble() * width);
                Canvas.SetTop(textBlock, random.NextDouble() * height);

                if (canvas.Children.Count >= Max)
                    canvas.Children.RemoveAt(0);
                canvas.Children.Add(textBlock);

                count++;
            });

            Thread.Sleep(1);
        }

        stopwatch.Stop();
        timer.Stop();
    }
}
