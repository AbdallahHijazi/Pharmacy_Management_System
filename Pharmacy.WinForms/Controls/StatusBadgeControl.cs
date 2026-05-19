using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

public enum InvoiceStatusBadgeKind
{
    Completed,
    Returned,
    Pending,
    Neutral
}

public sealed class StatusBadgeControl : Control
{
    private InvoiceStatusBadgeKind _kind = InvoiceStatusBadgeKind.Neutral;
    private string _badgeText = "—";

    public StatusBadgeControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        Size = new Size(72, 24);
        Font = PharmaTheme.ArabicFont(9f, FontStyle.Bold);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public InvoiceStatusBadgeKind Kind
    {
        get => _kind;
        set
        {
            _kind = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string BadgeText
    {
        get => _badgeText;
        set
        {
            _badgeText = value;
            Invalidate();
        }
    }

    public static InvoiceStatusBadgeKind FromStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return InvoiceStatusBadgeKind.Neutral;
        }

        return status.Trim().ToUpperInvariant() switch
        {
            "PAID" or "COMPLETED" or "مكتمل" => InvoiceStatusBadgeKind.Completed,
            "RETURNED" or "REFUNDED" or "مرتجع" => InvoiceStatusBadgeKind.Returned,
            "PENDING" or "معلق" => InvoiceStatusBadgeKind.Pending,
            _ => InvoiceStatusBadgeKind.Neutral
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var (back, fore) = _kind switch
        {
            InvoiceStatusBadgeKind.Completed => (PharmaTheme.PrimaryFixed, PharmaTheme.PrimaryGreen),
            InvoiceStatusBadgeKind.Returned => (PharmaTheme.ErrorContainer, PharmaTheme.Danger),
            InvoiceStatusBadgeKind.Pending => (PharmaTheme.WarningSurface, PharmaTheme.WarningStrong),
            _ => (PharmaTheme.SurfaceContainerLow, PharmaTheme.OnSurfaceVariant)
        };

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        using var path = RoundedDrawing.CreateRoundedRect(bounds, bounds.Height / 2);
        using var fill = new SolidBrush(back);
        g.FillPath(fill, path);
        TextRenderer.DrawText(
            g,
            _badgeText,
            Font,
            bounds,
            fore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
