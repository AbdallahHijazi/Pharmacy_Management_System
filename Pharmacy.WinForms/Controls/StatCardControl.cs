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

/// <summary>KPI card using labels (stable layout, no clipped custom text).</summary>
public sealed class StatCardControl : UserControl
{
    private readonly Label _titleLabel;
    private readonly Label _valueLabel;
    private readonly Label _iconLabel;
    private StatCardVisualTone _tone = StatCardVisualTone.Normal;

    public StatCardControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        BackColor = PharmaTheme.SurfaceContainerLowest;
        MinimumSize = new Size(140, 112);
        Height = 112;
        Margin = new Padding(6, 4, 6, 4);
        Padding = new Padding(14, 12, 14, 12);
        RightToLeft = RightToLeft.Yes;

        _iconLabel = new Label
        {
            AutoSize = false,
            BackColor = PharmaTheme.SurfaceContainerLow,
            Font = PharmaTheme.SectionFont,
            ForeColor = PharmaTheme.PrimaryGreen,
            Size = new Size(40, 40),
            Text = "●",
            TextAlign = ContentAlignment.MiddleCenter
        };

        _titleLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.SmallFont,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Height = 22,
            Text = string.Empty,
            TextAlign = ContentAlignment.MiddleRight
        };

        _valueLabel = new Label
        {
            AutoSize = false,
            Font = PharmaTheme.StatValueFont,
            ForeColor = PharmaTheme.TextDark,
            Height = 40,
            Text = "0",
            TextAlign = ContentAlignment.MiddleRight
        };

        Controls.Add(_valueLabel);
        Controls.Add(_titleLabel);
        Controls.Add(_iconLabel);

        Resize += (_, _) => LayoutCard();
        LayoutCard();
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardTitle
    {
        get => _titleLabel.Text;
        set => _titleLabel.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardValue
    {
        get => _valueLabel.Text;
        set => _valueLabel.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string IconText
    {
        get => _iconLabel.Text;
        set => _iconLabel.Text = value;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public StatCardVisualTone VisualTone
    {
        get => _tone;
        set
        {
            _tone = value;
            ApplyTone();
            Invalidate();
        }
    }

    private void ApplyTone()
    {
        switch (_tone)
        {
            case StatCardVisualTone.Danger:
                _iconLabel.BackColor = PharmaTheme.ErrorContainer;
                _iconLabel.ForeColor = PharmaTheme.Danger;
                _valueLabel.ForeColor = PharmaTheme.Danger;
                break;
            case StatCardVisualTone.Warning:
                _iconLabel.BackColor = PharmaTheme.WarningSurface;
                _iconLabel.ForeColor = PharmaTheme.WarningStrong;
                _valueLabel.ForeColor = PharmaTheme.WarningStrong;
                break;
            default:
                _iconLabel.BackColor = PharmaTheme.SurfaceContainerLow;
                _iconLabel.ForeColor = PharmaTheme.PrimaryGreen;
                _valueLabel.ForeColor = PharmaTheme.TextDark;
                break;
        }
    }

    private void LayoutCard()
    {
        var pad = Padding;
        var innerW = Math.Max(1, ClientSize.Width - pad.Horizontal);
        var innerH = Math.Max(1, ClientSize.Height - pad.Vertical);

        _iconLabel.SetBounds(pad.Left + innerW - _iconLabel.Width, pad.Top + 4, _iconLabel.Width, _iconLabel.Height);

        var textW = Math.Max(60, innerW - _iconLabel.Width - 10);
        _titleLabel.SetBounds(pad.Left, pad.Top + 2, textW, _titleLabel.Height);
        _valueLabel.SetBounds(pad.Left, _titleLabel.Bottom + 2, textW, Math.Max(36, innerH - _titleLabel.Bottom - 6));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        if (bounds.Width <= 2 || bounds.Height <= 2)
        {
            return;
        }

        using var path = RoundedRect(bounds, PharmaTheme.DashboardStatCornerRadius);
        using (var fill = new SolidBrush(BackColor))
        {
            e.Graphics.FillPath(fill, path);
        }

        var borderColor = _tone == StatCardVisualTone.Normal
            ? PharmaTheme.OutlineVariant
            : _tone == StatCardVisualTone.Danger
                ? Color.FromArgb(80, PharmaTheme.Danger)
                : Color.FromArgb(80, PharmaTheme.Warning);
        using var border = new Pen(borderColor);
        e.Graphics.DrawPath(border, path);
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
