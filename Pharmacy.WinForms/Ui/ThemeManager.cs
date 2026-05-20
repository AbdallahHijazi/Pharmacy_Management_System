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

    public static Color GetThemeSwatchColor(int index) =>
        Themes[Math.Clamp(index, 0, Themes.Length - 1)].PrimaryGreen;

    internal static readonly string[] ThemeNames =
    [
        "Healthcare Green",
        "Medical Blue",
        "Clinical Teal",
        "Soft Mint",
        "Warm Neutral",
        "Dark Clinical"
    ];

    private static Color Hex(string hex) => ColorTranslator.FromHtml(hex);

    private static ThemeSnapshot Medical(
        string primaryHex,
        string primaryContainerHex,
        string backgroundHex,
        string surfaceHex,
        string surfaceContainerHex,
        string surfaceHighHex,
        string surfaceHighestHex,
        string textPrimaryHex,
        string textMutedHex,
        string accentHex,
        bool isDark)
    {
        var primary = Hex(primaryHex);
        var primaryContainer = Hex(primaryContainerHex);
        var background = Hex(backgroundHex);
        var surface = Hex(surfaceHex);
        var surfaceLow = surfaceContainerHex == surfaceHex
            ? Mix(surface, background, 0.08f)
            : Hex(surfaceContainerHex);
        var surfaceMid = Hex(surfaceContainerHex);
        var surfaceHigh = Hex(surfaceHighHex);
        var surfaceHighest = Hex(surfaceHighestHex);
        var text = Hex(textPrimaryHex);
        var muted = Hex(textMutedHex);
        var accent = Hex(accentHex);
        var borderSoft = isDark ? Mix(surfaceHigh, accent, 0.12f) : Mix(surfaceMid, muted, 0.22f);

        Color Mix(Color a, Color b, float t)
        {
            static int C(float v) => (int)Math.Clamp(v, 0, 255);
            return Color.FromArgb(
                C(a.R + (b.R - a.R) * t),
                C(a.G + (b.G - a.G) * t),
                C(a.B + (b.B - a.B) * t));
        }

        var onSurfVar = isDark ? Mix(muted, Color.White, 0.25f) : Mix(muted, text, 0.35f);
        var outlineVar = isDark ? Mix(surfaceHighest, Color.White, 0.08f) : Mix(borderSoft, muted, 0.35f);
        var input = isDark ? surfaceHigh : Mix(surface, background, 0.35f);

        var sidebarLight = isDark ? Mix(background, Color.White, 0.04f) : Mix(background, Color.White, 0.55f);
        var navHover = isDark ? Mix(surfaceHigh, accent, 0.18f) : Mix(surfaceLow, accent, 0.28f);

        var sb = isDark ? Mix(background, primary, 0.35f) : Mix(primary, Color.Black, 0.55f);
        var sbHover = isDark ? Mix(sb, Color.White, 0.08f) : Mix(sb, Color.White, 0.06f);
        var sbActive = isDark ? Mix(sb, Color.White, 0.14f) : Mix(primary, Color.Black, 0.25f);

        var topDeep = isDark
            ? Color.FromArgb(255, background.R, background.G, background.B)
            : Color.FromArgb(255, Mix(background, accent, 0.15f).R, Mix(background, accent, 0.15f).G, Mix(background, accent, 0.15f).B);

        var shadow = Color.FromArgb(10, primary.R, primary.G, primary.B);

        return new ThemeSnapshot
        {
            PrimaryGreen = primary,
            PrimaryContainer = primaryContainer,
            Success = isDark ? accent : Color.FromArgb(0, 127, 86),
            AccentTeal = accent,
            SoftGreenBackground = background,
            CardBackground = surface,
            SurfaceContainerLowest = surface,
            SurfaceContainerLow = surfaceLow,
            SurfaceContainer = surfaceMid,
            SurfaceContainerHigh = surfaceHigh,
            SurfaceContainerHighest = surfaceHighest,
            OutlineVariant = outlineVar,
            OnSurfaceVariant = onSurfVar,
            InputSurface = input,
            TextDark = text,
            MutedText = muted,
            SidebarBackground = sb,
            SidebarHover = sbHover,
            SidebarActive = sbActive,
            SidebarLightBackground = sidebarLight,
            SidebarNavHoverFill = navHover,
            SidebarDivider = isDark ? Mix(surfaceHighest, Color.White, 0.06f) : Mix(borderSoft, muted, 0.4f),
            BorderLight = isDark ? Mix(surfaceHigh, Color.White, 0.05f) : Mix(borderSoft, background, 0.35f),
            BorderSoft = borderSoft,
            Danger = Color.FromArgb(186, 26, 26),
            Warning = Color.FromArgb(245, 158, 11),
            WarningStrong = Color.FromArgb(217, 119, 6),
            WarningSurface = isDark ? Color.FromArgb(48, 42, 28) : Color.FromArgb(255, 251, 235),
            SuccessSurface = isDark ? Mix(surfaceHigh, accent, 0.2f) : Mix(surfaceLow, accent, 0.35f),
            ErrorContainer = isDark ? Color.FromArgb(52, 32, 30) : Color.FromArgb(255, 218, 214),
            TopBarGradientDeep = topDeep,
            DashboardCardShadow = shadow,
            PrimaryFixed = isDark ? Mix(primaryContainer, Color.White, 0.35f) : Mix(accent, Color.White, 0.45f),
            LoginGradientTop = isDark ? Mix(background, Color.White, 0.03f) : Mix(background, Color.White, 0.5f),
            LoginGradientBottom = isDark ? Mix(background, Color.Black, 0.15f) : Mix(surfaceLow, primary, 0.1f),
            LoginCardFill = isDark ? surface : Color.FromArgb(253, 255, 254),
            LoginCardBorder = isDark ? outlineVar : Mix(borderSoft, primary, 0.2f),
            LoginOverlayScrim = isDark ? Color.FromArgb(170, 10, 12, 11) : Color.FromArgb(170, 232, 248, 240),
            LoginErrorSurface = isDark ? Color.FromArgb(42, 32, 30) : Color.FromArgb(255, 251, 249),
            LoginErrorBorder = isDark ? Color.FromArgb(90, 52, 50) : Color.FromArgb(232, 200, 200),
            LoginRevealHover = isDark ? Mix(surfaceHigh, accent, 0.15f) : Mix(surfaceLow, accent, 0.25f)
        };
    }

    private static readonly ThemeSnapshot[] Themes =
    [
        Medical(
            "#076443", "#2D7D5A", "#E7FFF1", "#FFFFFF",
            "#DCF4E5", "#D6EEE0", "#D0E8DA",
            "#0B1F17", "#3F4943", "#88D7AD", isDark: false),

        Medical(
            "#0B5CAD", "#1976D2", "#EAF4FF", "#FFFFFF",
            "#DCEEFF", "#CFE6FF", "#BDD9F7",
            "#082033", "#415466", "#64B5F6", isDark: false),

        Medical(
            "#006D6F", "#168C8E", "#E6FAF8", "#FFFFFF",
            "#D7F3F0", "#C8ECE8", "#B8E1DD",
            "#062524", "#3E5B59", "#5DCCC8", isDark: false),

        Medical(
            "#237A57", "#3E9C74", "#F0FFF7", "#FFFFFF",
            "#E1F8EC", "#D3F0E1", "#C3E6D4",
            "#10241A", "#4B6255", "#7ED9A6", isDark: false),

        Medical(
            "#426B5A", "#5C8A74", "#F5FAF7", "#FFFFFF",
            "#E9F1EC", "#DDE8E2", "#D1DDD6",
            "#18231E", "#56645D", "#A7C9B7", isDark: false),

        Medical(
            "#7EE0B5", "#1F6F55", "#101815", "#17211D",
            "#1D2A25", "#263832", "#30443D",
            "#E7FFF1", "#B7C9C0", "#88D7AD", isDark: true)
    ];
}
