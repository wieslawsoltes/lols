using Avalonia;
using Avalonia.Media;

namespace lols.Widgets;

public class GlyphRunWidget : TextWidget
{
    private GlyphRun? _glyphRun;
    private Matrix _rotateMatrix;
    private Matrix _translateMatrix;

    private GlyphRun GetGlyphRun(string text, Typeface typeface, double fontSize)
    {
        var glyphIndices = new ushort[text.Length];
        var glyphTypeface = typeface.GlyphTypeface;

        for (var i = 0; i < text.Length; i++)
        {
            glyphIndices[i] = glyphTypeface.GetGlyph(text[i]);
        }

        return new GlyphRun(typeface.GlyphTypeface, fontSize, text.ToCharArray(), glyphIndices);
    }
    
    public override void Draw(DrawingContext context)
    {
        if (Text is null || Foreground is null)
        {
            return;
        }

        if (_glyphRun is null)
        {
            _glyphRun = GetGlyphRun(Text, Typeface.Default, EmSize);

            _translateMatrix = Matrix.CreateTranslation(X, Y);
 
            _rotateMatrix = Matrix.CreateTranslation(-X, -Y)
                            * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                            * Matrix.CreateTranslation(X, Y);
        }

        using var translate = context.PushTransform(_translateMatrix);
        using var rotate = context.PushTransform(_rotateMatrix);

        context.DrawGlyphRun(Foreground, _glyphRun);
    }

    public override void Invalidate()
    {
        if (_glyphRun is null)
        {
            return;
        }

        _translateMatrix = Matrix.CreateTranslation(X, Y);
        
        _rotateMatrix = Matrix.CreateTranslation(-X, -Y)
                        * Matrix.CreateRotation(Matrix.ToRadians(Rotation))
                        * Matrix.CreateTranslation(X, Y);
    }
}
