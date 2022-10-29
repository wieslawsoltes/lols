using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
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

        while (count < 5000)
        {
            Dispatcher.UIThread.Post(() =>
            {            
                var label = new Label
                {
                    Content = "lol?",
                    Foreground = new SolidColorBrush(Color.FromRgb((byte)(random.NextSingle() * 255f), (byte)(random.NextSingle() * 255f), (byte)(random.NextSingle() * 255f))),
                    RenderTransform = new RotateTransform(random.NextDouble() * 360f),
                    Width = 80,
                    Height = 40
                };
                //AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.PositionProportional);
                //AbsoluteLayout.SetLayoutBounds(label, new Rect(random.NextDouble(), random.NextDouble(), 80, 24));
                var width = absolute.Bounds.Width;
                var height = absolute.Bounds.Height;
                
                Canvas.SetLeft(label, random.NextDouble() * width);
                Canvas.SetTop(label, random.NextDouble() * height);
                if (absolute.Children.Count >= Max)
                    absolute.Children.RemoveAt(0);
                absolute.Children.Add(label);
                count++;
            });
            //NOTE: plain Android we could put 1
            Thread.Sleep(1);
        }

        stopwatch.Stop();
        timer.Stop();
    }
}
