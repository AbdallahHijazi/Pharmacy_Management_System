using System.ComponentModel;
using System.Drawing.Drawing2D;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Controls.Users;

internal enum UsrActionButtonTone
{
    Primary,
    Danger
}

internal static class UsrColumnLayout
{
    internal readonly record struct Layout(
        Rectangle User,
        Rectangle Role,
        Rectangle LastLogin,
        Rectangle Status,
        Rectangle Actions,
        bool Compact);

    internal static Layout Calculate(Rectangle bounds, bool compact)
    {
        const int pad = 12;
        var actionsW = Math.Max(220, (int)(bounds.Width * 0.22));
        var statusW = Math.Max(88, (int)(bounds.Width * 0.11));
        var lastLoginW = compact ? 0 : Math.Max(120, (int)(bounds.Width * 0.16));
        var roleW = Math.Max(96, (int)(bounds.Width * 0.12));
        var fixedW = pad * 2 + actionsW + statusW + lastLoginW + roleW;
        var userW = Math.Max(180, bounds.Width - fixedW);

        var x = bounds.X + pad;
        var user = new Rectangle(x, bounds.Y, userW, bounds.Height);
        x += userW;
        var role = new Rectangle(x, bounds.Y, roleW, bounds.Height);
        x += roleW;
        var lastLogin = compact ? Rectangle.Empty : new Rectangle(x, bounds.Y, lastLoginW, bounds.Height);
        if (!compact)
        {
            x += lastLoginW;
        }

        var status = new Rectangle(x, bounds.Y, statusW, bounds.Height);
        x += statusW;
        var actions = new Rectangle(x, bounds.Y, actionsW, bounds.Height);

        return new Layout(user, role, lastLogin, status, actions, compact);
    }
}

internal class UsrRoundedPanel : Panel
{
    private readonly int _radius;

    public UsrRoundedPanel(int radius = PharmaTheme.UsersRowCornerRadius)
    {
        _radius = radius;
        DoubleBuffered = true;
        BackColor = FillColor;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color FillColor { get; set; } = PharmaTheme.Surface;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BorderColor { get; set; } = PharmaTheme.BorderSoft;

    public void ApplyThemeVisuals()
    {
        BackColor = FillColor;
        Invalidate(true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? PharmaTheme.Background);

        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        if (bounds.Width <= 4 || bounds.Height <= 4)
        {
            return;
        }

        RoundedDrawing.FillRounded(g, bounds, _radius, FillColor);
        RoundedDrawing.DrawRoundedBorder(g, bounds, _radius, BorderColor, 1f);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);
}

internal sealed class UsrSearchBox : UserControl
{
    private TextBox? _box;
    private bool _focused;

    public UsrSearchBox()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;
        Height = 48;
        MinimumSize = new Size(220, 48);
        Padding = new Padding(44, 0, 14, 0);
        RightToLeft = RightToLeft.Yes;

        _box = new TextBox
        {
            BorderStyle = BorderStyle.None,
            Font = PharmaTheme.ArabicFont(11f),
            BackColor = PharmaTheme.SurfaceContainerHigh,
            ForeColor = PharmaTheme.TextDark,
            RightToLeft = RightToLeft.Yes,
            TextAlign = HorizontalAlignment.Right,
            PlaceholderText = "البحث عن مستخدم..."
        };
        _box.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        _box.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        _box.TextChanged += (_, _) => SearchTextChanged?.Invoke(this, EventArgs.Empty);
        Controls.Add(_box);
        _box.Dock = DockStyle.Fill;
    }

    public event EventHandler? SearchTextChanged;

#pragma warning disable CS8765, CS8764
    public override string? Text
    {
        get => _box?.Text ?? string.Empty;
        set
        {
            if (_box is not null)
            {
                _box.Text = value ?? string.Empty;
            }
        }
    }
#pragma warning restore CS8765, CS8764

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string PlaceholderText
    {
        get => _box?.PlaceholderText ?? string.Empty;
        set
        {
            if (_box is not null)
            {
                _box.PlaceholderText = value;
            }
        }
    }

    public void ApplyThemeVisuals()
    {
        if (_box is not null)
        {
            _box.BackColor = PharmaTheme.SurfaceContainerHigh;
            _box.ForeColor = PharmaTheme.TextDark;
            _box.Font = PharmaTheme.ArabicFont(11f);
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = ClientRectangle;
        r.Inflate(-1, -1);
        RoundedDrawing.FillRounded(g, r, PharmaTheme.UsersSearchCornerRadius, PharmaTheme.SurfaceContainerHigh);
        RoundedDrawing.DrawRoundedBorder(
            g,
            r,
            PharmaTheme.UsersSearchCornerRadius,
            _focused ? PharmaTheme.Primary : PharmaTheme.BorderSoft,
            _focused ? 1.75f : 1f);

        TextRenderer.DrawText(
            g,
            SegoeMdl2Icons.Search,
            PharmaTheme.IconFont(12f),
            new Rectangle(14, 0, 28, Height),
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);
}

internal sealed class UsrStatCard : Control
{
    private string _title = string.Empty;
    private string _value = "0";
    private string _subtitle = string.Empty;
    private string _iconGlyph = SegoeMdl2Icons.Users;
    private bool _dangerTone;

    public UsrStatCard()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;
        MinimumSize = new Size(200, 128);
        Height = 128;
        RightToLeft = RightToLeft.Yes;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardTitle
    {
        get => _title;
        set { _title = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CardValue
    {
        get => _value;
        set { _value = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Subtitle
    {
        get => _subtitle;
        set { _subtitle = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string IconGlyph
    {
        get => _iconGlyph;
        set { _iconGlyph = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DangerTone
    {
        get => _dangerTone;
        set { _dangerTone = value; Invalidate(); }
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        RoundedDrawing.FillRounded(g, bounds, PharmaTheme.UsersStatCornerRadius, PharmaTheme.Surface);
        RoundedDrawing.DrawRoundedBorder(g, bounds, PharmaTheme.UsersStatCornerRadius, PharmaTheme.BorderSoft, 1f);

        const int innerPad = 22;
        var iconSize = 44;
        var iconRect = new Rectangle(bounds.Right - innerPad - iconSize, bounds.Y + innerPad, iconSize, iconSize);
        RoundedDrawing.FillRounded(g, iconRect, 12, PharmaTheme.WithAlpha(PharmaTheme.PrimaryContainer, 120));
        TextRenderer.DrawText(
            g,
            _iconGlyph,
            PharmaTheme.IconFont(16f),
            iconRect,
            _dangerTone ? PharmaTheme.Danger : PharmaTheme.Primary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textX = bounds.X + innerPad;
        var textW = Math.Max(80, iconRect.X - textX - 12);
        var titleRect = new Rectangle(textX, bounds.Y + innerPad, textW, 36);
        TextRenderer.DrawText(
            g,
            _title,
            PharmaTheme.StatTitleFont,
            titleRect,
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

        var valueRect = new Rectangle(textX, bounds.Y + innerPad + 40, textW, 32);
        var valueColor = _dangerTone ? PharmaTheme.Danger : PharmaTheme.TextDark;
        TextRenderer.DrawText(
            g,
            _value,
            PharmaTheme.NumberFont(20f, FontStyle.Bold),
            valueRect,
            valueColor,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        if (!string.IsNullOrWhiteSpace(_subtitle))
        {
            var subtitleRect = new Rectangle(textX, bounds.Y + innerPad + 72, textW, 20);
            TextRenderer.DrawText(
                g,
                _subtitle,
                PharmaTheme.SmallFont,
                subtitleRect,
                PharmaTheme.OnSurfaceVariant,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}

internal sealed class UsrTableHeader : Control
{
    public UsrTableHeader()
    {
        Height = 48;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;
    }

    public void ApplyThemeVisuals() => Invalidate();

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        RoundedDrawing.FillRounded(g, bounds, 12, PharmaTheme.SurfaceAlt);

        var compact = bounds.Width < 980;
        var columns = UsrColumnLayout.Calculate(bounds, compact);
        DrawHeader(g, columns.User, "المستخدم");
        DrawHeader(g, columns.Role, "الدور");
        if (!compact)
        {
            DrawHeader(g, columns.LastLogin, "آخر تسجيل دخول");
        }

        DrawHeader(g, columns.Status, "الحالة");
        DrawHeader(g, columns.Actions, "إجراءات");
    }

    private static void DrawHeader(Graphics g, Rectangle rect, string text)
    {
        if (rect.Width <= 0)
        {
            return;
        }

        TextRenderer.DrawText(
            g,
            text,
            PharmaTheme.TableHeaderFont,
            Rectangle.Inflate(rect, -8, 0),
            PharmaTheme.MutedText,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

internal sealed class UsrActionButton : Control
{
    private bool _hover;
    private bool _pressed;
    private UsrActionButtonTone _tone = UsrActionButtonTone.Primary;

    public UsrActionButton()
    {
        Height = 28;
        Cursor = Cursors.Hand;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.StandardClick
                | ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;
        Font = PharmaTheme.ArabicFont(9.25f, FontStyle.Bold);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public UsrActionButtonTone Tone
    {
        get => _tone;
        set { _tone = value; Invalidate(); }
    }

    public void ApplyThemeVisuals()
    {
        UpdateAutoWidth();
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        UpdateAutoWidth();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        UpdateAutoWidth();
    }

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
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _pressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_pressed && e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location))
        {
            OnClick(EventArgs.Empty);
        }

        _pressed = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        if (bounds.Width <= 2 || bounds.Height <= 2)
        {
            return;
        }

        var accent = _tone == UsrActionButtonTone.Danger ? PharmaTheme.Danger : PharmaTheme.Primary;
        if (_hover || _pressed)
        {
            var fill = _pressed
                ? PharmaTheme.WithAlpha(accent, 36)
                : PharmaTheme.WithAlpha(accent, 22);
            RoundedDrawing.FillRounded(g, bounds, 8, fill);
        }

        TextRenderer.DrawText(
            g,
            Text,
            Font,
            bounds,
            accent,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

    private void UpdateAutoWidth()
    {
        if (string.IsNullOrEmpty(Text))
        {
            Width = 48;
            return;
        }

        var textSize = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, Height), TextFormatFlags.NoPadding);
        Width = Math.Max(48, textSize.Width + 20);
    }
}

internal sealed class UsrUserRow : Control
{
    private readonly UserListItemView _user;
    private bool _hover;
    private bool _isCurrentUser;
    private readonly UsrActionButton _editButton;
    private readonly UsrActionButton _toggleButton;
    private readonly UsrActionButton _deleteButton;

    public UsrUserRow(UserListItemView user)
    {
        _user = user;
        Height = PharmaTheme.UsersRowHeight;
        Cursor = Cursors.Hand;
        RightToLeft = RightToLeft.Yes;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;

        _editButton = new UsrActionButton { Text = "تعديل", Tone = UsrActionButtonTone.Primary };
        _toggleButton = new UsrActionButton
        {
            Text = _user.IsActive ? "تعطيل" : "تفعيل",
            Tone = UsrActionButtonTone.Primary
        };
        _deleteButton = new UsrActionButton { Text = "حذف", Tone = UsrActionButtonTone.Danger };

        _editButton.Click += (_, _) => EditRequested?.Invoke(this, EventArgs.Empty);
        _toggleButton.Click += (_, _) => ToggleActiveRequested?.Invoke(this, EventArgs.Empty);
        _deleteButton.Click += (_, _) => DeleteRequested?.Invoke(this, EventArgs.Empty);

        Controls.Add(_editButton);
        Controls.Add(_toggleButton);
        Controls.Add(_deleteButton);
        Resize += (_, _) => LayoutButtons();
    }

    public UserListItemView User => _user;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsCurrentUser
    {
        get => _isCurrentUser;
        set
        {
            _isCurrentUser = value;
            _toggleButton.Enabled = !value;
            _deleteButton.Enabled = !value;
            _toggleButton.Visible = !value;
            _deleteButton.Visible = !value;
            LayoutButtons();
        }
    }

    public event EventHandler? EditRequested;
    public event EventHandler? ToggleActiveRequested;
    public event EventHandler? DeleteRequested;
    public event EventHandler? DetailsRequested;

    public void ApplyThemeVisuals()
    {
        _editButton.ApplyThemeVisuals();
        _toggleButton.ApplyThemeVisuals();
        _deleteButton.ApplyThemeVisuals();
        Invalidate();
    }

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

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button != MouseButtons.Left || IsOverActionButton(e.Location))
        {
            return;
        }

        DetailsRequested?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        e.Graphics.Clear(Parent?.BackColor ?? PharmaTheme.Background);

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        var bounds = ClientRectangle;
        bounds.Inflate(-1, -1);
        var fill = _hover ? PharmaTheme.SurfaceContainerLow : PharmaTheme.Surface;
        RoundedDrawing.FillRounded(g, bounds, PharmaTheme.UsersRowCornerRadius, fill);
        RoundedDrawing.DrawRoundedBorder(g, bounds, PharmaTheme.UsersRowCornerRadius, PharmaTheme.BorderSoft, 1f);

        var compact = Width < 980;
        var columns = UsrColumnLayout.Calculate(bounds, compact);
        DrawUserCell(g, columns.User);
        DrawRoleBadge(g, columns.Role, _user.RoleName);
        if (!compact)
        {
            DrawCell(g, columns.LastLogin, _user.LastLoginText, PharmaTheme.TableCellFont, PharmaTheme.OnSurfaceVariant);
        }

        DrawStatusCell(g, columns.Status, _user.StatusText, _user.IsActive);
        LayoutButtons();
    }

    private void DrawUserCell(Graphics g, Rectangle rect)
    {
        if (rect.Width <= 0)
        {
            return;
        }

        const int avatarSize = 44;
        const int pad = 12;
        var avatarX = rect.Right - pad - avatarSize;
        var avatarY = rect.Y + (rect.Height - avatarSize) / 2;
        var avatarRect = new Rectangle(avatarX, avatarY, avatarSize, avatarSize);
        using var brush = new SolidBrush(PharmaTheme.PrimaryContainer);
        g.FillEllipse(brush, avatarRect);
        TextRenderer.DrawText(
            g,
            _user.Initials,
            PharmaTheme.NumberFont(12f, FontStyle.Bold),
            avatarRect,
            PharmaTheme.PrimaryDark,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var textX = rect.X + 8;
        var textW = Math.Max(40, avatarX - textX - 10);
        TextRenderer.DrawText(
            g,
            _user.DisplayName,
            PharmaTheme.ArabicFont(11f, FontStyle.Bold),
            new Rectangle(textX, rect.Y + 18, textW, 24),
            PharmaTheme.TextDark,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        TextRenderer.DrawText(
            g,
            _user.ShortId,
            PharmaTheme.SmallFont,
            new Rectangle(textX, rect.Y + 44, textW, 18),
            PharmaTheme.OnSurfaceVariant,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawRoleBadge(Graphics g, Rectangle rect, string roleName)
    {
        if (rect.Width <= 0 || string.IsNullOrWhiteSpace(roleName))
        {
            return;
        }

        var (back, fore) = UserDisplayHelper.GetRoleBadgeColors(roleName);
        var font = PharmaTheme.ArabicFont(9f, FontStyle.Bold);
        var textSize = TextRenderer.MeasureText(roleName, font, new Size(rect.Width - 16, rect.Height), TextFormatFlags.NoPadding);
        var pillW = Math.Min(rect.Width - 8, textSize.Width + 20);
        var pillH = Math.Max(24, textSize.Height + 8);
        var pillX = rect.Right - 8 - pillW;
        var pillY = rect.Y + (rect.Height - pillH) / 2;
        var pillRect = new Rectangle(pillX, pillY, pillW, pillH);
        RoundedDrawing.FillRounded(g, pillRect, pillH / 2, back);
        TextRenderer.DrawText(
            g,
            roleName,
            font,
            pillRect,
            fore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawStatusCell(Graphics g, Rectangle rect, string statusText, bool isActive)
    {
        if (rect.Width <= 0)
        {
            return;
        }

        const int dotSize = 8;
        var dotColor = isActive ? PharmaTheme.Success : PharmaTheme.OnSurfaceVariant;
        var text = string.IsNullOrWhiteSpace(statusText) ? UserDisplayHelper.FormatStatus(isActive) : statusText;
        var font = PharmaTheme.TableCellFont;
        var textSize = TextRenderer.MeasureText(text, font);
        var totalW = dotSize + 6 + textSize.Width;
        var startX = rect.Right - 8 - totalW;
        var centerY = rect.Y + rect.Height / 2;
        var dotRect = new Rectangle(startX, centerY - dotSize / 2, dotSize, dotSize);
        using var dotBrush = new SolidBrush(dotColor);
        g.FillEllipse(dotBrush, dotRect);

        var textRect = new Rectangle(dotRect.Right + 6, rect.Y, textSize.Width + 4, rect.Height);
        TextRenderer.DrawText(
            g,
            text,
            font,
            textRect,
            PharmaTheme.TextDark,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private static void DrawCell(Graphics g, Rectangle rect, string text, Font font, Color color)
    {
        if (rect.Width <= 0)
        {
            return;
        }

        TextRenderer.DrawText(
            g,
            text,
            font,
            Rectangle.Inflate(rect, -8, 0),
            color,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void LayoutButtons()
    {
        var columns = UsrColumnLayout.Calculate(ClientRectangle, Width < 980);
        if (columns.Actions.Width <= 0)
        {
            return;
        }

        const int gap = 6;
        var y = (Height - 28) / 2;
        var x = columns.Actions.Right - 8;
        _deleteButton.SetBounds(x - _deleteButton.Width, y, _deleteButton.Width, 28);
        x = _deleteButton.Left - gap;
        _toggleButton.SetBounds(x - _toggleButton.Width, y, _toggleButton.Width, 28);
        x = _toggleButton.Left - gap;
        _editButton.SetBounds(x - _editButton.Width, y, _editButton.Width, 28);
    }

    private bool IsOverActionButton(Point location) =>
        _editButton.Bounds.Contains(location)
        || _toggleButton.Bounds.Contains(location)
        || _deleteButton.Bounds.Contains(location);
}

internal sealed class UsrPaginationBar : UsrRoundedPanel
{
    private readonly Label _prevButton = new();
    private readonly Label _nextButton = new();
    private readonly Label _infoLabel = new();
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _fromIndex;
    private int _toIndex;
    private int _totalCount;

    public UsrPaginationBar() : base(18)
    {
        Height = 56;
        FillColor = PharmaTheme.Surface;
        RightToLeft = RightToLeft.Yes;

        _prevButton.Text = "السابق";
        _prevButton.AutoSize = true;
        _prevButton.Cursor = Cursors.Hand;
        _prevButton.Font = PharmaTheme.SmallFont;
        _prevButton.RightToLeft = RightToLeft.Yes;
        _prevButton.Click += (_, _) => PageChangeRequested?.Invoke(this, Math.Max(1, _currentPage - 1));

        _nextButton.Text = "التالي";
        _nextButton.AutoSize = true;
        _nextButton.Cursor = Cursors.Hand;
        _nextButton.Font = PharmaTheme.SmallFont;
        _nextButton.RightToLeft = RightToLeft.Yes;
        _nextButton.Click += (_, _) => PageChangeRequested?.Invoke(this, Math.Min(_totalPages, _currentPage + 1));

        _infoLabel.AutoSize = false;
        _infoLabel.Height = 24;
        _infoLabel.TextAlign = ContentAlignment.MiddleCenter;
        _infoLabel.Font = PharmaTheme.SmallFont;
        _infoLabel.RightToLeft = RightToLeft.Yes;

        Controls.Add(_infoLabel);
        Controls.Add(_nextButton);
        Controls.Add(_prevButton);
        Resize += (_, _) => LayoutBar();
    }

    public event EventHandler<int>? PageChangeRequested;

    public void Update(int currentPage, int totalPages, int fromIndex, int toIndex, int totalCount)
    {
        _currentPage = Math.Max(1, currentPage);
        _totalPages = Math.Max(1, totalPages);
        _fromIndex = fromIndex;
        _toIndex = toIndex;
        _totalCount = totalCount;
        _infoLabel.Text = _totalCount <= 0
            ? "لا يوجد مستخدمون"
            : $"عرض {_fromIndex} إلى {_toIndex} من {_totalCount} مستخدم";
        LayoutBar();
        Invalidate();
    }

    public new void ApplyThemeVisuals()
    {
        FillColor = PharmaTheme.Surface;
        _prevButton.ForeColor = _currentPage <= 1 ? PharmaTheme.MutedText : PharmaTheme.Primary;
        _nextButton.ForeColor = _currentPage >= _totalPages ? PharmaTheme.MutedText : PharmaTheme.Primary;
        _infoLabel.ForeColor = PharmaTheme.OnSurfaceVariant;
        base.ApplyThemeVisuals();
    }

    private void LayoutBar()
    {
        _prevButton.Location = new Point(20, 16);
        _nextButton.Location = new Point(Width - _nextButton.Width - 20, 16);
        var infoW = Math.Min(420, Math.Max(240, Width - 220));
        _infoLabel.SetBounds((Width - infoW) / 2, 16, infoW, 24);
    }
}

internal sealed class UsrUserDetailsPanel : UsrRoundedPanel
{
    private UserListItemView? _user;
    private readonly Label _backButton = new();
    private readonly Panel _contentPanel = new();

    public UsrUserDetailsPanel() : base(PharmaTheme.UsersRowCornerRadius)
    {
        FillColor = PharmaTheme.Surface;
        Visible = false;
        Width = PharmaTheme.UsersDetailsWidth;
        RightToLeft = RightToLeft.Yes;

        _backButton.Text = "رجوع";
        _backButton.AutoSize = true;
        _backButton.Cursor = Cursors.Hand;
        _backButton.Font = PharmaTheme.ArabicFont(10f, FontStyle.Bold);
        _backButton.RightToLeft = RightToLeft.Yes;
        _backButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);

        _contentPanel.AutoScroll = true;
        _contentPanel.BackColor = PharmaTheme.Surface;
        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.RightToLeft = RightToLeft.Yes;

        Controls.Add(_contentPanel);
        Controls.Add(_backButton);
        Resize += (_, _) => Render();
    }

    public event EventHandler? CloseRequested;

    public void Bind(UserListItemView? user)
    {
        _user = user;
        Visible = user is not null;
        Render();
    }

    public new void ApplyThemeVisuals()
    {
        FillColor = PharmaTheme.Surface;
        _backButton.ForeColor = PharmaTheme.Primary;
        _contentPanel.BackColor = PharmaTheme.Surface;
        base.ApplyThemeVisuals();
        Render();
    }

    private void Render()
    {
        _contentPanel.Controls.Clear();
        if (_user is null)
        {
            return;
        }

        _backButton.Location = new Point(Width - _backButton.Width - 16, 12);

        var contentW = Math.Max(280, ClientSize.Width - 24);
        var host = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = PharmaTheme.Surface,
            RightToLeft = RightToLeft.Yes,
            Width = contentW,
            Padding = new Padding(8, 44, 8, 12)
        };

        host.Controls.Add(MakeTitle(_user.DisplayName));
        host.Controls.Add(MakeRow("المعرف", _user.ShortId, contentW));
        host.Controls.Add(MakeRow("البريد الإلكتروني", string.IsNullOrWhiteSpace(_user.Email) ? "—" : _user.Email, contentW));
        host.Controls.Add(MakeRow("الهاتف", string.IsNullOrWhiteSpace(_user.Phone) ? "—" : _user.Phone, contentW));
        host.Controls.Add(MakeRow("الدور", _user.RoleName, contentW));
        host.Controls.Add(MakeRow("الحالة", _user.StatusText, contentW));
        host.Controls.Add(MakeRow("آخر تسجيل دخول", _user.LastLoginText, contentW));

        _contentPanel.Controls.Add(host);
    }

    private static Control MakeTitle(string text) => new Label
    {
        Text = text,
        AutoSize = true,
        Font = PharmaTheme.ArabicFont(16f, FontStyle.Bold),
        ForeColor = PharmaTheme.TextDark,
        RightToLeft = RightToLeft.Yes,
        Margin = new Padding(0, 0, 0, 12)
    };

    private static Control MakeRow(string caption, string value, int width)
    {
        var panel = new Panel
        {
            Height = 30,
            Width = width,
            Margin = new Padding(0, 0, 0, 4),
            BackColor = PharmaTheme.Surface,
            RightToLeft = RightToLeft.Yes
        };
        var cap = new Label
        {
            Text = caption,
            Dock = DockStyle.Right,
            Width = 120,
            ForeColor = PharmaTheme.OnSurfaceVariant,
            Font = PharmaTheme.SmallFont,
            TextAlign = ContentAlignment.MiddleRight,
            RightToLeft = RightToLeft.Yes
        };
        var val = new Label
        {
            Text = value,
            Dock = DockStyle.Fill,
            ForeColor = PharmaTheme.TextDark,
            Font = PharmaTheme.BodyFont,
            TextAlign = ContentAlignment.MiddleRight,
            AutoEllipsis = true,
            RightToLeft = RightToLeft.Yes
        };
        panel.Controls.Add(val);
        panel.Controls.Add(cap);
        return panel;
    }
}
