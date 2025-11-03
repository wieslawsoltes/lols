using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using lols.Widgets;

namespace lols;

internal static class TextDisplayFactory
{
    private static readonly object s_fastLock = new();
    private static readonly Queue<FastTextBlock> s_fastPool = new();

    private static readonly object s_textLock = new();
    private static readonly Queue<TextBlock> s_textPool = new();

    public static Control Rent()
    {
        if (BenchmarkOptions.UseFastTextBlock)
        {
            FastTextBlock? control = null;
            lock (s_fastLock)
            {
                if (s_fastPool.Count > 0)
                {
                    control = s_fastPool.Dequeue();
                }
            }

            control ??= new FastTextBlock();
            Reset(control);
            return control;
        }
        else
        {
            TextBlock? control = null;
            lock (s_textLock)
            {
                if (s_textPool.Count > 0)
                {
                    control = s_textPool.Dequeue();
                }
            }

            control ??= new TextBlock();
            Reset(control);
            return control;
        }
    }

    public static void Return(Control control)
    {
        Reset(control);

        switch (control)
        {
            case FastTextBlock fast:
                lock (s_fastLock)
                {
                    s_fastPool.Enqueue(fast);
                }

                break;

            case TextBlock text:
                lock (s_textLock)
                {
                    s_textPool.Enqueue(text);
                }

                break;
        }
    }

    public static void SetText(Control control, string text)
    {
        switch (control)
        {
            case FastTextBlock fast:
                fast.Text = text;
                break;
            case TextBlock block:
                block.Text = text;
                break;
        }
    }

    public static void SetForeground(Control control, IBrush? brush)
    {
        switch (control)
        {
            case FastTextBlock fast:
                fast.Foreground = brush;
                break;
            case TextBlock block:
                block.Foreground = brush;
                break;
        }
    }

    public static void SetBackground(Control control, IBrush? brush)
    {
        switch (control)
        {
            case FastTextBlock fast:
                fast.Background = brush;
                break;
            case TextBlock block:
                block.Background = brush;
                break;
        }
    }

    public static void SetPadding(Control control, Thickness padding)
    {
        switch (control)
        {
            case FastTextBlock fast:
                fast.Padding = padding;
                break;
            case TextBlock block:
                block.Padding = padding;
                break;
        }
    }

    private static void Reset(Control control)
    {
        SetText(control, string.Empty);
        SetForeground(control, null);
        SetBackground(control, null);
        SetPadding(control, default);

        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        control.VerticalAlignment = VerticalAlignment.Stretch;
        control.RenderTransform = null;
        control.Width = double.NaN;
        control.Height = double.NaN;

        Canvas.SetLeft(control, double.NaN);
        Canvas.SetTop(control, double.NaN);
    }
}
