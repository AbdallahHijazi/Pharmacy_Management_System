using System.Drawing;

namespace Pharmacy.WinForms.Ui;

internal static class PharmaTheme
{
    public static readonly Color PrimaryGreen = Color.FromArgb(7, 100, 67); // #076443
    public static readonly Color SoftGreenBackground = Color.FromArgb(231, 255, 241);
    public static readonly Color CardBackground = Color.White;
    public static readonly Color TextDark = Color.FromArgb(11, 31, 23);
    public static readonly Color MutedText = Color.FromArgb(111, 122, 114);
    public static readonly Color SidebarBackground = Color.FromArgb(5, 72, 49);
    public static readonly Color SidebarHover = Color.FromArgb(14, 92, 63);
    public static readonly Color SidebarActive = Color.FromArgb(22, 118, 78);
    public static readonly Color BorderLight = Color.FromArgb(214, 228, 220);
    public static readonly Color Danger = Color.FromArgb(176, 42, 42);
    public static readonly Color Warning = Color.FromArgb(180, 120, 20);

    public static Font TitleFont { get; } = new("Segoe UI", 14F, FontStyle.Bold);
    public static Font SectionFont { get; } = new("Segoe UI", 11.5F, FontStyle.Bold);
    public static Font BodyFont { get; } = new("Segoe UI", 10F);
    public static Font SmallFont { get; } = new("Segoe UI", 9F);
    public static Font StatValueFont { get; } = new("Segoe UI", 20F, FontStyle.Bold);
}
