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
    /// <summary>Primary card / KPI surface (reference: surface-container-lowest).</summary>
    public static readonly Color SurfaceContainerLowest = Color.White;
    public static readonly Color SurfaceContainerLow = Color.FromArgb(225, 249, 235); // #E1F9EB
    public static readonly Color SurfaceContainer = Color.FromArgb(220, 244, 229);
    public static readonly Color SurfaceContainerHighest = Color.FromArgb(208, 232, 218); // #D0E8DA
    public static readonly Color OutlineVariant = Color.FromArgb(190, 201, 192); // #BEC9C0
    /// <summary>Secondary text on surfaces (reference: on-surface-variant).</summary>
    public static readonly Color OnSurfaceVariant = Color.FromArgb(63, 73, 67); // #3F4943
    /// <summary>Main app / dashboard backdrop.</summary>
    public static Color Background => SoftGreenBackground;
    /// <summary>Cards, panels, and elevated surfaces.</summary>
    public static Color Surface => CardBackground;
    public static readonly Color InputSurface = Color.FromArgb(246, 249, 247);
    public static readonly Color TextDark = Color.FromArgb(11, 31, 23);
    public static readonly Color MutedText = Color.FromArgb(111, 122, 114); // #6F7A72
    /// <summary>Legacy dark sidebar (login / older screens); dashboard uses <see cref="SidebarLightBackground"/>.</summary>
    public static readonly Color SidebarBackground = Color.FromArgb(5, 72, 49);
    public static readonly Color SidebarHover = Color.FromArgb(14, 92, 63);
    public static readonly Color SidebarActive = Color.FromArgb(22, 118, 78);
    /// <summary>Light docked sidebar (PharmaCare reference).</summary>
    public static readonly Color SidebarLightBackground = Color.FromArgb(244, 246, 247); // #F4F6F7
    public static readonly Color SidebarNavHoverFill = Color.FromArgb(230, 241, 235);
    public static readonly Color SidebarDivider = Color.FromArgb(210, 220, 214);
    public static readonly Color BorderLight = Color.FromArgb(214, 228, 220);
    public static readonly Color Danger = Color.FromArgb(186, 26, 26); // #BA1A1A
    public static readonly Color Warning = Color.FromArgb(245, 158, 11); // #F59E0B
    public static readonly Color WarningStrong = Color.FromArgb(217, 119, 6); // #D97706
    public static readonly Color WarningSurface = Color.FromArgb(255, 251, 235); // #FFFBEB
    public static readonly Color ErrorContainer = Color.FromArgb(255, 218, 214); // #FFDAD6
    public static readonly Color TopBarGradientDeep = Color.FromArgb(255, 220, 241, 232);
    /// <summary>Soft KPI / section shadow tint (reference: rgba(7,100,67,0.02)).</summary>
    public static readonly Color DashboardCardShadow = Color.FromArgb(6, 7, 100, 67);

    public const int DashboardSectionCornerRadius = 14;
    public const int DashboardStatCornerRadius = 16;

    // Login screen (read by LoginForm / LoginBackgroundControl; future theme switch can remap these)
    public static readonly Color LoginGradientTop = Color.FromArgb(238, 252, 247);
    public static readonly Color LoginGradientBottom = Color.FromArgb(188, 228, 218);
    public static readonly Color LoginCardFill = Color.FromArgb(253, 255, 254);
    public static readonly Color LoginCardBorder = Color.FromArgb(218, 234, 228);
    public static readonly Color LoginOverlayScrim = Color.FromArgb(170, 232, 248, 240);

    public static Font TitleFont { get; } = new("Segoe UI", 14F, FontStyle.Bold);
    /// <summary>Dashboard page title (reference: text-2xl / ~2rem).</summary>
    public static Font DashboardHeadlineFont { get; } = new("Segoe UI", 15.75F, FontStyle.Bold);
    public static Font DashboardSubtitleFont { get; } = new("Segoe UI", 10F, FontStyle.Regular);
    public static Font SectionFont { get; } = new("Segoe UI", 11.5F, FontStyle.Bold);
    public static Font BodyFont { get; } = new("Segoe UI", 10F);
    public static Font SmallFont { get; } = new("Segoe UI", 9F);
    public static Font StatValueFont { get; } = new("Segoe UI", 20F, FontStyle.Bold);
    public static Font LoginTitleFont { get; } = new("Segoe UI", 20F, FontStyle.Bold);
    public static Font LoginSubtitleFont { get; } = new("Segoe UI", 9.75F);
    public static Font LoginButtonFont { get; } = new("Segoe UI", 11F, FontStyle.Bold);
    public static Font LoginFieldLabelFont { get; } = new("Segoe UI", 10F, FontStyle.Bold);

    public const int LoginButtonHeight = 52;
    public const int LoginCardMinWidth = 420;
    public const int LoginCardMaxWidth = 480;
    public const int LoginCardCornerRadius = 28;
    public const int LoginInputCornerRadius = 22;
    public const int LoginButtonCornerRadius = 24;
    public const int LoginInputHeight = 52;
    public const int LoginIconColumnWidth = 48;
    public const int LoginRevealColumnWidth = 44;
    public const int LoginNoticeCornerRadius = 16;

    public static readonly Color LoginErrorSurface = Color.FromArgb(255, 251, 249);
    public static readonly Color LoginErrorBorder = Color.FromArgb(232, 200, 200);
    public static readonly Color LoginRevealHover = Color.FromArgb(236, 248, 242);
}
