using System.Collections.Concurrent;
using Avalonia.Media;

namespace Lightspeed;

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
