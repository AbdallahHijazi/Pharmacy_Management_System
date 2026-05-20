using Pharmacy.WinForms.Forms;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var boot = LocalAppSettingsStore.LoadOrDefault();
        UiBranding.InitializeFromLocal(boot);
        ThemeManager.ApplyThemeIndex(boot.ThemeIndex);
        FontScaleManager.SetLevel(boot.FontSizeLevel);

        Application.Run(new LoginForm(AppServices.AuthService));
    }
}
