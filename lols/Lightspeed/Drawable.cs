using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Lightspeed;

public abstract class Drawable
{
    public double X { get; set; }

    public double Y { get; set; }

    public double Rotation { get; set; }

    public ImmutableSolidColorBrush? Foreground { get; set; }

    public abstract void Draw(DrawingContext context);
}
