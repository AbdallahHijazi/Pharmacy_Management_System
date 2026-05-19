using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

public sealed class QuickActionTileControl : Control
{
    private bool _isHover;

    public event EventHandler? TileClicked;

    public QuickActionTileControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
        Font = PharmaTheme.BodyFont;
        ForeColor = PharmaTheme.TextDark;
        Height = 58;
        Margin = new Padding(0, 0, 0, 10);
        RightToLeft = RightToLeft.Yes;
        BackColor = PharmaTheme.SurfaceContainerLowest;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title { get; set; } = string.Empty;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Description { get; set; } = string.Empty;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string IconGlyph { get; set; } = SegoeMdl2Icons.Add;

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
        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        TileClicked?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        var fill = _isHover ? PharmaTheme.SurfaceContainer : PharmaTheme.SurfaceContainerLow;
        RoundedDrawing.FillRounded(g, bounds, 12, fill);
        RoundedDrawing.DrawRoundedBorder(g, bounds, 12, PharmaTheme.BorderSoft);

        var iconRect = new Rectangle(bounds.Right - 46, bounds.Y + (bounds.Height - 32) / 2, 32, 32);
        RoundedDrawing.FillRounded(g, iconRect, 10, PharmaTheme.SurfaceContainerHighest);
        TextRenderer.DrawText(
            g,
            IconGlyph,
            PharmaTheme.IconFont(14f),
            iconRect,
            PharmaTheme.PrimaryGreen,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textRect = new Rectangle(bounds.X + 12, bounds.Y + 8, bounds.Width - 58, bounds.Height - 16);
        TextRenderer.DrawText(
            g,
            Title + "\n" + Description,
            Font,
            textRect,
            ForeColor,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
    }
}
