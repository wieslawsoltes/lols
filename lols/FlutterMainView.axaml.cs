using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;

namespace lols;

public abstract class Drawable
{
    public double X { get; set; }

    public double Y { get; set; }

    public double Rotation { get; set; }

    public ImmutableSolidColorBrush? Foreground { get; set; }

    public abstract void Draw(DrawingContext context);
}

public class SurfaceDrawable : Drawable
{
    public ConcurrentQueue<Drawable> Drawables { get; set; } = new();

    public override void Draw(DrawingContext context)
    {
        foreach (var drawable in Drawables)
        {
            drawable.Draw(context);
        }
    }
}

public class TextDrawable : Drawable
{
    private FormattedText? _formattedText;
    private Matrix _matrix;

    public string? Text { get; set; }

    public double EmSize { get; set; }

    public override void Draw(DrawingContext context)
    {
        if (Text is null)
        {
            return;
        }

        if (_formattedText is null)
        {
            _formattedText = new FormattedText(
                Text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                EmSize,
                Foreground);

            _matrix = Matrix.CreateTranslation(-X, -Y)
                      * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                      * Matrix.CreateTranslation(X, Y);
        }

        using var d = context.PushPostTransform(_matrix);
        context.DrawText(_formattedText, new Point(X, Y));
    }

    public void Invalidate()
    {
        if (_formattedText is null)
        {
            return;
        }
        
        if (Foreground is { })
        {
            _formattedText.SetForegroundBrush(Foreground);
        }

        _formattedText.SetFontSize(EmSize);

        _matrix = Matrix.CreateTranslation(-X, -Y)
                  * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                  * Matrix.CreateTranslation(X, Y);
    }
}

public class LolsView : Control
{
    private readonly SurfaceDrawable _surface = new ();
    const int Max = 500;

    public void AddLol(double width, double height)
    {
        TextDrawable? lol = null;
        if (_surface.Drawables.Count >= Max)
        {
            _surface.Drawables.TryDequeue(out var drawable);
            if (drawable is TextDrawable textDrawable)
            {
                lol = textDrawable;
            }
        }

        var random = Random.Shared;
        Span<byte> rgb = stackalloc byte[3];
        random.NextBytes(rgb);

        lol ??= new TextDrawable();
        lol.Rotation = random.NextDouble() * 360d;
        lol.X = random.NextDouble() * width;
        lol.Y = random.NextDouble() * height;
        lol.Foreground = new ImmutableSolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2]));
        lol.EmSize = 14f;
        lol.Text = "lol?";
        lol.Invalidate();

        _surface.Drawables.Enqueue(lol);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        _surface.Draw(context);
    }
}
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

        while (count < 10000000)
        {
            canvas.AddLol(width, height);
            Dispatcher.UIThread.Post(() => canvas.InvalidateVisual());
            count++;

            if (count % 32768 == 0)
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

