using System.Drawing;

namespace Pharmacy.WinForms.Ui;

internal static class PharmaTheme
{
    private static ThemeSnapshot T => ThemeManager.Snapshot;

    public static Color PrimaryGreen => T.PrimaryGreen;
    public static Color PrimaryContainer => T.PrimaryContainer;
    public static Color Success => T.Success;
    public static Color AccentTeal => T.AccentTeal;
    public static Color SoftGreenBackground => T.SoftGreenBackground;
    public static Color CardBackground => T.CardBackground;
    public static Color SurfaceContainerLowest => T.SurfaceContainerLowest;
    public static Color SurfaceContainerLow => T.SurfaceContainerLow;
    public static Color SurfaceContainer => T.SurfaceContainer;
    public static Color SurfaceContainerHighest => T.SurfaceContainerHighest;
    public static Color OutlineVariant => T.OutlineVariant;
    public static Color OnSurfaceVariant => T.OnSurfaceVariant;
    public static Color Background => T.SoftGreenBackground;
    public static Color Surface => T.CardBackground;
    public static Color InputSurface => T.InputSurface;
    public static Color TextDark => T.TextDark;
    public static Color MutedText => T.MutedText;
    public static Color SidebarBackground => T.SidebarBackground;
    public static Color SidebarHover => T.SidebarHover;
    public static Color SidebarActive => T.SidebarActive;
    public static Color SidebarLightBackground => T.SidebarLightBackground;
    public static Color SidebarNavHoverFill => T.SidebarNavHoverFill;
    public static Color SidebarDivider => T.SidebarDivider;
    public static Color BorderLight => T.BorderLight;
    public static Color BorderSoft => T.BorderSoft;
    public static Color Danger => T.Danger;
    public static Color Warning => T.Warning;
    public static Color WarningStrong => T.WarningStrong;
    public static Color WarningSurface => T.WarningSurface;
    public static Color SuccessSurface => T.SuccessSurface;
    public static Color ErrorContainer => T.ErrorContainer;
    public static Color TopBarGradientDeep => T.TopBarGradientDeep;
    public static Color DashboardCardShadow => T.DashboardCardShadow;
    public static Color PrimaryFixed => T.PrimaryFixed;

    public const int DashboardCardCornerRadius = 20;
    public const int DashboardStatCornerRadius = 20;
    public const int DashboardSectionCornerRadius = 22;
    public const int DashboardButtonCornerRadius = 14;
    public const int DashboardSearchCornerRadius = 18;
    public const int DashboardSidebarItemRadius = 14;
    public const int DashboardQuickActionRadius = 14;

    private static readonly string[] ArabicFontCandidates = ["Cairo", "Segoe UI", "Tahoma"];
    private static readonly string[] NumberFontCandidates = ["Inter", "Segoe UI", "Tahoma"];

    private static float Sc(float sizePx) => sizePx * FontScaleManager.Multiplier;

    public static Font ArabicFont(float size, FontStyle style = FontStyle.Regular) =>
        CreateFont(ArabicFontCandidates, Sc(size), style);

    public static Font NumberFont(float size, FontStyle style = FontStyle.Bold) =>
        CreateFont(NumberFontCandidates, Sc(size), style);

    public static Font IconFont(float size) =>
        new("Segoe MDL2 Assets", Sc(size), FontStyle.Regular, GraphicsUnit.Point);

    public static Font TitleFont => ArabicFont(14f, FontStyle.Bold);
    public static Font DashboardHeadlineFont => ArabicFont(22f, FontStyle.Bold);
    public static Font DashboardSubtitleFont => ArabicFont(10.5f);
    public static Font SidebarBrandFont => ArabicFont(20f, FontStyle.Bold);
    public static Font SidebarSubtitleFont => ArabicFont(10f);
    public static Font SidebarNavFont => ArabicFont(11.5f, FontStyle.Bold);
    public static Font SidebarLogoutFont => ArabicFont(11.5f, FontStyle.Bold);
    public static Font SectionFont => ArabicFont(12f, FontStyle.Bold);
    public static Font BodyFont => ArabicFont(10.25f);
    public static Font SmallFont => ArabicFont(9.25f);
    public static Font StatTitleFont => ArabicFont(10f, FontStyle.Bold);
    public static Font StatValueFont => NumberFont(22f, FontStyle.Bold);
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

    public static Color LoginGradientTop => T.LoginGradientTop;
    public static Color LoginGradientBottom => T.LoginGradientBottom;
    public static Color LoginCardFill => T.LoginCardFill;
    public static Color LoginCardBorder => T.LoginCardBorder;
    public static Color LoginOverlayScrim => T.LoginOverlayScrim;
    public static Color LoginErrorSurface => T.LoginErrorSurface;
    public static Color LoginErrorBorder => T.LoginErrorBorder;
    public static Color LoginRevealHover => T.LoginRevealHover;

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
