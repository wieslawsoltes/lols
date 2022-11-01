using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace Lightspeed;

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
                14f,
                Foreground);

            _matrix =
                Matrix.CreateTranslation(-X, -Y)
                * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                * Matrix.CreateTranslation(X, Y);
        }

        using var d = context.PushPostTransform(_matrix);
        context.DrawText(_formattedText, new Point(X, Y));
    }
}
