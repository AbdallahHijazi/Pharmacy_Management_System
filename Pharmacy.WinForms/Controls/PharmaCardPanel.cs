using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

/// <summary>Rounded surface panel for dashboard sections (fills and clips to a rounded rectangle).</summary>
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
        Paint += OnCardPaint;
        UpdateRegion();
    }

    private void OnCardPaint(object? sender, PaintEventArgs e)
    {
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        if (bounds.Width <= 1 || bounds.Height <= 1)
        {
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = RoundedRect(bounds, _cornerRadius);
        using var border = new Pen(PharmaTheme.OutlineVariant);
        e.Graphics.DrawPath(border, path);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = Math.Max(4, value);
            UpdateRegion();
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateRegion();
    }

    private void UpdateRegion()
    {
        if (Width <= 2 || Height <= 2)
        {
            return;
        }

        using var path = RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), _cornerRadius);
        var newRegion = new Region(path);
        var old = Region;
        Region = newRegion;
        old?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Region?.Dispose();
            Region = null;
        }

        base.Dispose(disposing);
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
