using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

/// <summary>Decorative sales area chart placeholder (no fake data labels).</summary>
public sealed class SalesChartPlaceholderControl : Control
{
    public SalesChartPlaceholderControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        BackColor = PharmaTheme.SurfaceContainerLow;
        MinimumSize = new Size(200, 160);
        RightToLeft = RightToLeft.Yes;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var plot = ClientRectangle;
        plot.Inflate(-16, -40);
        if (plot.Width < 20 || plot.Height < 20)
        {
            return;
        }

        using (var grid = new Pen(Color.FromArgb(35, PharmaTheme.OutlineVariant)))
        {
            for (var i = 1; i <= 3; i++)
            {
                var y = plot.Top + (plot.Height * i / 4);
                g.DrawLine(grid, plot.Left, y, plot.Right, y);
            }
        }

        var points = new[]
        {
            new PointF(plot.Left, plot.Bottom - plot.Height * 0.35f),
            new PointF(plot.Left + plot.Width * 0.15f, plot.Bottom - plot.Height * 0.55f),
            new PointF(plot.Left + plot.Width * 0.32f, plot.Bottom - plot.Height * 0.42f),
            new PointF(plot.Left + plot.Width * 0.5f, plot.Bottom - plot.Height * 0.68f),
            new PointF(plot.Left + plot.Width * 0.68f, plot.Bottom - plot.Height * 0.48f),
            new PointF(plot.Left + plot.Width * 0.85f, plot.Bottom - plot.Height * 0.72f),
            new PointF(plot.Right, plot.Bottom - plot.Height * 0.58f)
        };

        using var areaPath = new GraphicsPath();
        areaPath.AddLines(points);
        areaPath.AddLine(points[^1], new PointF(plot.Right, plot.Bottom));
        areaPath.AddLine(new PointF(plot.Right, plot.Bottom), new PointF(plot.Left, plot.Bottom));
        areaPath.CloseFigure();

        using (var areaBrush = new LinearGradientBrush(plot, Color.FromArgb(90, PharmaTheme.PrimaryGreen), Color.FromArgb(10, PharmaTheme.PrimaryGreen), LinearGradientMode.Vertical))
        {
            g.FillPath(areaBrush, areaPath);
        }

        using var linePen = new Pen(PharmaTheme.PrimaryGreen, 2f);
        g.DrawLines(linePen, points);

        var hintRect = new Rectangle(0, (ClientSize.Height - 48) / 2, ClientSize.Width, 48);
        TextRenderer.DrawText(
            g,
            "معاينة بصرية — سيتم ربط الرسم بالبيانات لاحقاً",
            PharmaTheme.SmallFont,
            hintRect,
            PharmaTheme.MutedText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
