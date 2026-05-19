using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

public enum StatCardVisualTone
{
    Normal,
    Warning,
    Danger
}

public sealed class StatCardControl : Control
{
    private string _title = string.Empty;
    private string _value = "0";
    private string? _badge;
    private string _iconGlyph = SegoeMdl2Icons.Payments;
    private StatCardVisualTone _tone = StatCardVisualTone.Normal;

    public StatCardControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        MinimumSize = new Size(150, 132);
        Height = 136;
        Margin = new Padding(8, 6, 8, 6);
        Padding = new Padding(20, 18, 20, 18);
        RightToLeft = RightToLeft.Yes;
        BackColor = PharmaTheme.Background;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        RoundedDrawing.ApplyRoundedRegion(this, PharmaTheme.DashboardStatCornerRadius);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        RoundedDrawing.ApplyRoundedRegion(this, PharmaTheme.DashboardStatCornerRadius);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        using var brush = new SolidBrush(PharmaTheme.Background);
        pevent.Graphics.FillRectangle(brush, ClientRectangle);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardTitle
    {
        get => _title;
        set
        {
            _title = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardValue
    {
        get => _value;
        set
        {
            _value = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? CardBadge
    {
        get => _badge;
        set
        {
            _badge = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string IconText
    {
        get => _iconGlyph;
        set
        {
            _iconGlyph = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public StatCardVisualTone VisualTone
    {
        get => _tone;
        set
        {
            _tone = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var card = ClientRectangle;
        card.Inflate(-2, -2);
        RoundedDrawing.DrawSoftShadow(g, card, PharmaTheme.DashboardStatCornerRadius, PharmaTheme.DashboardCardShadow);
        RoundedDrawing.FillRounded(g, card, PharmaTheme.DashboardStatCornerRadius, PharmaTheme.Surface);

        if (_tone == StatCardVisualTone.Warning)
        {
            RoundedDrawing.DrawRoundedBorder(g, card, PharmaTheme.DashboardStatCornerRadius, Color.FromArgb(55, PharmaTheme.Warning), 1.5f);
        }
        else if (_tone == StatCardVisualTone.Danger)
        {
            RoundedDrawing.DrawRoundedBorder(g, card, PharmaTheme.DashboardStatCornerRadius, Color.FromArgb(55, PharmaTheme.Danger), 1.5f);
        }
        else
        {
            RoundedDrawing.DrawRoundedBorder(g, card, PharmaTheme.DashboardStatCornerRadius, PharmaTheme.BorderSoft);
        }

        var (iconBack, iconFore) = _tone switch
        {
            StatCardVisualTone.Danger => (PharmaTheme.ErrorContainer, PharmaTheme.Danger),
            StatCardVisualTone.Warning => (PharmaTheme.WarningSurface, PharmaTheme.WarningStrong),
            _ => (PharmaTheme.SurfaceContainerLow, PharmaTheme.PrimaryGreen)
        };

        var iconRect = new Rectangle(card.Right - 52, card.Y + 16, 42, 42);
        RoundedDrawing.FillRounded(g, iconRect, 12, iconBack);
        TextRenderer.DrawText(
            g,
            _iconGlyph,
            PharmaTheme.IconFont(16f),
            iconRect,
            iconFore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textW = card.Width - Padding.Horizontal - 56;
        var titleHeight = TextLayoutHelper.LineHeight(PharmaTheme.StatTitleFont, 6);
        var valueHeight = TextLayoutHelper.LineHeight(PharmaTheme.StatValueFont, 10);

        var titleRect = TextLayoutHelper.DeflateVertical(
            new Rectangle(card.X + Padding.Left, card.Y + 16, textW, titleHeight),
            2);
        TextRenderer.DrawText(
            g,
            _title,
            PharmaTheme.StatTitleFont,
            titleRect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

        var valueColor = _tone == StatCardVisualTone.Danger ? PharmaTheme.Danger
            : _tone == StatCardVisualTone.Warning ? PharmaTheme.WarningStrong
            : PharmaTheme.TextDark;
        var valueRect = TextLayoutHelper.DeflateVertical(
            new Rectangle(card.X + Padding.Left, titleRect.Bottom + 6, textW, valueHeight),
            2);
        TextRenderer.DrawText(
            g,
            _value,
            PharmaTheme.StatValueFont,
            valueRect,
            valueColor,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

        if (!string.IsNullOrWhiteSpace(_badge))
        {
            var badgeSize = TextRenderer.MeasureText(_badge, PharmaTheme.StatBadgeFont);
            var badgeRect = new Rectangle(card.Right - badgeSize.Width - 22, card.Bottom - 32, badgeSize.Width + 14, 24);
            RoundedDrawing.FillRounded(g, badgeRect, 12, Color.FromArgb(28, PharmaTheme.Success));
            TextRenderer.DrawText(
                g,
                _badge,
                PharmaTheme.StatBadgeFont,
                TextLayoutHelper.DeflateVertical(badgeRect, 2),
                PharmaTheme.Success,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
