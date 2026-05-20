using System.Drawing;

namespace Pharmacy.WinForms.Ui;

internal static class ThemeManager
{
    public static event EventHandler? ThemeChanged;

    private static int _currentIndex;

    internal static ThemeSnapshot Snapshot => Themes[Math.Clamp(_currentIndex, 0, Themes.Length - 1)];

    public static int ThemeCount => Themes.Length;

    public static int CurrentIndex => Math.Clamp(_currentIndex, 0, Themes.Length - 1);

    public static void ApplyThemeIndex(int index)
    {
        _currentIndex = Math.Clamp(index, 0, Themes.Length - 1);
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    public static string GetThemeHintName(int index) => ThemeNames[Math.Clamp(index, 0, ThemeNames.Length - 1)];

    internal static readonly string[] ThemeNames =
    [
        "Healthcare Green",
        "Medical Blue",
        "Clinical Purple",
        "Sky Teal",
        "Dark Mode",
        "Neutral Gray"
    ];

    private static ThemeSnapshot Mk(
        Color primary,
        Color primaryContainer,
        Color softBg,
        Color surfaceLow,
        Color surfaceMid,
        Color surfaceHigh,
        Color borderSoft,
        Color accentTeal,
        Color sidebarLight,
        Color navHoverFill,
        Color topBarDeepBase,
        Color shadowBase,
        bool isDark)
    {
        var text = isDark ? Color.FromArgb(232, 236, 233) : Color.FromArgb(11, 31, 23);
        var muted = isDark ? Color.FromArgb(152, 160, 155) : Color.FromArgb(111, 122, 114);
        var onSurfVar = isDark ? Color.FromArgb(180, 187, 181) : Color.FromArgb(63, 73, 67);
        var outlineVar = isDark ? Color.FromArgb(86, 92, 88) : Color.FromArgb(190, 201, 192);

        Color Mix(Color a, Color b, float t)
        {
            var tr = ClampChannel(a.R + (b.R - a.R) * t);
            var tg = ClampChannel(a.G + (b.G - a.G) * t);
            var tb = ClampChannel(a.B + (b.B - a.B) * t);
            return Color.FromArgb(a.A, tr, tg, tb);
        }

        static int ClampChannel(float v) => (int)Math.Clamp(v, 0, 255);

        var card = isDark ? Color.FromArgb(37, 40, 39) : Color.White;
        var input = isDark ? Color.FromArgb(44, 48, 46) : Color.FromArgb(246, 249, 247);
        var warnSurf = isDark ? Color.FromArgb(48, 42, 28) : Color.FromArgb(255, 251, 235);
        var successSurf = isDark ? Color.FromArgb(28, 44, 36) : Color.FromArgb(220, 244, 229);
        var errCont = isDark ? Color.FromArgb(52, 32, 30) : Color.FromArgb(255, 218, 214);

        var sb = isDark ? Mix(primary, Color.Black, 0.55f) : Color.FromArgb(5, 72, 49);
        if (isDark)
        {
            sb = Mix(Color.FromArgb(24, 28, 26), primary, 0.35f);
        }

        var sbHover = isDark ? Mix(sb, Color.White, 0.08f) : Color.FromArgb(14, 92, 63);
        var sbActive = isDark ? Mix(sb, Color.White, 0.14f) : Color.FromArgb(22, 118, 78);

        var topDeep = isDark
            ? Color.FromArgb(255, 28, 32, 30)
            : Color.FromArgb(255, topBarDeepBase.R, topBarDeepBase.G, topBarDeepBase.B);

        var shadow = Color.FromArgb(10, shadowBase.R, shadowBase.G, shadowBase.B);

        var loginTop = isDark ? Color.FromArgb(28, 32, 35) : Mix(softBg, Color.White, 0.55f);
        var loginBot = isDark ? Color.FromArgb(18, 22, 24) : Mix(surfaceLow, primary, 0.12f);
        var loginCard = isDark ? Color.FromArgb(40, 44, 42) : Color.FromArgb(253, 255, 254);
        var loginBorder = isDark ? Color.FromArgb(72, 78, 75) : Mix(borderSoft, primary, 0.15f);

        return new ThemeSnapshot
        {
            PrimaryGreen = primary,
            PrimaryContainer = primaryContainer,
            Success = Color.FromArgb(0, 127, 86),
            AccentTeal = accentTeal,
            SoftGreenBackground = softBg,
            CardBackground = card,
            SurfaceContainerLowest = card,
            SurfaceContainerLow = surfaceLow,
            SurfaceContainer = surfaceMid,
            SurfaceContainerHighest = surfaceHigh,
            OutlineVariant = outlineVar,
            OnSurfaceVariant = onSurfVar,
            InputSurface = input,
            TextDark = text,
            MutedText = muted,
            SidebarBackground = sb,
            SidebarHover = sbHover,
            SidebarActive = sbActive,
            SidebarLightBackground = sidebarLight,
            SidebarNavHoverFill = navHoverFill,
            SidebarDivider = isDark ? Color.FromArgb(60, 66, 62) : Color.FromArgb(210, 220, 214),
            BorderLight = isDark ? Color.FromArgb(58, 64, 60) : Color.FromArgb(214, 228, 220),
            BorderSoft = borderSoft,
            Danger = Color.FromArgb(186, 26, 26),
            Warning = Color.FromArgb(245, 158, 11),
            WarningStrong = Color.FromArgb(217, 119, 6),
            WarningSurface = warnSurf,
            SuccessSurface = successSurf,
            ErrorContainer = errCont,
            TopBarGradientDeep = topDeep,
            DashboardCardShadow = shadow,
            PrimaryFixed = isDark ? Mix(primaryContainer, Color.White, 0.35f) : Color.FromArgb(163, 243, 200),
            LoginGradientTop = loginTop,
            LoginGradientBottom = loginBot,
            LoginCardFill = loginCard,
            LoginCardBorder = loginBorder,
            LoginOverlayScrim = isDark ? Color.FromArgb(170, 10, 12, 11) : Color.FromArgb(170, 232, 248, 240),
            LoginErrorSurface = isDark ? Color.FromArgb(42, 32, 30) : Color.FromArgb(255, 251, 249),
            LoginErrorBorder = isDark ? Color.FromArgb(90, 52, 50) : Color.FromArgb(232, 200, 200),
            LoginRevealHover = isDark ? Color.FromArgb(48, 56, 52) : Color.FromArgb(236, 248, 242)
        };
    }

    private static readonly ThemeSnapshot[] Themes =
    [
        // 0 Healthcare Green (matches original PharmaCare palette)
        Mk(
            Color.FromArgb(7, 100, 67),
            Color.FromArgb(45, 125, 90),
            Color.FromArgb(231, 255, 241),
            Color.FromArgb(225, 249, 235),
            Color.FromArgb(220, 244, 229),
            Color.FromArgb(208, 232, 218),
            Color.FromArgb(220, 232, 224),
            Color.FromArgb(43, 184, 168),
            Color.FromArgb(244, 246, 247),
            Color.FromArgb(230, 241, 235),
            Color.FromArgb(220, 241, 232),
            Color.FromArgb(7, 100, 67),
            isDark: false),

        // 1 Medical Blue
        Mk(
            Color.FromArgb(30, 64, 175),
            Color.FromArgb(59, 130, 246),
            Color.FromArgb(236, 244, 255),
            Color.FromArgb(219, 234, 254),
            Color.FromArgb(207, 225, 252),
            Color.FromArgb(191, 216, 250),
            Color.FromArgb(210, 226, 244),
            Color.FromArgb(20, 184, 166),
            Color.FromArgb(244, 246, 250),
            Color.FromArgb(226, 235, 255),
            Color.FromArgb(226, 235, 255),
            Color.FromArgb(37, 99, 235),
            isDark: false),

        // 2 Clinical Purple
        Mk(
            Color.FromArgb(107, 33, 168),
            Color.FromArgb(147, 51, 234),
            Color.FromArgb(248, 240, 255),
            Color.FromArgb(241, 225, 255),
            Color.FromArgb(236, 214, 255),
            Color.FromArgb(229, 200, 255),
            Color.FromArgb(230, 222, 240),
            Color.FromArgb(45, 212, 191),
            Color.FromArgb(247, 245, 250),
            Color.FromArgb(237, 228, 250),
            Color.FromArgb(242, 230, 255),
            Color.FromArgb(109, 40, 217),
            isDark: false),

        // 3 Sky Teal
        Mk(
            Color.FromArgb(15, 118, 110),
            Color.FromArgb(45, 212, 191),
            Color.FromArgb(224, 252, 248),
            Color.FromArgb(204, 251, 241),
            Color.FromArgb(178, 245, 234),
            Color.FromArgb(153, 246, 228),
            Color.FromArgb(190, 232, 225),
            Color.FromArgb(8, 145, 178),
            Color.FromArgb(244, 248, 248),
            Color.FromArgb(214, 245, 240),
            Color.FromArgb(210, 244, 236),
            Color.FromArgb(13, 148, 136),
            isDark: false),

        // 4 Dark Mode
        Mk(
            Color.FromArgb(52, 211, 153),
            Color.FromArgb(16, 185, 129),
            Color.FromArgb(23, 26, 25),
            Color.FromArgb(34, 38, 36),
            Color.FromArgb(40, 44, 42),
            Color.FromArgb(50, 55, 52),
            Color.FromArgb(68, 74, 70),
            Color.FromArgb(45, 212, 191),
            Color.FromArgb(30, 34, 32),
            Color.FromArgb(44, 56, 50),
            Color.FromArgb(24, 30, 28),
            Color.FromArgb(52, 211, 153),
            isDark: true),

        // 5 Neutral Gray
        Mk(
            Color.FromArgb(82, 82, 91),
            Color.FromArgb(113, 113, 122),
            Color.FromArgb(244, 244, 245),
            Color.FromArgb(235, 236, 238),
            Color.FromArgb(228, 228, 231),
            Color.FromArgb(212, 212, 216),
            Color.FromArgb(220, 222, 224),
            Color.FromArgb(99, 102, 106),
            Color.FromArgb(248, 248, 249),
            Color.FromArgb(235, 236, 238),
            Color.FromArgb(240, 240, 242),
            Color.FromArgb(82, 82, 91),
            isDark: false)
    ];
}
