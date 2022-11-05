using System.Collections.Concurrent;
using Avalonia.Media;

namespace lols.Widgets;

public class SurfaceWidget : Widget
{
    public ConcurrentQueue<Widget> Children { get; set; } = new();

    public override void Draw(DrawingContext context)
    {
        foreach (var drawable in Children)
        {
            drawable.Draw(context);
        }
    }
}
