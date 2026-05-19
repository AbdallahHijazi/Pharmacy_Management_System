using System.Drawing;

namespace Pharmacy.WinForms.Ui;

internal static class PharmaTheme
{
    public static readonly Color PrimaryGreen = Color.FromArgb(7, 100, 67);
    public static readonly Color PrimaryContainer = Color.FromArgb(45, 125, 90);
    public static readonly Color Success = Color.FromArgb(0, 127, 86);
    public static readonly Color AccentTeal = Color.FromArgb(43, 184, 168);
    public static readonly Color SoftGreenBackground = Color.FromArgb(231, 255, 241);
    public static readonly Color CardBackground = Color.White;
    public static readonly Color SurfaceContainerLowest = Color.White;
    public static readonly Color SurfaceContainerLow = Color.FromArgb(225, 249, 235);
    public static readonly Color SurfaceContainer = Color.FromArgb(220, 244, 229);
    public static readonly Color SurfaceContainerHighest = Color.FromArgb(208, 232, 218);
    public static readonly Color OutlineVariant = Color.FromArgb(190, 201, 192);
    public static readonly Color OnSurfaceVariant = Color.FromArgb(63, 73, 67);
    public static Color Background => SoftGreenBackground;
    public static Color Surface => CardBackground;
    public static readonly Color InputSurface = Color.FromArgb(246, 249, 247);
    public static readonly Color TextDark = Color.FromArgb(11, 31, 23);
    public static readonly Color MutedText = Color.FromArgb(111, 122, 114);
    public static readonly Color SidebarBackground = Color.FromArgb(5, 72, 49);
    public static readonly Color SidebarHover = Color.FromArgb(14, 92, 63);
    public static readonly Color SidebarActive = Color.FromArgb(22, 118, 78);
    public static readonly Color SidebarLightBackground = Color.FromArgb(244, 246, 247);
    public static readonly Color SidebarNavHoverFill = Color.FromArgb(230, 241, 235);
    public static readonly Color SidebarDivider = Color.FromArgb(210, 220, 214);
    public static readonly Color BorderLight = Color.FromArgb(214, 228, 220);
    public static readonly Color BorderSoft = Color.FromArgb(220, 232, 224);
    public static readonly Color Danger = Color.FromArgb(186, 26, 26);
    public static readonly Color Warning = Color.FromArgb(245, 158, 11);
    public static readonly Color WarningStrong = Color.FromArgb(217, 119, 6);
    public static readonly Color WarningSurface = Color.FromArgb(255, 251, 235);
    public static readonly Color SuccessSurface = Color.FromArgb(220, 244, 229);
    public static readonly Color ErrorContainer = Color.FromArgb(255, 218, 214);
    public static readonly Color TopBarGradientDeep = Color.FromArgb(255, 220, 241, 232);
    public static readonly Color DashboardCardShadow = Color.FromArgb(10, 7, 100, 67);
    public static readonly Color PrimaryFixed = Color.FromArgb(163, 243, 200);

    public const int DashboardCardCornerRadius = 18;
    public const int DashboardStatCornerRadius = 18;
    public const int DashboardSectionCornerRadius = 18;
    public const int DashboardButtonCornerRadius = 14;
    public const int DashboardSearchCornerRadius = 16;
    public const int DashboardSidebarItemRadius = 12;

    private static readonly string[] ArabicFontCandidates = ["Cairo", "Segoe UI", "Tahoma"];
    private static readonly string[] NumberFontCandidates = ["Inter", "Segoe UI", "Tahoma"];

    public static Font ArabicFont(float size, FontStyle style = FontStyle.Regular) =>
        CreateFont(ArabicFontCandidates, size, style);

    public static Font NumberFont(float size, FontStyle style = FontStyle.Bold) =>
        CreateFont(NumberFontCandidates, size, style);

    public static Font IconFont(float size) => new("Segoe MDL2 Assets", size, FontStyle.Regular, GraphicsUnit.Point);

    public static Font TitleFont => ArabicFont(14f, FontStyle.Bold);
    public static Font DashboardHeadlineFont => ArabicFont(22f, FontStyle.Bold);
    public static Font DashboardSubtitleFont => ArabicFont(10.5f);
    public static Font SectionFont => ArabicFont(12f, FontStyle.Bold);
    public static Font BodyFont => ArabicFont(10.25f);
    public static Font SmallFont => ArabicFont(9.25f);
    public static Font StatTitleFont => ArabicFont(10f, FontStyle.Bold);
    public static Font StatValueFont => NumberFont(24f, FontStyle.Bold);
    public static Font StatBadgeFont => NumberFont(9f, FontStyle.Bold);
    public static Font TableHeaderFont => ArabicFont(9.5f, FontStyle.Bold);
    public static Font TableCellFont => ArabicFont(10f);
    public static Font TableAmountFont => NumberFont(10.5f, FontStyle.Bold);

    public static Font LoginTitleFont => ArabicFont(20f, FontStyle.Bold);
    public static Font LoginSubtitleFont => ArabicFont(9.75f);
    public static Font LoginButtonFont => ArabicFont(11f, FontStyle.Bold);
    public static Font LoginFieldLabelFont => ArabicFont(10f, FontStyle.Bold);

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

    public static readonly Color LoginGradientTop = Color.FromArgb(238, 252, 247);
    public static readonly Color LoginGradientBottom = Color.FromArgb(188, 228, 218);
    public static readonly Color LoginCardFill = Color.FromArgb(253, 255, 254);
    public static readonly Color LoginCardBorder = Color.FromArgb(218, 234, 228);
    public static readonly Color LoginOverlayScrim = Color.FromArgb(170, 232, 248, 240);
    public static readonly Color LoginErrorSurface = Color.FromArgb(255, 251, 249);
    public static readonly Color LoginErrorBorder = Color.FromArgb(232, 200, 200);
    public static readonly Color LoginRevealHover = Color.FromArgb(236, 248, 242);

    private static Font CreateFont(string[] candidates, float size, FontStyle style)
    {
        foreach (var name in candidates)
        {
            try
            {
                return new Font(name, size, style, GraphicsUnit.Point);
            }
            catch
            {
                // Try next candidate.
            }
        }

        return new Font(FontFamily.GenericSansSerif, size, style, GraphicsUnit.Point);
    }
}
