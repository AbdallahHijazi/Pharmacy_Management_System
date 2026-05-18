using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

/// <summary>Rounded surface panel for dashboard sections (border only; does not clip children).</summary>
public sealed class PharmaCardPanel : Panel
{
    private int _cornerRadius = PharmaTheme.DashboardSectionCornerRadius;

    public PharmaCardPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        BackColor = PharmaTheme.SurfaceContainerLowest;
        BorderStyle = BorderStyle.None;
        Padding = new Padding(16);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = Math.Max(4, value);
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        if (bounds.Width <= 1 || bounds.Height <= 1)
        {
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = RoundedRect(bounds, _cornerRadius);
        using var fill = new SolidBrush(BackColor);
        e.Graphics.FillPath(fill, path);
        using var border = new Pen(PharmaTheme.OutlineVariant);
        e.Graphics.DrawPath(border, path);
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
