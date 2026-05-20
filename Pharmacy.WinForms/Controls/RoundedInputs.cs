using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls;

/// <summary>Rounded input host for settings fields (distinct from Login RoundedTextInput in Forms).</summary>
internal sealed class RoundedFieldBox : UserControl
{
    private readonly TextBox _box;
    private bool _isConstructing = true;
    private bool _childrenCreated;
    private const int CornerRadius = 12;

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
        BackColor = PharmaTheme.SurfaceContainerHighest;
        Margin = Padding.Empty;

        // Children first — Height/Padding/Size trigger layout; _box must exist before any of that.
        _box = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = PharmaTheme.BodyFont,
            BackColor = PharmaTheme.SurfaceContainerHighest,
            ForeColor = PharmaTheme.TextDark
        };
        _box.HandleCreated += (_, _) => ClipInnerCornersSafely();
        _box.SizeChanged += (_, _) => ClipInnerCornersSafely();
        Controls.Add(_box);
        _childrenCreated = true;

        Padding = new Padding(14, 0, 14, 0);
        MinimumSize = new Size(120, 44);
        Height = 44;

        _isConstructing = false;
        ResumeLayout(performLayout: false);
    }

    private void ClipInnerCornersSafely()
    {
        if (_isConstructing || !_childrenCreated || Disposing || IsDisposed)
        {
            return;
        }

        if (_box.IsDisposed || !_box.IsHandleCreated)
        {
            return;
        }

        var r = _box.ClientRectangle;
        if (r.Width < 4 || r.Height < 4)
        {
            _box.Region = null;
            return;
        }

        r.Inflate(-1, -1);
        using var path = RoundedDrawing.CreateRoundedRect(r, 8);
        var prev = _box.Region;
        _box.Region = new Region(path);
        prev?.Dispose();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TextBox Inner => _box;

    #pragma warning disable CS8765 // Base Text setter nullability varies by target framework.
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

        BackColor = PharmaTheme.SurfaceContainerHighest;
        _box.BackColor = PharmaTheme.SurfaceContainerHighest;
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
        ClipInnerCornersSafely();
    }

    private void LayoutChildrenSafely()
    {
        var innerH = Math.Max(24, ClientSize.Height - 10);
        var top = (ClientSize.Height - innerH) / 2;
        var w = Math.Max(20, ClientSize.Width - Padding.Horizontal);
        _box.SetBounds(Padding.Left, top, w, innerH);
    }

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

        RoundedDrawing.FillRounded(g, r, CornerRadius, PharmaTheme.SurfaceContainerHighest);
        RoundedDrawing.DrawRoundedBorder(g, r, CornerRadius, PharmaTheme.BorderSoft, 1f);
    }
}

/// <summary>Drop-down with rounded chrome on the collapsed control.</summary>
internal sealed class RoundedComboInput : UserControl
{
    private readonly ComboBox _combo;
    private bool _isConstructing = true;
    private bool _childrenCreated;
    private const int CornerRadius = 12;

    public RoundedComboInput()
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
        BackColor = PharmaTheme.SurfaceContainerHighest;

        _combo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Popup,
            Font = PharmaTheme.BodyFont,
            BackColor = PharmaTheme.SurfaceContainerHighest,
            ForeColor = PharmaTheme.TextDark,
            IntegralHeight = false
        };
        _combo.HandleCreated += (_, _) => ClipComboCornersSafely();
        _combo.SizeChanged += (_, _) => ClipComboCornersSafely();
        Controls.Add(_combo);
        _childrenCreated = true;

        Padding = new Padding(10, 0, 6, 0);
        MinimumSize = new Size(120, 44);
        Height = 44;

        _isConstructing = false;
        ResumeLayout(performLayout: false);
    }

    private void ClipComboCornersSafely()
    {
        if (_isConstructing || !_childrenCreated || Disposing || IsDisposed)
        {
            return;
        }

        if (_combo.IsDisposed || !_combo.IsHandleCreated)
        {
            return;
        }

        var r = _combo.ClientRectangle;
        if (r.Width < 4 || r.Height < 4)
        {
            _combo.Region = null;
            return;
        }

        r.Inflate(-1, -1);
        using var path = RoundedDrawing.CreateRoundedRect(r, 8);
        var prev = _combo.Region;
        _combo.Region = new Region(path);
        prev?.Dispose();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ComboBox Combo => _combo;

    public void SyncTheme()
    {
        if (!_childrenCreated || _combo.IsDisposed)
        {
            return;
        }

        BackColor = PharmaTheme.SurfaceContainerHighest;
        _combo.BackColor = PharmaTheme.SurfaceContainerHighest;
        _combo.ForeColor = PharmaTheme.TextDark;
        _combo.Font = PharmaTheme.BodyFont;
        Invalidate();
    }

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

        RoundedDrawing.FillRounded(g, r, CornerRadius, PharmaTheme.SurfaceContainerHighest);
        RoundedDrawing.DrawRoundedBorder(g, r, CornerRadius, PharmaTheme.BorderSoft, 1f);
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);

        if (_isConstructing || !_childrenCreated || Disposing || IsDisposed)
        {
            return;
        }

        if (_combo.IsDisposed)
        {
            return;
        }

        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        const int m = 6;
        _combo.SetBounds(m, m, Math.Max(20, ClientSize.Width - 2 * m), Math.Max(24, ClientSize.Height - 2 * m));
        ClipComboCornersSafely();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_isConstructing || !_childrenCreated || Disposing || IsDisposed)
        {
            return;
        }

        Invalidate();
    }
}

/// <summary>Rounded CTA with primary border (e.g. backup now).</summary>
internal sealed class RoundedPrimaryOutlineButton : Control
{
    private const int Radius = 12;
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
        RoundedDrawing.DrawRoundedBorder(g, b, Radius, PharmaTheme.PrimaryGreen, 1.5f);
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
    private const int Radius = 12;
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

    public void RefreshThemeVisuals()
    {
        Invalidate();
    }

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
    private const int Radius = 12;
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
        var bg = _hover ? PharmaTheme.SurfaceContainerLow : PharmaTheme.SurfaceContainerHighest;
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

/// <summary>Segment chip for font size (small / medium / large).</summary>
internal sealed class FontSizeSegmentButton : Control
{
    private bool _selected;

    public FontSizeSegmentButton(string caption)
    {
        Text = caption;
        Cursor = Cursors.Hand;
        Height = 40;
        MinimumSize = new Size(56, 40);
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
        var back = _selected ? PharmaTheme.PrimaryGreen : PharmaTheme.SurfaceContainerHighest;
        var text = _selected ? Color.White : PharmaTheme.OnSurfaceVariant;
        RoundedDrawing.FillRounded(g, b, 12, back);
        if (_selected)
        {
            RoundedDrawing.DrawRoundedBorder(g, b, 12, PharmaTheme.PrimaryContainer, 1.5f);
        }
        else
        {
            RoundedDrawing.DrawRoundedBorder(g, b, 12, PharmaTheme.BorderSoft);
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
