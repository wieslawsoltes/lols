namespace lols.Widgets;

public abstract class TextWidget : Widget
{
    public string? Text { get; set; }

    public double EmSize { get; set; }

    public abstract void Invalidate();
}
