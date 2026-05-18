using System.Drawing;

namespace Pharmacy.WinForms.Ui;

internal static class PharmaTheme
{
    public static readonly Color PrimaryGreen = Color.FromArgb(7, 100, 67); // #076443
    /// <summary>Secondary / hover surface for primary actions (#2D7D5A).</summary>
    public static readonly Color PrimaryContainer = Color.FromArgb(45, 125, 90);
    /// <summary>Accent teal for highlights and focus (#2BB8A8).</summary>
    public static readonly Color AccentTeal = Color.FromArgb(43, 184, 168);
    public static readonly Color SoftGreenBackground = Color.FromArgb(231, 255, 241); // #E7FFF1
    public static readonly Color CardBackground = Color.White; // #FFFFFF surface
    /// <summary>Main app / dashboard backdrop.</summary>
    public static Color Background => SoftGreenBackground;
    /// <summary>Cards, panels, and elevated surfaces.</summary>
    public static Color Surface => CardBackground;
    public static readonly Color InputSurface = Color.FromArgb(246, 249, 247);
    public static readonly Color TextDark = Color.FromArgb(11, 31, 23);
    public static readonly Color MutedText = Color.FromArgb(111, 122, 114); // #6F7A72
    public static readonly Color SidebarBackground = Color.FromArgb(5, 72, 49);
    public static readonly Color SidebarHover = Color.FromArgb(14, 92, 63);
    public static readonly Color SidebarActive = Color.FromArgb(22, 118, 78);
    public static readonly Color BorderLight = Color.FromArgb(214, 228, 220);
    public static readonly Color Danger = Color.FromArgb(176, 42, 42);
    public static readonly Color Warning = Color.FromArgb(180, 120, 20);

    // Login screen (read by LoginForm / LoginBackgroundControl; future theme switch can remap these)
    public static readonly Color LoginGradientTop = Color.FromArgb(238, 252, 247);
    public static readonly Color LoginGradientBottom = Color.FromArgb(188, 228, 218);
    public static readonly Color LoginCardFill = Color.FromArgb(253, 255, 254);
    public static readonly Color LoginCardBorder = Color.FromArgb(218, 234, 228);
    public static readonly Color LoginOverlayScrim = Color.FromArgb(170, 232, 248, 240);

    public static Font TitleFont { get; } = new("Segoe UI", 14F, FontStyle.Bold);
    public static Font SectionFont { get; } = new("Segoe UI", 11.5F, FontStyle.Bold);
    public static Font BodyFont { get; } = new("Segoe UI", 10F);
    public static Font SmallFont { get; } = new("Segoe UI", 9F);
    public static Font StatValueFont { get; } = new("Segoe UI", 20F, FontStyle.Bold);
}
