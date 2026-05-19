using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pharmacy.WinForms.Ui;

internal static class RoundedDrawing
{
    public static GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return path;
        }

        var d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void DrawSoftShadow(Graphics g, Rectangle bounds, int radius, Color shadowTint, int offsetY = 2)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var shadowRect = new Rectangle(bounds.X, bounds.Y + offsetY, bounds.Width, bounds.Height);
        using var path = CreateRoundedRect(shadowRect, radius);
        using var brush = new SolidBrush(shadowTint);
        g.FillPath(brush, path);
    }

    public static void FillRounded(Graphics g, Rectangle bounds, int radius, Color fill)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = CreateRoundedRect(bounds, radius);
        using var brush = new SolidBrush(fill);
        g.FillPath(brush, path);
    }

    public static void DrawRoundedBorder(Graphics g, Rectangle bounds, int radius, Color border, float width = 1f)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        bounds.Inflate(-1, -1);
        using var path = CreateRoundedRect(bounds, radius);
        using var pen = new Pen(border, width);
        g.DrawPath(pen, path);
    }

    public static void ApplyRoundedRegion(Control control, int radius)
    {
        if (control.IsDisposed)
        {
            return;
        }

        var bounds = control.ClientRectangle;
        bounds.Inflate(-1, -1);
        if (bounds.Width <= 2 || bounds.Height <= 2)
        {
            control.Region = null;
            return;
        }

        using var path = CreateRoundedRect(bounds, radius);
        var previous = control.Region;
        control.Region = new Region(path);
        previous?.Dispose();
    }
}
