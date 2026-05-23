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
        Themes[Math.Clamp(index, 0, Themes.Length - 1)].Primary;

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

    private static Color Mix(Color a, Color b, float t)
    {
        static int C(float v) => (int)Math.Clamp(v, 0, 255);
        return Color.FromArgb(
            C(a.R + (b.R - a.R) * t),
            C(a.G + (b.G - a.G) * t),
            C(a.B + (b.B - a.B) * t));
    }

    private static ThemeSnapshot Medical(
        string primaryHex,
        string primaryDarkHex,
        string primaryLightHex,
        string backgroundHex,
        string surfaceHex,
        string surfaceAltHex,
        string borderHex,
        string textHex,
        string mutedHex,
        bool isDark)
    {
        var primary = Hex(primaryHex);
        var primaryDark = Hex(primaryDarkHex);
        var primaryLight = Hex(primaryLightHex);
        var background = Hex(backgroundHex);
        var surface = Hex(surfaceHex);
        var surfaceAlt = Hex(surfaceAltHex);
        var border = Hex(borderHex);
        var text = Hex(textHex);
        var muted = Hex(mutedHex);

        var surfaceHigh = isDark ? Mix(surfaceAlt, border, 0.35f) : Mix(surfaceAlt, border, 0.28f);
        var surfaceHighest = isDark ? Mix(surface, border, 0.42f) : Mix(surface, border, 0.18f);
        var input = isDark ? surfaceHigh : Mix(surface, surfaceAlt, 0.45f);
        var onPrimary = isDark ? text : Hex("#FFFFFF");

        var sidebarBg = isDark ? Mix(background, primary, 0.28f) : primaryDark;
        var sidebarHover = isDark ? Mix(sidebarBg, primary, 0.12f) : Mix(sidebarBg, primary, 0.08f);
        var sidebarActive = isDark ? Mix(sidebarBg, primary, 0.22f) : Mix(primary, primaryDark, 0.35f);
        var sidebarLight = isDark ? Mix(background, surface, 0.35f) : Mix(background, surface, 0.72f);
        var navHover = isDark ? Mix(surfaceAlt, primary, 0.2f) : Mix(surfaceAlt, primary, 0.22f);
        var divider = isDark ? Mix(border, text, 0.12f) : Mix(border, muted, 0.35f);
        var borderLight = isDark ? Mix(surfaceHigh, text, 0.06f) : Mix(border, background, 0.4f);
        var topDeep = isDark
            ? Color.FromArgb(255, background.R, background.G, background.B)
            : Color.FromArgb(255, Mix(background, primary, 0.12f).R, Mix(background, primary, 0.12f).G, Mix(background, primary, 0.12f).B);
        var shadow = Color.FromArgb(isDark ? 48 : 12, primary.R, primary.G, primary.B);

        return new ThemeSnapshot
        {
            Primary = primary,
            PrimaryDark = primaryDark,
            PrimaryLight = primaryLight,
            Background = background,
            Surface = surface,
            SurfaceAlt = surfaceAlt,
            Border = border,
            Text = text,
            MutedText = muted,
            OnPrimary = onPrimary,

            PrimaryGreen = primary,
            PrimaryContainer = primaryLight,
            Success = isDark ? Mix(primary, Hex("#7EE0B5"), 0.35f) : Hex("#0F7A55"),
            AccentTeal = primary,
            SoftGreenBackground = background,
            CardBackground = surface,
            SurfaceContainerLowest = surface,
            SurfaceContainerLow = surfaceAlt,
            SurfaceContainer = surfaceAlt,
            SurfaceContainerHigh = surfaceHigh,
            SurfaceContainerHighest = surfaceHighest,
            OutlineVariant = border,
            OnSurfaceVariant = muted,
            InputSurface = input,
            TextDark = text,
            SidebarBackground = sidebarBg,
            SidebarHover = sidebarHover,
            SidebarActive = sidebarActive,
            SidebarLightBackground = sidebarLight,
            SidebarNavHoverFill = navHover,
            SidebarDivider = divider,
            BorderLight = borderLight,
            BorderSoft = border,
            Danger = Hex("#BA1A1A"),
            Warning = Hex("#F59E0B"),
            WarningStrong = Hex("#D97706"),
            WarningSurface = isDark ? Hex("#302A1C") : Hex("#FFFBEB"),
            SuccessSurface = isDark ? Mix(surfaceAlt, primary, 0.22f) : Mix(surfaceAlt, primary, 0.32f),
            ErrorContainer = isDark ? Hex("#34201E") : Hex("#FFDAD6"),
            TopBarGradientDeep = topDeep,
            DashboardCardShadow = shadow,
            PrimaryFixed = isDark ? Mix(primaryLight, text, 0.25f) : Mix(primaryLight, primary, 0.35f),
            LoginGradientTop = isDark ? Mix(background, text, 0.04f) : Mix(background, surface, 0.55f),
            LoginGradientBottom = isDark ? Mix(background, primaryDark, 0.25f) : Mix(surfaceAlt, primary, 0.08f),
            LoginCardFill = isDark ? surface : Mix(surface, Hex("#FDFFFE"), 0.5f),
            LoginCardBorder = isDark ? border : Mix(border, primary, 0.18f),
            LoginOverlayScrim = isDark ? Color.FromArgb(170, background.R, background.G, background.B) : Color.FromArgb(170, Mix(background, primaryLight, 0.35f)),
            LoginErrorSurface = isDark ? Hex("#2A201E") : Hex("#FFFBF9"),
            LoginErrorBorder = isDark ? Hex("#5A3432") : Hex("#E8C8C8"),
            LoginRevealHover = isDark ? Mix(surfaceHigh, primary, 0.18f) : Mix(surfaceAlt, primary, 0.22f)
        };
    }

    private static readonly ThemeSnapshot[] Themes =
    [
        Medical(
            "#0F7A55", "#075E43", "#DDF7EA",
            "#F3FFF8", "#FFFFFF", "#E8F7EF", "#C6E3D4",
            "#10231B", "#5F766B", isDark: false),

        Medical(
            "#1565C0", "#0D47A1", "#E3F2FD",
            "#F5FAFF", "#FFFFFF", "#EAF4FF", "#C7DDF3",
            "#102033", "#5D7085", isDark: false),

        Medical(
            "#00796B", "#005B50", "#DDF7F3",
            "#F2FFFC", "#FFFFFF", "#E5F7F3", "#BFE3DC",
            "#0E2724", "#5E7773", isDark: false),

        Medical(
            "#2E8B68", "#1F6B50", "#E4F8EE",
            "#FAFFFC", "#FFFFFF", "#EEF8F3", "#D0E7DA",
            "#15251E", "#6D7D74", isDark: false),

        Medical(
            "#447260", "#2E5546", "#EEF5F1",
            "#FBFCFA", "#FFFFFF", "#F2F5F1", "#D8DED8",
            "#202620", "#6F776F", isDark: false),

        Medical(
            "#49C6A3", "#2FAE8C", "#123B34",
            "#0F1F1C", "#172B27", "#1E3833", "#31534C",
            "#EAF7F3", "#A9C4BD", isDark: true)
    ];
}
