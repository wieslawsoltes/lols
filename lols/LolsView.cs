using System;
using System.Collections.Concurrent;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace lols;

public class LolsView : Canvas
{ 
    ConcurrentQueue<Lol> lols = new();

    public void AddLol(double width, double height)
    {
        if (lols.Count >= 500)
        {
            lols.TryDequeue(out _);
        }
        lols.Enqueue(new Lol(width, height));
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        foreach (var lol in lols)
        {
            var m = 
                Matrix.CreateTranslation(-lol.X, -lol.Y)
                * Matrix.CreateRotation(Matrix.ToRadians(lol.Rotation)) 
                * Matrix.CreateTranslation(lol.X, lol.Y);

            using var d = context.PushPostTransform(m);
            context.DrawText(lol.Text, new Point(lol.X, lol.Y));
        }
    }

    class Lol
    {
        public Lol(double width, double height)
        {
            var random = Random.Shared;
            Rotation = random.NextDouble() * 360d;
            X = random.NextDouble() * width;
            Y = random.NextDouble() * height;
            Foreground = new ImmutableSolidColorBrush(
                Color.FromRgb(
                    (byte)(random.NextSingle() * 255f), 
                    (byte)(random.NextSingle() * 255f), 
                    (byte)(random.NextSingle() * 255f)));
            Text = new FormattedText(
                "lol?",
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Typeface.Default, 
                14f,
                Foreground);
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double Rotation { get; set; }

        public FormattedText Text { get; set; }
        
        public ImmutableSolidColorBrush Foreground { get; set; }
    }
}
