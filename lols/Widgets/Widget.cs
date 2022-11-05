using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace lols.Widgets;

public abstract class Widget
{
    public double X { get; set; }

    public double Y { get; set; }

    public double Rotation { get; set; }

    public ImmutableSolidColorBrush? Foreground { get; set; }

    public abstract void Draw(DrawingContext context);
}
