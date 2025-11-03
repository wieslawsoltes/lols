using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using Avalonia.VisualTree;

namespace lols.Widgets;

/// <summary>
/// Lightweight text control tuned for high-frequency updates.
/// </summary>
public class FastTextBlock : Control
{

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<FastTextBlock, string?>(nameof(Text));

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<FastTextBlock>();

    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<FastTextBlock>();

    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        TextElement.FontStyleProperty.AddOwner<FastTextBlock>();

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        TextElement.FontWeightProperty.AddOwner<FastTextBlock>();

    public static readonly StyledProperty<FontStretch> FontStretchProperty =
        TextElement.FontStretchProperty.AddOwner<FastTextBlock>();

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<FastTextBlock>();

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<FastTextBlock, IBrush?>(nameof(Background));

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<FastTextBlock, TextAlignment>(nameof(TextAlignment), TextAlignment.Start);

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<FastTextBlock, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);

    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<FastTextBlock, Thickness>(nameof(Padding));

    private TextLayout? _textLayout;
    private Size _lastConstraint = new(double.NaN, double.NaN);
    private Size _contentSize;
    private Size _desiredSize;
    private bool _layoutDirty = true;
    private LayoutParameters _layoutParams;
    private double _overhangLeading;
    private double _overhangTrailing;
    private double _overhangBefore;
    private double _overhangAfter;

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public FontStretch FontStretch
    {
        get => GetValue(FontStretchProperty);
        set => SetValue(FontStretchProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        var property = change.Property;

        if (property == TextProperty ||
            property == FontSizeProperty ||
            property == FontFamilyProperty ||
            property == FontWeightProperty ||
            property == FontStyleProperty ||
            property == FontStretchProperty ||
            property == TextWrappingProperty ||
            property == PaddingProperty)
        {
            OnLayoutPropertyChanged();
        }
        else if (property == TextAlignmentProperty || property == ForegroundProperty)
        {
            OnLayoutPropertyChanged(affectsMeasure: false);
        }
        else if (property == BackgroundProperty)
        {
            InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var contentConstraint = GetContentConstraint(availableSize);
        EnsureLayout(contentConstraint);
        return _desiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var contentConstraint = GetContentConstraint(finalSize);
        EnsureLayout(contentConstraint);
        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var background = Background;
        if (background is not null)
        {
            context.FillRectangle(background, new Rect(Bounds.Size));
        }

        if (_textLayout is null)
        {
            EnsureLayout(GetContentConstraint(Bounds.Size));
        }

        if (_textLayout is null)
        {
            return;
        }

        var padding = Padding;
        var available = Bounds.Size;
        var contentWidth = _contentSize.Width;
        var contentHeight = _contentSize.Height;

        var contentAreaWidth = Math.Max(0, available.Width - padding.Left - padding.Right);
        var contentAreaHeight = Math.Max(0, available.Height - padding.Top - padding.Bottom);

        double originX = padding.Left - _overhangLeading;
        double originY = padding.Top - _overhangBefore;

        switch (HorizontalAlignment)
        {
            case HorizontalAlignment.Center:
                originX += ClampOffset(contentAreaWidth, contentWidth, (contentAreaWidth - contentWidth) / 2);
                break;
            case HorizontalAlignment.Right:
                originX += ClampOffset(contentAreaWidth, contentWidth, contentAreaWidth - contentWidth);
                break;
        }

        switch (VerticalAlignment)
        {
            case VerticalAlignment.Center:
                originY += ClampOffset(contentAreaHeight, contentHeight, (contentAreaHeight - contentHeight) / 2);
                break;
            case VerticalAlignment.Bottom:
                originY += ClampOffset(contentAreaHeight, contentHeight, contentAreaHeight - contentHeight);
                break;
        }

        _textLayout.Draw(context, new Point(originX, originY));
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _textLayout?.Dispose();
        _textLayout = null;
        _contentSize = default;
        _desiredSize = default;
        _overhangLeading = _overhangTrailing = _overhangBefore = _overhangAfter = 0;
        _lastConstraint = new Size(double.NaN, double.NaN);
        _layoutParams = LayoutParameters.Invalid;
        _layoutDirty = true;
    }

    private void OnLayoutPropertyChanged(bool affectsMeasure = true)
    {
        if (affectsMeasure)
        {
            var previousDesired = _desiredSize;

            if (!double.IsNaN(_lastConstraint.Width) && !double.IsNaN(_lastConstraint.Height))
            {
                var parameters = CreateLayoutParameters(_lastConstraint);
                var layout = BuildLayout(parameters);
                ReplaceLayout(layout, parameters, _lastConstraint);

                if (!NearlyEquals(_desiredSize, previousDesired))
                {
                    InvalidateMeasure();
                }
                else
                {
                    InvalidateVisual();
                }
            }
            else
            {
                _layoutDirty = true;
                InvalidateMeasure();
            }
        }
        else
        {
            InvalidateVisual();
        }
    }

    private void EnsureLayout(Size contentConstraint)
    {
        var parameters = CreateLayoutParameters(contentConstraint);

        if (!_layoutDirty && _layoutParams.IsValid && _layoutParams.Equals(parameters))
        {
            return;
        }

        var layout = BuildLayout(parameters);
        ReplaceLayout(layout, parameters, contentConstraint);
    }

    private LayoutParameters CreateLayoutParameters(Size contentConstraint)
    {
        var text = Text ?? string.Empty;
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        var maxWidth = double.IsPositiveInfinity(contentConstraint.Width) ? double.PositiveInfinity : Math.Max(0, contentConstraint.Width);
        var maxHeight = double.IsPositiveInfinity(contentConstraint.Height) ? double.PositiveInfinity : Math.Max(0, contentConstraint.Height);

        return new LayoutParameters(
            text,
            typeface,
            FontSize,
            TextAlignment,
            TextWrapping,
            FlowDirection,
            maxWidth,
            maxHeight);
    }

    private TextLayout BuildLayout(LayoutParameters parameters)
    {
        return new TextLayout(
            parameters.Text,
            parameters.Typeface,
            fontFeatures: null,
            fontSize: parameters.FontSize,
            Foreground,
            textAlignment: parameters.TextAlignment,
            textWrapping: parameters.TextWrapping,
            textTrimming: TextTrimming.None,
            textDecorations: null,
            flowDirection: parameters.FlowDirection,
            maxWidth: parameters.MaxWidth,
            maxHeight: parameters.MaxHeight);
    }

    private void ReplaceLayout(TextLayout layout, LayoutParameters parameters, Size contentConstraint)
    {
        var oldLayout = _textLayout;
        _textLayout = layout;
        oldLayout?.Dispose();

        _overhangLeading = layout.OverhangLeading;
        _overhangTrailing = layout.OverhangTrailing;
        _overhangBefore = CalculateTopOverhang(layout);
        _overhangAfter = layout.OverhangAfter;

        var width = layout.WidthIncludingTrailingWhitespace + _overhangLeading + _overhangTrailing;
        var height = layout.Height + _overhangBefore + _overhangAfter;

        _contentSize = new Size(Math.Max(0, Math.Ceiling(width)), Math.Max(0, Math.Ceiling(height)));
        _lastConstraint = contentConstraint;
        _layoutParams = parameters;
        _desiredSize = Inflate(_contentSize);
        _layoutDirty = false;
    }

    private Size GetContentConstraint(Size available)
    {
        var padding = Padding;
        var width = available.Width;
        var height = available.Height;

        if (!double.IsPositiveInfinity(width))
        {
            width = Math.Max(0, width - padding.Left - padding.Right);
        }

        if (!double.IsPositiveInfinity(height))
        {
            height = Math.Max(0, height - padding.Top - padding.Bottom);
        }

        return new Size(width, height);
    }

    private Size Inflate(Size contentSize)
    {
        var padding = Padding;
        return new Size(
            contentSize.Width + padding.Left + padding.Right,
            contentSize.Height + padding.Top + padding.Bottom);
    }

    private static bool NearlyEquals(Size a, Size b)
    {
        return MathUtilities.AreClose(a.Width, b.Width) && MathUtilities.AreClose(a.Height, b.Height);
    }

    private static double ClampOffset(double available, double content, double offset)
    {
        if (double.IsPositiveInfinity(available))
        {
            return offset;
        }

        var maxOffset = Math.Max(0, available - content);
        return MathUtilities.Clamp(offset, 0, maxOffset);
    }

    private static double CalculateTopOverhang(TextLayout layout)
    {
        var aboveBaseline = layout.Extent - layout.OverhangAfter;
        var overhang = aboveBaseline - layout.Baseline;

        if (double.IsNaN(overhang) || double.IsInfinity(overhang))
        {
            return 0;
        }

        return Math.Max(0, overhang);
    }

    private readonly struct LayoutParameters : IEquatable<LayoutParameters>
    {
        public readonly string Text;
        public readonly Typeface Typeface;
        public readonly double FontSize;
        public readonly TextAlignment TextAlignment;
        public readonly TextWrapping TextWrapping;
        public readonly FlowDirection FlowDirection;
        public readonly double MaxWidth;
        public readonly double MaxHeight;
        public readonly bool IsValid;

        public LayoutParameters(string text, Typeface typeface, double fontSize, TextAlignment textAlignment, TextWrapping textWrapping, FlowDirection flowDirection, double maxWidth, double maxHeight)
        {
            Text = text;
            Typeface = typeface;
            FontSize = fontSize;
            TextAlignment = textAlignment;
            TextWrapping = textWrapping;
            FlowDirection = flowDirection;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            IsValid = true;
        }

        public bool Equals(LayoutParameters other)
        {
            if (!IsValid || !other.IsValid)
            {
                return false;
            }

            return Text == other.Text &&
                   Typeface.Equals(other.Typeface) &&
                   MathUtilities.AreClose(FontSize, other.FontSize) &&
                   TextAlignment == other.TextAlignment &&
                   TextWrapping == other.TextWrapping &&
                   FlowDirection == other.FlowDirection &&
                   MathUtilities.AreClose(MaxWidth, other.MaxWidth) &&
                   MathUtilities.AreClose(MaxHeight, other.MaxHeight);
        }

        public override bool Equals(object? obj) => obj is LayoutParameters other && Equals(other);

        public override int GetHashCode()
        {
            var hash = Text.GetHashCode();
            hash = (hash * 397) ^ Typeface.GetHashCode();
            hash = (hash * 397) ^ FontSize.GetHashCode();
            hash = (hash * 397) ^ (int)TextAlignment;
            hash = (hash * 397) ^ (int)TextWrapping;
            hash = (hash * 397) ^ (int)FlowDirection;
            hash = (hash * 397) ^ MaxWidth.GetHashCode();
            hash = (hash * 397) ^ MaxHeight.GetHashCode();
            return hash;
        }

        public static LayoutParameters Invalid => default;
    }
}
