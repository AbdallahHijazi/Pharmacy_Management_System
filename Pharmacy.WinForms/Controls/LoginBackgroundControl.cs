using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

/// <summary>
/// Full-client medical backdrop (gradient, soft circles, ECG). Centers a hosted card on layout.
/// </summary>
public sealed class LoginBackgroundControl : Panel
{
    private Control? _hostedCard;

    public LoginBackgroundControl()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        BackColor = PharmaTheme.LoginGradientTop;
    }

    /// <summary>Registers the login card; must be called once after the card is constructed.</summary>
    public void SetHostedCard(Control card)
    {
        if (_hostedCard is not null)
        {
            Controls.Remove(_hostedCard);
        }

        _hostedCard = card;
        card.Anchor = AnchorStyles.None;
        Controls.Add(card);
        PerformLayout();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        if (_hostedCard is null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        var horizontalMargin = Math.Max(16, ClientSize.Width / 28);
        var maxCardWidth = Math.Max(
            PharmaTheme.LoginCardMinWidth,
            Math.Min(PharmaTheme.LoginCardMaxWidth, ClientSize.Width - horizontalMargin * 2));
        _hostedCard.MaximumSize = new Size(maxCardWidth, 8000);
        _hostedCard.MinimumSize = new Size(
            Math.Min(PharmaTheme.LoginCardMinWidth, maxCardWidth),
            120);
        _hostedCard.Width = maxCardWidth;
        _hostedCard.Left = (ClientSize.Width - _hostedCard.Width) / 2;
        _hostedCard.Top = Math.Max(16, (ClientSize.Height - _hostedCard.Height) / 2);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = ClientRectangle;
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        using (var brush = new LinearGradientBrush(
                   rect,
                   PharmaTheme.LoginGradientTop,
                   PharmaTheme.LoginGradientBottom,
                   42F))
        {
            g.FillRectangle(brush, rect);
        }

        DrawSoftCircles(g, rect);
        DrawEcghLine(g, rect);
        DrawCrossIcons(g, rect);
    }

    private static void DrawSoftCircles(Graphics g, Rectangle bounds)
    {
        var scale = Math.Min(bounds.Width, bounds.Height) / 900f;
        if (scale < 0.35f)
        {
            scale = 0.35f;
        }

        void Circle(float cx, float cy, float radius, int alpha)
        {
            var r = radius * scale;
            var x = bounds.X + cx * bounds.Width - r;
            var y = bounds.Y + cy * bounds.Height - r;
            var d = r * 2;
            using var b = new SolidBrush(Color.FromArgb(alpha, PharmaTheme.PrimaryGreen));
            g.FillEllipse(b, x, y, d, d);
        }

        Circle(0.08f, 0.12f, 140, 18);
        Circle(0.92f, 0.18f, 110, 14);
        Circle(0.85f, 0.78f, 160, 16);
        Circle(0.12f, 0.82f, 120, 12);
        Circle(0.5f, 0.06f, 90, 10);
    }

    private static void DrawEcghLine(Graphics g, Rectangle bounds)
    {
        var bandTop = bounds.Y + (int)(bounds.Height * 0.68);
        var bandH = Math.Max(40, (int)(bounds.Height * 0.12));
        var left = bounds.X + (int)(bounds.Width * 0.06);
        var right = bounds.Right - (int)(bounds.Width * 0.06);
        if (right <= left + 40)
        {
            return;
        }

        var midY = bandTop + bandH / 2f;
        var w = right - left;
        const int steps = 120;
        var pts = new PointF[steps];
        for (var i = 0; i < steps; i++)
        {
            var t = i / (float)(steps - 1);
            var x = left + t * w;
            float y;
            if (t is > 0.18f and < 0.22f)
            {
                y = midY - 22;
            }
            else if (t is > 0.22f and < 0.26f)
            {
                y = midY + 18;
            }
            else if (t is > 0.34f and < 0.38f)
            {
                y = midY - 10;
            }
            else if (t is > 0.38f and < 0.42f)
            {
                y = midY + 8;
            }
            else if (t is > 0.52f and < 0.58f)
            {
                y = midY - 28;
            }
            else if (t is > 0.58f and < 0.64f)
            {
                y = midY + 12;
            }
            else
            {
                y = midY + (float)(Math.Sin(t * Math.PI * 6) * 2.5);
            }

            pts[i] = new PointF(x, y);
        }

        using var pen = new Pen(Color.FromArgb(55, PharmaTheme.AccentTeal), Math.Max(1.2f, 1.6f * Math.Min(1f, bounds.Width / 900f)))
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        g.DrawLines(pen, pts);
    }

    private static void DrawCrossIcons(Graphics g, Rectangle bounds)
    {
        using var pen = new Pen(Color.FromArgb(28, PharmaTheme.PrimaryGreen), 1.6f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        var s = Math.Max(14, Math.Min(22, bounds.Width / 48));
        void Plus(float nx, float ny)
        {
            var cx = bounds.X + nx * bounds.Width;
            var cy = bounds.Y + ny * bounds.Height;
            g.DrawLine(pen, cx - s, cy, cx + s, cy);
            g.DrawLine(pen, cx, cy - s, cx, cy + s);
        }

        Plus(0.18f, 0.35f);
        Plus(0.78f, 0.42f);
        Plus(0.72f, 0.88f);
    }
}

/// <summary>Rounded elevated card with soft shadow (no transparent control backgrounds).</summary>
public sealed class LoginCardPanel : Panel
{
    private const int CornerRadius = 22;
    private const int ShadowInset = 6;

    public LoginCardPanel()
    {
        DoubleBuffered = true;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        BackColor = PharmaTheme.LoginCardFill;
        Margin = new Padding(8);
        Padding = new Padding(32 + ShadowInset, 28 + ShadowInset, 32 + ShadowInset, 28 + ShadowInset);
        RightToLeft = RightToLeft.Yes;
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        var innerWidth = ClientSize.Width - Padding.Horizontal;
        if (innerWidth <= 0)
        {
            return;
        }

        foreach (Control child in Controls)
        {
            child.Width = innerWidth;
            child.Left = Padding.Left;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(
            ShadowInset,
            ShadowInset,
            Width - ShadowInset * 2 - 1,
            Height - ShadowInset * 2 - 1);
        if (rect.Width <= 8 || rect.Height <= 8)
        {
            return;
        }

        for (var i = 3; i >= 1; i--)
        {
            var offset = i * 2;
            var alpha = 8 + i * 4;
            using var path = RoundedRect(
                new Rectangle(rect.X + offset, rect.Y + offset, rect.Width, rect.Height),
                CornerRadius);
            using var brush = new SolidBrush(Color.FromArgb(alpha, 20, 40, 32));
            g.FillPath(brush, path);
        }

        using (var path = RoundedRect(rect, CornerRadius))
        {
            using var fill = new SolidBrush(PharmaTheme.LoginCardFill);
            g.FillPath(fill, path);
            using var border = new Pen(PharmaTheme.LoginCardBorder, 1.2F);
            g.DrawPath(border, path);
        }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        using var brush = new SolidBrush(Parent?.BackColor ?? PharmaTheme.LoginGradientTop);
        pevent.Graphics.FillRectangle(brush, ClientRectangle);
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
