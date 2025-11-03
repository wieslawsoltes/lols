using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace lols;

public partial class FlutterMainView : UserControl
{
    private int _count;
    private readonly System.Timers.Timer _timer = new(500);
    private readonly Stopwatch _stopwatch = new();
    private readonly bool _isBrowser;
    private bool _started;

    private Control? _lolsLabel;
    private Control? _startLabel;

    private static readonly IBrush StartIdleBrush = Brushes.Gray;
    private static readonly IBrush StartHoverBrush = Brushes.LightGray;

    public FlutterMainView()
    {
        InitializeComponent();

        _isBrowser = OperatingSystem.IsBrowser();
        EnsureLabels();
    }

    protected override void OnLoaded(RoutedEventArgs routedEventArgs)
    {
        base.OnLoaded(routedEventArgs);
    }

    private void EnsureLabels()
    {
        if (_lolsLabel is null)
        {
            _lolsLabel = CreateScoreLabel();
            lolsHost.Content = _lolsLabel;
        }

        if (_startLabel is null)
        {
            _startLabel = CreateStartLabel();
            startHost.Content = _startLabel;
        }
    }

    private Control CreateScoreLabel()
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

    private Control CreateStartLabel()
    {
        var control = TextDisplayFactory.Rent();
        control.HorizontalAlignment = HorizontalAlignment.Center;
        control.VerticalAlignment = VerticalAlignment.Bottom;
        TextDisplayFactory.SetText(control, "Start");
        TextDisplayFactory.SetForeground(control, Brushes.White);
        TextDisplayFactory.SetBackground(control, StartIdleBrush);
        TextDisplayFactory.SetPadding(control, new Thickness(7));
        control.Cursor = new Cursor(StandardCursorType.Hand);

        control.PointerEntered += OnStartPointerEntered;
        control.PointerExited += OnStartPointerExited;
        control.PointerPressed += OnStartPointerPressed;

        return control;
    }

    private void OnStartPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_started)
        {
            _started = true;
            Start();
        }
    }

    private void OnStartPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Control control)
        {
            TextDisplayFactory.SetBackground(control, StartHoverBrush);
        }
    }

    private void OnStartPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Control control)
        {
            TextDisplayFactory.SetBackground(control, StartIdleBrush);
        }
    }

    private void Start()
    {
        _timer.Elapsed += OnTimer;

        _stopwatch.Start();
        _timer.Start();
        _ = _isBrowser
            ? Task.Run(RunTest)
            : Task.Factory.StartNew(RunTest, TaskCreationOptions.LongRunning);
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

    private async void RunTest()
    {
        var width = canvas.Bounds.Width;
        var height = canvas.Bounds.Height;
        var step = _isBrowser ? 256 : 1_048_576;

        while (_count < 30_000_000)
        {
            canvas.AddLol(width, height);
            Dispatcher.UIThread.Post(() => canvas.InvalidateVisual());
            _count++;

            if (_count % step == 0)
            {
                if (_isBrowser)
                {
                    var tcs = new TaskCompletionSource();
                    Dispatcher.UIThread.Post(static o => ((TaskCompletionSource)o!).SetResult(), tcs, DispatcherPriority.Background);
                    await Task.WhenAll(Task.Delay(1), tcs.Task);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        _stopwatch.Stop();
        _timer.Stop();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        _timer.Elapsed -= OnTimer;
        _timer.Stop();

        if (_startLabel is { })
        {
            _startLabel.PointerEntered -= OnStartPointerEntered;
            _startLabel.PointerExited -= OnStartPointerExited;
            _startLabel.PointerPressed -= OnStartPointerPressed;
            startHost.Content = null;
            TextDisplayFactory.Return(_startLabel);
            _startLabel = null;
        }

        if (_lolsLabel is { })
        {
            lolsHost.Content = null;
            TextDisplayFactory.Return(_lolsLabel);
            _lolsLabel = null;
        }
    }
}
