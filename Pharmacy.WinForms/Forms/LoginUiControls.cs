using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms;

public enum LoginInputFieldKind
{
    Email,
    Password
}

public sealed class RoundedTextInput : UserControl
{
    private readonly Panel iconPanel;
    private readonly Panel inputHost;
    private readonly Panel revealPanel;
    private readonly TextBox textBox = new();
    private bool focused;
    private LoginInputFieldKind _fieldKind = LoginInputFieldKind.Email;

    public RoundedTextInput()
    {
        DoubleBuffered = true;
        BackColor = PharmaTheme.LoginCardFill;
        Height = PharmaTheme.LoginInputHeight;
        MinimumSize = new Size(200, PharmaTheme.LoginInputHeight);
        Padding = new Padding(0);
        RightToLeft = RightToLeft.No;

        iconPanel = new Panel
        {
            BackColor = PharmaTheme.InputSurface,
            Dock = DockStyle.Left,
            Width = PharmaTheme.LoginIconColumnWidth
        };
        iconPanel.Paint += PaintFieldIcon;

        inputHost = new Panel
        {
            BackColor = PharmaTheme.InputSurface,
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 10, 12, 10)
        };
        inputHost.Controls.Add(textBox);

        textBox.BorderStyle = BorderStyle.None;
        textBox.BackColor = PharmaTheme.InputSurface;
        textBox.ForeColor = PharmaTheme.TextDark;
        textBox.Font = PharmaTheme.BodyFont;
        textBox.Multiline = false;
        textBox.AutoSize = false;
        textBox.RightToLeft = RightToLeft.No;
        textBox.GotFocus += (_, _) => { focused = true; Invalidate(); };
        textBox.LostFocus += (_, _) => { focused = false; Invalidate(); };

        revealPanel = new Panel
        {
            BackColor = PharmaTheme.InputSurface,
            Cursor = Cursors.Hand,
            Dock = DockStyle.Right,
            Visible = false,
            Width = PharmaTheme.LoginRevealColumnWidth
        };
        revealPanel.Paint += PaintRevealIcon;
        revealPanel.Click += (_, _) => ToggleReveal();
        revealPanel.MouseEnter += (_, _) => revealPanel.BackColor = PharmaTheme.LoginRevealHover;
        revealPanel.MouseLeave += (_, _) => revealPanel.BackColor = PharmaTheme.InputSurface;

        Controls.Add(revealPanel);
        Controls.Add(inputHost);
        Controls.Add(iconPanel);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public LoginInputFieldKind FieldKind
    {
        get => _fieldKind;
        set
        {
            _fieldKind = value;
            revealPanel.Visible = value == LoginInputFieldKind.Password;

            iconPanel.Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string InputText
    {
        get => textBox.Text;
        set => textBox.Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string PlaceholderText
    {
        get => textBox.PlaceholderText;
        set => textBox.PlaceholderText = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsPassword
    {
        get => textBox.UseSystemPasswordChar;
        set
        {
            textBox.UseSystemPasswordChar = value;
            revealPanel.Invalidate();
            iconPanel.Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ReadOnly
    {
        get => textBox.ReadOnly;
        set => textBox.ReadOnly = value;
    }

    public void SetRevealInteractionEnabled(bool enabled)
    {
        revealPanel.Enabled = enabled;
        revealPanel.Cursor = enabled ? Cursors.Hand : Cursors.Default;
    }

    private void ToggleReveal()
    {
        if (ReadOnly || _fieldKind != LoginInputFieldKind.Password)
        {
            return;
        }

        textBox.UseSystemPasswordChar = !textBox.UseSystemPasswordChar;
        revealPanel.Invalidate();
    }

    private void PaintFieldIcon(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(PharmaTheme.PrimaryGreen, 1.85F)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };

        if (_fieldKind == LoginInputFieldKind.Password)
        {
            DrawLockIcon(e.Graphics, pen);
        }
        else
        {
            DrawMailIcon(e.Graphics, pen);
        }
    }

    private static void DrawMailIcon(Graphics g, Pen pen)
    {
        var x = 12f;
        var y = 14f;
        var w = 24f;
        var h = 18f;
        g.DrawRectangle(pen, x, y, w, h);
        g.DrawLine(pen, x, y, x + w * 0.5f, y + h * 0.55f);
        g.DrawLine(pen, x + w, y, x + w * 0.5f, y + h * 0.55f);
    }

    private static void DrawLockIcon(Graphics g, Pen pen)
    {
        var shackle = new RectangleF(17, 12, 14, 16);
        g.DrawArc(pen, shackle.X, shackle.Y, shackle.Width, shackle.Height, 180, 180);
        using var bodyPath = RoundedCapsule(new RectangleF(14, 24, 20, 16), 6f);
        g.DrawPath(pen, bodyPath);
    }

    private static GraphicsPath RoundedCapsule(RectangleF r, float radius)
    {
        var path = new GraphicsPath();
        var d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void PaintRevealIcon(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var pen = new Pen(PharmaTheme.PrimaryGreen, 1.75F)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        var cx = revealPanel.ClientSize.Width / 2f;
        var cy = revealPanel.ClientSize.Height / 2f;
        var r = 9f;
        g.DrawEllipse(pen, cx - r, cy - r, r * 2, r * 2);
        using var dot = new SolidBrush(PharmaTheme.PrimaryGreen);
        g.FillEllipse(dot, cx - 2.2f, cy - 2.2f, 4.4f, 4.4f);

        if (textBox.UseSystemPasswordChar)
        {
            return;
        }

        using var hidePen = new Pen(PharmaTheme.MutedText, 1.6F)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        g.DrawLine(hidePen, cx - r + 2f, cy + r - 1f, cx + r - 2f, cy - r + 1f);
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        var innerH = inputHost.ClientSize.Height - inputHost.Padding.Vertical;
        if (innerH <= 0)
        {
            return;
        }

        var textH = Math.Max(TextRenderer.MeasureText("أgy", textBox.Font).Height + 6, textBox.Font.Height + 10);
        textH = Math.Min(textH, innerH);
        textBox.Height = textH;
        textBox.Width = inputHost.ClientSize.Width - inputHost.Padding.Horizontal;
        textBox.Left = inputHost.Padding.Left;
        textBox.Top = inputHost.Padding.Top + (innerH - textH) / 2;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedGeometry.Create(rect, PharmaTheme.LoginInputCornerRadius);
        using var bg = new SolidBrush(PharmaTheme.InputSurface);
        using var border = new Pen(
            focused ? PharmaTheme.AccentTeal : PharmaTheme.LoginCardBorder,
            focused ? 2F : 1.1F)
        {
            LineJoin = LineJoin.Round
        };
        e.Graphics.FillPath(bg, path);
        e.Graphics.DrawPath(border, path);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        using var b = new SolidBrush(BackColor);
        pevent.Graphics.FillRectangle(b, ClientRectangle);
    }
}

public sealed class RoundedButton : Button
{
    public RoundedButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = PharmaTheme.PrimaryGreen;
        Cursor = Cursors.Hand;
        ForeColor = Color.White;
        Font = PharmaTheme.LoginButtonFont;
        TextAlign = ContentAlignment.MiddleCenter;
        UseCompatibleTextRendering = true;
        MinimumSize = new Size(120, PharmaTheme.LoginButtonHeight);
        Height = PharmaTheme.LoginButtonHeight;
        Padding = new Padding(24, 12, 24, 12);
        RightToLeft = RightToLeft.Yes;
        MouseEnter += (_, _) => BackColor = PharmaTheme.PrimaryContainer;
        MouseLeave += (_, _) => BackColor = PharmaTheme.PrimaryGreen;
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        pevent.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = ClientRectangle;
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        using var path = RoundedGeometry.Create(rect, PharmaTheme.LoginButtonCornerRadius);
        using var brush = new SolidBrush(BackColor);
        pevent.Graphics.FillPath(brush, path);
        using var rim = new Pen(Color.FromArgb(40, Color.White), 1f) { LineJoin = LineJoin.Round };
        if (rect.Width > 6 && rect.Height > 6)
        {
            using var inner = RoundedGeometry.Create(Rectangle.Inflate(rect, -1, -1), Math.Max(4, PharmaTheme.LoginButtonCornerRadius - 2));
            pevent.Graphics.DrawPath(rim, inner);
        }

        var textRect = Rectangle.Inflate(rect, -Padding.Horizontal / 2 - 2, -Padding.Vertical / 2 - 2);
        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            textRect,
            ForeColor,
            TextFormatFlags.HorizontalCenter
                | TextFormatFlags.VerticalCenter
                | TextFormatFlags.SingleLine
                | TextFormatFlags.WordEllipsis
                | TextFormatFlags.RightToLeft);
    }
}

internal static class RoundedGeometry
{
    public static GraphicsPath Create(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return path;
        }

        var diameter = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}

/// <summary>Rounded soft panel for validation messages.</summary>
internal sealed class LoginSoftNoticePanel : Panel
{
    private readonly Label _label;

    public LoginSoftNoticePanel()
    {
        DoubleBuffered = true;
        BackColor = PharmaTheme.LoginErrorSurface;
        Padding = new Padding(14, 12, 14, 12);
        Visible = false;

        _label = new Label
        {
            AutoSize = true,
            BackColor = PharmaTheme.LoginErrorSurface,
            Dock = DockStyle.Fill,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.Danger,
            MaximumSize = new Size(PharmaTheme.LoginCardMaxWidth - 72, 0),
            RightToLeft = RightToLeft.Yes,
            TextAlign = ContentAlignment.TopRight,
            UseCompatibleTextRendering = true
        };
        Controls.Add(_label);
        SizeChanged += (_, _) => ApplyRoundedRegion();
        HandleCreated += (_, _) => ApplyRoundedRegion();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Message
    {
        get => _label.Text;
        set
        {
            _label.Text = value ?? string.Empty;
            Visible = !string.IsNullOrWhiteSpace(value);
            if (!Visible)
            {
                Region = null;
            }
            else
            {
                ApplyRoundedRegion();
            }

            Invalidate();
        }
    }

    private void ApplyRoundedRegion()
    {
        if (Width <= 2 || Height <= 2)
        {
            Region = null;
            return;
        }

        using var path = RoundedGeometry.Create(new Rectangle(0, 0, Width - 1, Height - 1), PharmaTheme.LoginNoticeCornerRadius);
        Region = new Region(path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (!Visible || string.IsNullOrWhiteSpace(_label.Text))
        {
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedGeometry.Create(rect, PharmaTheme.LoginNoticeCornerRadius);
        using var border = new Pen(PharmaTheme.LoginErrorBorder, 1.15F) { LineJoin = LineJoin.Round };
        e.Graphics.DrawPath(border, path);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        using var b = new SolidBrush(Visible ? PharmaTheme.LoginErrorSurface : PharmaTheme.LoginCardFill);
        pevent.Graphics.FillRectangle(b, ClientRectangle);
    }
}

internal sealed class LogoMark : Control
{
    public LogoMark()
    {
        DoubleBuffered = true;
        BackColor = PharmaTheme.LoginCardFill;
        MinimumSize = new Size(64, 64);
        Size = new Size(64, 64);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var cross = new SolidBrush(PharmaTheme.PrimaryGreen);
        using var accent = new SolidBrush(PharmaTheme.AccentTeal);
        using var leaf = new SolidBrush(PharmaTheme.PrimaryContainer);
        using var whitePen = new Pen(Color.White, 2f) { LineJoin = LineJoin.Round };

        using (var armV = RoundedGeometry.Create(new Rectangle(26, 8, 12, 48), 5))
        {
            e.Graphics.FillPath(cross, armV);
        }

        using (var armH = RoundedGeometry.Create(new Rectangle(8, 26, 48, 12), 5))
        {
            e.Graphics.FillPath(cross, armH);
        }

        using var leafPath = new GraphicsPath();
        leafPath.AddBezier(31, 55, 60, 50, 60, 16, 32, 24);
        leafPath.AddBezier(32, 24, 40, 34, 39, 46, 31, 55);
        e.Graphics.FillPath(leaf, leafPath);
        e.Graphics.FillEllipse(accent, 44, 18, 10, 10);
        e.Graphics.DrawBezier(whitePen, 34, 52, 41, 41, 48, 31, 56, 19);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        using var b = new SolidBrush(BackColor);
        pevent.Graphics.FillRectangle(b, ClientRectangle);
    }
}
