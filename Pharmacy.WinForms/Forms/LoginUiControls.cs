using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms;

public sealed class RoundedTextInput : UserControl
{
    private readonly TextBox textBox = new();
    private bool focused;

    public RoundedTextInput()
    {
        DoubleBuffered = true;
        BackColor = PharmaTheme.LoginCardFill;
        Height = 48;
        Padding = new Padding(48, 13, 18, 8);

        textBox.BorderStyle = BorderStyle.None;
        textBox.BackColor = PharmaTheme.InputSurface;
        textBox.ForeColor = PharmaTheme.TextDark;
        textBox.Font = new Font("Segoe UI", 10.5F);
        textBox.Dock = DockStyle.Fill;
        textBox.GotFocus += (_, _) => { focused = true; Invalidate(); };
        textBox.LostFocus += (_, _) => { focused = false; Invalidate(); };
        Controls.Add(textBox);
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
        set => textBox.UseSystemPasswordChar = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ReadOnly
    {
        get => textBox.ReadOnly;
        set => textBox.ReadOnly = value;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = RoundedGeometry.Create(rect, 18);
        using var bg = new SolidBrush(PharmaTheme.InputSurface);
        using var border = new Pen(
            focused ? PharmaTheme.AccentTeal : PharmaTheme.LoginCardBorder,
            focused ? 1.8F : 1F);
        e.Graphics.FillPath(bg, path);
        e.Graphics.DrawPath(border, path);

        using var iconPen = new Pen(PharmaTheme.PrimaryGreen, 1.8F);
        if (IsPassword)
        {
            e.Graphics.DrawRectangle(iconPen, 18, 23, 12, 10);
            e.Graphics.DrawArc(iconPen, 19, 14, 10, 16, 190, 160);
        }
        else
        {
            e.Graphics.DrawEllipse(iconPen, 18, 14, 12, 12);
            e.Graphics.DrawArc(iconPen, 13, 27, 23, 14, 200, 140);
        }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        using var b = new SolidBrush(BackColor);
        pevent.Graphics.FillRectangle(b, ClientRectangle);
    }
}

public sealed class RoundedButton : Button
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int BorderRadius { get; set; } = 18;

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = PharmaTheme.PrimaryGreen;
        Cursor = Cursors.Hand;
        ForeColor = Color.White;
        MouseEnter += (_, _) => BackColor = PharmaTheme.PrimaryContainer;
        MouseLeave += (_, _) => BackColor = PharmaTheme.PrimaryGreen;
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = RoundedGeometry.Create(ClientRectangle, BorderRadius);
        Region = new Region(path);
        base.OnPaint(pevent);
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

internal sealed class LogoMark : Control
{
    public LogoMark()
    {
        DoubleBuffered = true;
        BackColor = PharmaTheme.LoginCardFill;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var cross = new SolidBrush(PharmaTheme.PrimaryGreen);
        using var accent = new SolidBrush(PharmaTheme.AccentTeal);
        using var leaf = new SolidBrush(PharmaTheme.PrimaryContainer);
        using var whitePen = new Pen(Color.White, 2.2F);

        e.Graphics.FillRectangle(cross, 26, 6, 12, 52);
        e.Graphics.FillRectangle(cross, 6, 26, 52, 12);

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
