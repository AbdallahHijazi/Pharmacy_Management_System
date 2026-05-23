using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

public sealed class GradientRoundedButton : Control
{
    private bool _isHover;
    private bool _isPressed;

    public GradientRoundedButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
        Font = PharmaTheme.ArabicFont(10.5f, FontStyle.Bold);
        ForeColor = PharmaTheme.OnPrimary;
        Height = 46;
        MinimumSize = new Size(120, 42);
        Padding = new Padding(18, 4, 18, 4);
        RightToLeft = RightToLeft.Yes;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? IconGlyph { get; set; }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHover = false;
        _isPressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _isPressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _isPressed = false;
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-2, -2);

        if (Enabled)
        {
            RoundedDrawing.DrawSoftShadow(g, bounds, PharmaTheme.DashboardButtonCornerRadius, PharmaTheme.DashboardCardShadow);
        }

        Color top;
        Color bottom;
        Color textColor;
        if (!Enabled)
        {
            top = PharmaTheme.SurfaceContainerHigh;
            bottom = PharmaTheme.SurfaceContainer;
            textColor = PharmaTheme.OnSurfaceVariant;
        }
        else
        {
            top = _isPressed
                ? PharmaTheme.PrimaryContainer
                : _isHover
                    ? PharmaTheme.PrimaryDark
                    : PharmaTheme.Primary;
            bottom = _isPressed ? PharmaTheme.Primary : PharmaTheme.PrimaryContainer;
            textColor = ForeColor;
        }

        using (var path = RoundedDrawing.CreateRoundedRect(bounds, PharmaTheme.DashboardButtonCornerRadius))
        using (var brush = new LinearGradientBrush(bounds, top, bottom, LinearGradientMode.ForwardDiagonal))
        {
            g.FillPath(brush, path);
        }

        var textRect = bounds;
        if (!string.IsNullOrEmpty(IconGlyph))
        {
            var iconRect = new Rectangle(bounds.Right - 34, bounds.Y, 28, bounds.Height);
            TextRenderer.DrawText(
                g,
                IconGlyph,
                PharmaTheme.IconFont(11f),
                iconRect,
                textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            textRect = new Rectangle(bounds.X + 8, bounds.Y, bounds.Width - 40, bounds.Height);
        }

        TextRenderer.DrawText(
            g,
            Text,
            Font,
            TextLayoutHelper.DeflateVertical(textRect, 2),
            textColor,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
    }
}
