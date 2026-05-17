using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pharmacy.WinForms.Forms;

public sealed class AmbientFormSurface : Panel
{
    public AmbientFormSurface()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var bg = new LinearGradientBrush(
            ClientRectangle,
            Color.FromArgb(249, 252, 250),
            Color.FromArgb(235, 245, 240),
            0F);
        e.Graphics.FillRectangle(bg, ClientRectangle);
    }
}

public sealed class DnaPanel : Panel
{
    public DnaPanel()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var bg = new LinearGradientBrush(
            ClientRectangle,
            Color.FromArgb(2, 48, 34),
            Color.FromArgb(0, 88, 58),
            35F);
        e.Graphics.FillRectangle(bg, ClientRectangle);

        DrawSubtleGrid(e.Graphics);
        DrawGlow(e.Graphics, new PointF(Width * .52F, Height * .5F), Math.Max(220, Math.Min(Width, Height) / 2), Color.FromArgb(105, 63, 233, 177));
        DrawGlow(e.Graphics, new PointF(Width * .76F, Height * .22F), Math.Max(120, Math.Min(Width, Height) / 4), Color.FromArgb(65, 150, 255, 215));
        DrawDnaHelix(e.Graphics);
    }

    private static void DrawGlow(Graphics g, PointF center, int radius, Color color)
    {
        using var path = new GraphicsPath();
        path.AddEllipse(center.X - radius, center.Y - radius, radius * 2, radius * 2);
        using var brush = new PathGradientBrush(path)
        {
            CenterColor = color,
            SurroundColors = new[] { Color.Transparent }
        };
        g.FillPath(brush, path);
    }

    private void DrawDnaHelix(Graphics g)
    {
        var centerX = Width * .5F;
        var top = Height * .15F;
        var helixHeight = Height * .72F;
        var amplitude = Math.Max(54F, Width * .11F);
        const int steps = 52;
        var left = new PointF[steps];
        var right = new PointF[steps];

        for (var i = 0; i < steps; i++)
        {
            var t = i / (float)(steps - 1);
            var y = top + helixHeight * t;
            var wave = (float)Math.Sin(t * Math.PI * 5.2F);
            left[i] = new PointF(centerX - amplitude * wave, y);
            right[i] = new PointF(centerX + amplitude * wave, y);
        }

        using var glowPen = new Pen(Color.FromArgb(90, 139, 255, 213), 9F)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        using var strandPen = new Pen(Color.FromArgb(230, 205, 255, 236), 3.2F)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        using var rungPen = new Pen(Color.FromArgb(125, 175, 255, 225), 1.8F);
        using var nodeBrush = new SolidBrush(Color.FromArgb(232, 220, 255, 239));
        using var nodeGlow = new SolidBrush(Color.FromArgb(80, 99, 241, 185));

        g.DrawCurve(glowPen, left);
        g.DrawCurve(glowPen, right);
        g.DrawCurve(strandPen, left);
        g.DrawCurve(strandPen, right);

        for (var i = 2; i < steps - 2; i += 4)
        {
            g.DrawLine(rungPen, left[i], right[i]);
            g.FillEllipse(nodeGlow, left[i].X - 9, left[i].Y - 9, 18, 18);
            g.FillEllipse(nodeGlow, right[i].X - 9, right[i].Y - 9, 18, 18);
            g.FillEllipse(nodeBrush, left[i].X - 4, left[i].Y - 4, 8, 8);
            g.FillEllipse(nodeBrush, right[i].X - 4, right[i].Y - 4, 8, 8);
        }
    }

    private void DrawSubtleGrid(Graphics g)
    {
        using var pen = new Pen(Color.FromArgb(18, 222, 255, 238), 1F);
        for (var x = 0; x < Width; x += 42)
        {
            g.DrawLine(pen, x, 0, x, Height);
        }

        for (var y = 0; y < Height; y += 42)
        {
            g.DrawLine(pen, 0, y, Width, y);
        }
    }
}

public sealed class RoundedTextInput : UserControl
{
    private readonly TextBox textBox = new();
    private bool focused;

    public RoundedTextInput()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Height = 48;
        Padding = new Padding(48, 13, 18, 8);

        textBox.BorderStyle = BorderStyle.None;
        textBox.BackColor = Color.FromArgb(246, 249, 247);
        textBox.ForeColor = Color.FromArgb(20, 48, 37);
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
        using var bg = new SolidBrush(Color.FromArgb(246, 249, 247));
        using var border = new Pen(
            focused ? Color.FromArgb(12, 119, 78) : Color.FromArgb(214, 226, 220),
            focused ? 1.8F : 1F);
        e.Graphics.FillPath(bg, path);
        e.Graphics.DrawPath(border, path);

        using var iconPen = new Pen(Color.FromArgb(33, 73, 56), 1.8F);
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
}

public sealed class RoundedButton : Button
{
    private readonly Color normalColor = Color.FromArgb(2, 104, 67);
    private readonly Color hoverColor = Color.FromArgb(7, 135, 88);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int BorderRadius { get; set; } = 18;

    public RoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = normalColor;
        Cursor = Cursors.Hand;
        ForeColor = Color.White;
        MouseEnter += (_, _) => BackColor = hoverColor;
        MouseLeave += (_, _) => BackColor = normalColor;
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
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        using var green = new SolidBrush(Color.FromArgb(2, 104, 67));
        using var lightGreen = new SolidBrush(Color.FromArgb(34, 158, 101));
        using var whitePen = new Pen(Color.White, 2.2F);

        e.Graphics.FillRectangle(green, 26, 6, 12, 52);
        e.Graphics.FillRectangle(green, 6, 26, 52, 12);

        using var leaf = new GraphicsPath();
        leaf.AddBezier(31, 55, 60, 50, 60, 16, 32, 24);
        leaf.AddBezier(32, 24, 40, 34, 39, 46, 31, 55);
        e.Graphics.FillPath(lightGreen, leaf);
        e.Graphics.DrawBezier(whitePen, 34, 52, 41, 41, 48, 31, 56, 19);
    }
}
