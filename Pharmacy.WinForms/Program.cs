using Pharmacy.WinForms.Forms;
using Pharmacy.WinForms.Services;

namespace Pharmacy.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new LoginForm(AppServices.AuthService));
    }
}
