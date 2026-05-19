using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

public sealed class PharmaCardPanel : Panel
{
    private int _cornerRadius = PharmaTheme.DashboardSectionCornerRadius;

    public PharmaCardPanel()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.UserPaint,
            true);
        DoubleBuffered = true;
        BackColor = PharmaTheme.Surface;
        BorderStyle = BorderStyle.None;
        Padding = new Padding(22, 20, 22, 20);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = Math.Max(8, value);
            ApplyRoundedRegion();
            Invalidate();
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyRoundedRegion();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyRoundedRegion();
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var parentBack = Parent?.BackColor ?? PharmaTheme.Background;
        using var outer = new SolidBrush(parentBack);
        e.Graphics.FillRectangle(outer, ClientRectangle);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        var bounds = ClientRectangle;
        bounds.Inflate(-2, -2);
        if (bounds.Width <= 2 || bounds.Height <= 2)
        {
            return;
        }

        RoundedDrawing.DrawSoftShadow(g, bounds, _cornerRadius, PharmaTheme.DashboardCardShadow);
        RoundedDrawing.FillRounded(g, bounds, _cornerRadius, BackColor);
        RoundedDrawing.DrawRoundedBorder(g, bounds, _cornerRadius, PharmaTheme.BorderSoft);
    }

    private void ApplyRoundedRegion()
    {
        RoundedDrawing.ApplyRoundedRegion(this, _cornerRadius);
    }
}
