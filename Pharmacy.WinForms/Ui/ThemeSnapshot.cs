using System.Drawing;

namespace Pharmacy.WinForms.Ui;

/// <summary>Full color set for one app theme (shell + content + login).</summary>
internal sealed class ThemeSnapshot
{
    public required Color Primary { get; init; }
    public required Color PrimaryDark { get; init; }
    public required Color PrimaryLight { get; init; }
    public required Color Background { get; init; }
    public required Color Surface { get; init; }
    public required Color SurfaceAlt { get; init; }
    public required Color Border { get; init; }
    public required Color Text { get; init; }
    public required Color MutedText { get; init; }
    public required Color OnPrimary { get; init; }

    public required Color PrimaryGreen { get; init; }
    public required Color PrimaryContainer { get; init; }
    public required Color Success { get; init; }
    public required Color AccentTeal { get; init; }
    public required Color SoftGreenBackground { get; init; }
    public required Color CardBackground { get; init; }
    public required Color SurfaceContainerLowest { get; init; }
    public required Color SurfaceContainerLow { get; init; }
    public required Color SurfaceContainer { get; init; }
    public required Color SurfaceContainerHigh { get; init; }
    public required Color SurfaceContainerHighest { get; init; }
    public required Color OutlineVariant { get; init; }
    public required Color OnSurfaceVariant { get; init; }
    public required Color InputSurface { get; init; }
    public required Color TextDark { get; init; }
    public required Color SidebarBackground { get; init; }
    public required Color SidebarHover { get; init; }
    public required Color SidebarActive { get; init; }
    public required Color SidebarLightBackground { get; init; }
    public required Color SidebarNavHoverFill { get; init; }
    public required Color SidebarDivider { get; init; }
    public required Color BorderLight { get; init; }
    public required Color BorderSoft { get; init; }
    public required Color Danger { get; init; }
    public required Color Warning { get; init; }
    public required Color WarningStrong { get; init; }
    public required Color WarningSurface { get; init; }
    public required Color SuccessSurface { get; init; }
    public required Color ErrorContainer { get; init; }
    public required Color TopBarGradientDeep { get; init; }
    public required Color DashboardCardShadow { get; init; }
    public required Color PrimaryFixed { get; init; }

    public required Color LoginGradientTop { get; init; }
    public required Color LoginGradientBottom { get; init; }
    public required Color LoginCardFill { get; init; }
    public required Color LoginCardBorder { get; init; }
    public required Color LoginOverlayScrim { get; init; }
    public required Color LoginErrorSurface { get; init; }
    public required Color LoginErrorBorder { get; init; }
    public required Color LoginRevealHover { get; init; }
}
