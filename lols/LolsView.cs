using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using lols.Widgets;

namespace lols;

public class LolsView : Control
{
    private readonly SurfaceWidget _surfaceWidget = new ();
    const int Max = 500;
    readonly bool _isBrowser;

    private TextWidget CreateTextWidget()
    {
        return _isBrowser 
            ? new FormattedTextWidget() 
            : new GlyphRunWidget();
    }

    public LolsView()
    {
        _isBrowser = OperatingSystem.IsBrowser();
    }
    
    public void AddLol(double width, double height)
    {
        TextWidget? lol = null;
        if (_surfaceWidget.Children.Count >= Max)
        {
            _surfaceWidget.Children.TryDequeue(out var drawable);
            if (drawable is TextWidget textWidget)
            {
                lol = textWidget;
            }
        }

        var random = Random.Shared;
        Span<byte> rgb = stackalloc byte[3];
        random.NextBytes(rgb);

        lol ??= CreateTextWidget();
        lol.Rotation = random.NextDouble() * 360d;
        lol.X = random.NextDouble() * width;
        lol.Y = random.NextDouble() * height;
        lol.Foreground = new ImmutableSolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2]));
        lol.EmSize = 14f;
        lol.Text = "lol?";
        lol.Invalidate();

        _surfaceWidget.Children.Enqueue(lol);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        _surfaceWidget.Draw(context);
    }
}
