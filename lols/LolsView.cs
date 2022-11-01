using System;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Lightspeed;

namespace lols;

public class LolsView : Control
{
    private readonly SurfaceDrawable _surface = new ();

    public void AddLol(double width, double height)
    {
        TextDrawable? lol = null;
        if (_surface.Drawables.Count >= 500)
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
        lol.Text = "lol?";

        _surface.Drawables.Enqueue(lol);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        _surface.Draw(context);
    }
}
