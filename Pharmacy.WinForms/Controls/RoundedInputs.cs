using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

/// <summary>Rounded input host for settings fields (distinct from Login RoundedTextInput in Forms).</summary>
internal class RoundedFieldBox : UserControl
{
    private TextBox _box = null!;
    private bool _isConstructing = true;
    private bool _childrenCreated;
    private bool _focused;
    protected int CornerRadius { get; set; } = PharmaTheme.SettingsFieldCornerRadius;
    protected virtual int VerticalPad => 5;

    public RoundedFieldBox()
    {
        SuspendLayout();
        _isConstructing = true;
        _childrenCreated = false;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;
        BackColor = PharmaTheme.SurfaceContainerHigh;
        Margin = Padding.Empty;

        _box = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = PharmaTheme.BodyFont,
            BackColor = PharmaTheme.SurfaceContainerHigh,
            ForeColor = PharmaTheme.TextDark
        };
        _box.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        _box.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        Controls.Add(_box);
        _childrenCreated = true;

        Padding = new Padding(14, 0, 14, 0);
        MinimumSize = new Size(120, 44);

        _isConstructing = false;
        ResumeLayout(performLayout: false);
        Height = 44;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TextBox Inner => _box;

    #pragma warning disable CS8765
    public override string Text
    {
        get => _box.Text;
        set => _box.Text = value ?? string.Empty;
    }
#pragma warning restore CS8765

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public HorizontalAlignment TextAlign
    {
        get => _box.TextAlign;
        set => _box.TextAlign = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ReadOnly
    {
        get => _box.ReadOnly;
        set => _box.ReadOnly = value;
    }

    public void ApplyThemeVisuals()
    {
        if (!_childrenCreated || _box.IsDisposed)
        {
            return;
        }

        var fill = PharmaTheme.SurfaceContainerHigh;
        BackColor = fill;
        _box.BackColor = fill;
        _box.ForeColor = PharmaTheme.TextDark;
        _box.Font = PharmaTheme.BodyFont;
        Invalidate();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);

        if (_isConstructing || !_childrenCreated || Disposing || IsDisposed)
        {
            return;
        }

        if (_box.IsDisposed)
        {
            return;
        }

        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        LayoutChildrenSafely();
    }

    protected virtual void LayoutChildrenSafely()
    {
        var innerH = Math.Max(22, ClientSize.Height - VerticalPad * 2);
        var top = (ClientSize.Height - innerH) / 2;
        var w = Math.Max(20, ClientSize.Width - Padding.Horizontal);
        _box.SetBounds(Padding.Left, top, w, innerH);
    }

    protected Color FieldFillColor => PharmaTheme.SurfaceContainerHigh;

    protected Color BorderColor => _focused ? PharmaTheme.Primary : PharmaTheme.BorderSoft;

    protected float BorderWidth => _focused ? 1.75f : 1f;

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_childrenCreated || Disposing || IsDisposed)
        {
            return;
        }

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = ClientRectangle;
        r.Inflate(-1, -1);
        if (r.Width < 4 || r.Height < 4)
        {
            return;
        }

        RoundedDrawing.FillRounded(g, r, CornerRadius, FieldFillColor);
        RoundedDrawing.DrawRoundedBorder(g, r, CornerRadius, BorderColor, BorderWidth);
    }
}

/// <summary>Compact rounded numeric field for alert rows.</summary>
internal sealed class RoundedNumberField : RoundedFieldBox
{
    public RoundedNumberField()
    {
        CornerRadius = PharmaTheme.SettingsFieldCornerRadius;
        Padding = new Padding(8, 0, 8, 0);
        MinimumSize = new Size(48, 40);
        Height = 40;
    }
}

/// <summary>Rounded segment chip (font size, backup schedule, etc.).</summary>
internal class SegmentChipButton : Control
{
    private bool _selected;

    public SegmentChipButton(string caption)
    {
        Text = caption;
        Cursor = Cursors.Hand;
        Height = 40;
        MinimumSize = new Size(52, 40);
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsSelected
    {
        get => _selected;
        set
        {
            _selected = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var b = ClientRectangle;
        b.Inflate(-1, -1);
        var back = _selected ? PharmaTheme.PrimaryGreen : PharmaTheme.SurfaceContainerHigh;
        var text = _selected ? PharmaTheme.OnPrimary : PharmaTheme.OnSurfaceVariant;
        RoundedDrawing.FillRounded(g, b, PharmaTheme.SettingsChipCornerRadius, back);
        if (_selected)
        {
            RoundedDrawing.DrawRoundedBorder(g, b, PharmaTheme.SettingsChipCornerRadius, PharmaTheme.PrimaryContainer, 1.75f);
        }
        else
        {
            RoundedDrawing.DrawRoundedBorder(g, b, PharmaTheme.SettingsChipCornerRadius, PharmaTheme.BorderSoft);
        }

        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.SmallFont,
            b,
            text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

/// <summary>Rounded CTA with primary border (e.g. backup now).</summary>
internal sealed class RoundedPrimaryOutlineButton : Control
{
    private const int Radius = 14;
    private bool _hover;

    public RoundedPrimaryOutlineButton()
    {
        Cursor = Cursors.Hand;
        Height = 44;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public void RefreshThemeVisuals() => Invalidate();

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var b = ClientRectangle;
        b.Inflate(-1, -1);
        var fill = _hover ? PharmaTheme.SurfaceContainerLow : PharmaTheme.SurfaceContainer;
        RoundedDrawing.FillRounded(g, b, Radius, fill);
        RoundedDrawing.DrawRoundedBorder(g, b, Radius, PharmaTheme.PrimaryGreen, 1.75f);
        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.ArabicFont(10f, FontStyle.Bold),
            b,
            PharmaTheme.PrimaryGreen,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }
}

/// <summary>Secondary rounded action (outline / neutral).</summary>
internal sealed class RoundedNeutralButton : Control
{
    private const int Radius = 14;
    private bool _pressed;

    public RoundedNeutralButton()
    {
        Cursor = Cursors.Hand;
        Size = new Size(110, 44);
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public void RefreshThemeVisuals() => Invalidate();

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _pressed = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _pressed = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var b = ClientRectangle;
        b.Inflate(-1, -1);
        var fill = _pressed ? PharmaTheme.SurfaceContainerLow : PharmaTheme.SurfaceContainer;
        RoundedDrawing.FillRounded(g, b, Radius, fill);
        RoundedDrawing.DrawRoundedBorder(g, b, Radius, PharmaTheme.BorderSoft);
        TextRenderer.DrawText(
            g,
            Text,
            PharmaTheme.BodyFont,
            b,
            PharmaTheme.TextDark,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }
}

/// <summary>Rounded icon square (Segoe MDL2 text).</summary>
internal sealed class RoundedIconButton : Control
{
    private const int Radius = 14;
    private bool _hover;

    public RoundedIconButton(string glyphMdL2)
    {
        Glyph = glyphMdL2;
        Cursor = Cursors.Hand;
        Size = new Size(48, 44);
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick,
            true);
    }

    public string Glyph { get; }

    public void RefreshThemeVisuals() => Invalidate();

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var b = ClientRectangle;
        b.Inflate(-1, -1);
        var bg = _hover ? PharmaTheme.SurfaceContainerLow : PharmaTheme.SurfaceContainerHigh;
        RoundedDrawing.FillRounded(g, b, Radius, bg);
        RoundedDrawing.DrawRoundedBorder(g, b, Radius, PharmaTheme.BorderSoft);
        TextRenderer.DrawText(
            g,
            Glyph,
            PharmaTheme.IconFont(14f),
            b,
            PharmaTheme.PrimaryGreen,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

/// <summary>Font size segment — alias of <see cref="SegmentChipButton"/>.</summary>
internal sealed class FontSizeSegmentButton : SegmentChipButton
{
    public FontSizeSegmentButton(string caption) : base(caption)
    {
    }
}
