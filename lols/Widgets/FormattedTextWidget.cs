using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace lols.Widgets;

public class FormattedTextWidget : Widget
{
    private FormattedText? _formattedText;
    private Matrix _rotateMatrix;

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

            _rotateMatrix = Matrix.CreateTranslation(-X, -Y)
                      * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                      * Matrix.CreateTranslation(X, Y);
        }

        using var d = context.PushPostTransform(_rotateMatrix);
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

        _rotateMatrix = Matrix.CreateTranslation(-X, -Y)
                  * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                  * Matrix.CreateTranslation(X, Y);
    }
}
