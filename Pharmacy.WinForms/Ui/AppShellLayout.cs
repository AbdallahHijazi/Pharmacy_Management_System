namespace Pharmacy.WinForms.Ui;

/// <summary>
/// Shell layout contract for <see cref="Forms.MainForm"/>.
/// Sidebar and TopBar are created once in MainForm; pages host content only inside contentHost.
/// </summary>
internal static class AppShellLayout
{
    public const int SidebarColumnWidth = PharmaTheme.ShellSidebarWidth;
    public const int TopBarHeight = PharmaTheme.ShellTopBarHeight;
}
