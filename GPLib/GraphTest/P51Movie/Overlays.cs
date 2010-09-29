using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using OpenTK;
using OpenTK.Graphics;

namespace P51Movie
{
    public class Overlay
    {
        public class OverlayDef
        {
            public enum OverlayKind
            {
                FadeIn,
                FadeOut,
                FadeInOut,
                Scroll,
                Cut,
                Delay,
            }

            public OverlayKind Kind = OverlayKind.Cut;
            public string Text = string.Empty;

            public Vector2 Position = Vector2.Zero;
            public bool Centered = true;

            public int FontSize = 10;

            public double LifeTime = 0;
            public double Speed = 0;
            public Color TextColor = Color.White;

            public bool CenterOrigin = false;

            public System.Drawing.Font Font;
        }

        public List<OverlayDef> Sequence = new List<OverlayDef>();

        public bool Loop = false;

        double SequenceStartTime = 0;
        int SequenceIndex = -1;

        TextPrinter printer = new TextPrinter(TextQuality.High);

        public void Update ( double time, double delta )
        {
            if (Sequence.Count < 1 || SequenceIndex >= Sequence.Count)
                return;

            if (SequenceIndex == -1)
            {
                SequenceIndex = 0;
                SequenceStartTime = time;
            }

            OverlayDef seq = Sequence[SequenceIndex];

            double age = time - SequenceStartTime;
            if (age >= seq.LifeTime)
            {
                SequenceStartTime = time;

                SequenceIndex++;
                if (SequenceIndex >= Sequence.Count && Loop)
                    SequenceIndex = 0;
            }
        }

        public void Draw ( double time )
        {
            if (Sequence.Count < 1 || SequenceIndex >= Sequence.Count)
                return;

            OverlayDef seq = Sequence[SequenceIndex];
            double age = time - SequenceStartTime;

            switch ( seq.Kind )
            {
                case OverlayDef.OverlayKind.Delay:
                    break;

                case OverlayDef.OverlayKind.Cut:
                    DrawText(seq, seq.Position, 1.0f);
                    break;
            }
        }

        protected void DrawText ( OverlayDef overlay, Vector2 pos, float alpha )
        {
          //  string[] lines = overlay.Text.Split("\n");

            RectangleF bounds = printer.Measure(overlay.Text, overlay.Font).BoundingBox;

            printer.Begin();

            RectangleF rect = new RectangleF();

            rect.Size = bounds.Size;

            Vector2 origin = overlay.Position;
            if (overlay.CenterOrigin)
                origin = new Vector2(Sceene.API.Width / 2.0f + overlay.Position.X, Sceene.API.Height / 2.0f + overlay.Position.Y);

            if (overlay.Centered)
                rect.Location = new PointF(origin.X - bounds.Width * 0.5f, Sceene.API.Height - (origin.Y + bounds.Height * 0.5f));
            else
                rect.Location = new PointF(origin.X, Sceene.API.Height - origin.Y);

            TextPrinterOptions options = TextPrinterOptions.Default;
            TextAlignment alignment = TextAlignment.Near;
            if (overlay.Centered)
                alignment = TextAlignment.Center;

            printer.Print(overlay.Text, overlay.Font, overlay.TextColor/*Color.FromArgb((int)(255 * alpha), overlay.TextColor)*/, rect, TextPrinterOptions.Default, alignment);


            printer.End();
        }
    }
}
