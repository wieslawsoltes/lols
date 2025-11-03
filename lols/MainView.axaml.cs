using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace lols;

public partial class MainView : UserControl
{
    private const int Max = 500;
    private int _count;
    private readonly System.Timers.Timer _timer = new(500);
    private readonly Stopwatch _stopwatch = new();
    private Control? _lolsLabel;

    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs routedEventArgs)
    {
        base.OnLoaded(routedEventArgs);

        EnsureScoreLabel();
        Start();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        _timer.Elapsed -= OnTimer;
        _timer.Stop();

        while (canvas.Children.Count > 0)
        {
            if (canvas.Children[0] is Control control)
            {
                canvas.Children.RemoveAt(0);
                TextDisplayFactory.Return(control);
            }
            else
            {
                canvas.Children.RemoveAt(0);
            }
        }

        if (_lolsLabel is { })
        {
            lolsHost.Content = null;
            TextDisplayFactory.Return(_lolsLabel);
            _lolsLabel = null;
        }
    }

    private void Start()
    {
        _timer.Elapsed += OnTimer;

        _stopwatch.Start();
        _timer.Start();
        _ = Task.Factory.StartNew(RunTest, TaskCreationOptions.LongRunning);
    }

    private void OnTimer(object? sender, System.Timers.ElapsedEventArgs e)
    {
        double avg = _count / _stopwatch.Elapsed.TotalSeconds;
        string text = "LOL/s: " + avg.ToString("0.00", CultureInfo.InvariantCulture);
        Dispatcher.UIThread.Post(() => UpdateText(text));
    }

    private void UpdateText(string text)
    {
        if (_lolsLabel is { } label)
        {
            TextDisplayFactory.SetText(label, text);
        }
    }

    private void RunTest()
    {
        var random = Random.Shared;
        var width = canvas.Bounds.Width;
        var height = canvas.Bounds.Height;

        while (_count < 5000)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var rgb = new byte[3];
                random.NextBytes(rgb);

                var textControl = CreateDynamicTextDisplay(
                    new SolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2])),
                    random.NextDouble() * 360d);

                Canvas.SetLeft(textControl, random.NextDouble() * width);
                Canvas.SetTop(textControl, random.NextDouble() * height);

                if (canvas.Children.Count >= Max)
                {
                    if (canvas.Children[0] is Control old)
                    {
                        canvas.Children.RemoveAt(0);
                        TextDisplayFactory.Return(old);
                    }
                    else
                    {
                        canvas.Children.RemoveAt(0);
                    }
                }

                canvas.Children.Add(textControl);
                _count++;
            });

            Thread.Sleep(1);
        }

        _stopwatch.Stop();
        _timer.Stop();
    }

    private void EnsureScoreLabel()
    {
        if (_lolsLabel is not null)
        {
            return;
        }

        _lolsLabel = CreateScoreLabel();
        lolsHost.Content = _lolsLabel;
    }

    private static Control CreateScoreLabel()
    {
        var control = TextDisplayFactory.Rent();
        control.HorizontalAlignment = HorizontalAlignment.Center;
        control.VerticalAlignment = VerticalAlignment.Center;
        TextDisplayFactory.SetText(control, "LOL/s:");
        TextDisplayFactory.SetBackground(control, Brushes.Red);
        TextDisplayFactory.SetForeground(control, Brushes.White);
        TextDisplayFactory.SetPadding(control, new Thickness(7));
        return control;
    }

    private static Control CreateDynamicTextDisplay(IBrush foreground, double rotation)
    {
        var control = TextDisplayFactory.Rent();
        TextDisplayFactory.SetText(control, "lol?");
        TextDisplayFactory.SetForeground(control, foreground);
        control.RenderTransform = new RotateTransform(rotation);
        control.Width = 80;
        control.Height = 40;
        return control;
    }
}
