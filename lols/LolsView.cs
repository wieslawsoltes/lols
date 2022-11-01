using System;
using Avalonia.Controls;
using Avalonia.Lightspeed;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace lols;

public class LolsView : Control
{
    private readonly SurfaceDrawable _surface = new ();

    public void AddLol(double width, double height)
    {
        if (_surface.Drawables.Count >= 500)
        {
            _surface.Drawables.TryDequeue(out _);
        }

        var random = Random.Shared;
        Span<byte> rgb = stackalloc byte[3];
        random.NextBytes(rgb);

        var lol = new TextDrawable
        {
            Rotation = random.NextDouble() * 360d,
            X = random.NextDouble() * width,
            Y = random.NextDouble() * height,
            Foreground = new ImmutableSolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2])),
            Text = "lol?"
        };

        _surface.Drawables.Enqueue(lol);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        _surface.Draw(context);
    }
}
