using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Forms.Dashboard;
using Pharmacy.WinForms.Forms.Settings;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms;

public sealed partial class MainForm : Form
{
    private readonly AuthService _authService;
    private readonly Dictionary<AppNavigation, Control> _pages = new();
    private Control? _activePage;
    private DashboardControl? _dashboard;

    public MainForm(AuthService authService)
    {
        _authService = authService;
        InitializeComponent();
        AppServices.ApiClient.EnsureSessionAuthorization();
        topBar.BindUser();
        WireEvents();
        ShowPage(AppNavigation.Dashboard);
    }

    private void WireEvents()
    {
        sidebar.NavigationRequested += (_, navigation) => ShowPage(navigation);
        sidebar.LogoutRequested += (_, _) => RequestLogout();
        topBar.LogoutRequested += (_, _) => RequestLogout();
        topBar.SearchSubmitted += (_, query) =>
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return;
            }

            UiFeedback.ShowFeatureNotAvailable(this, "البحث العام في النظام");
        };

        topBar.NotificationsClicked += (_, _) => UiFeedback.ShowFeatureNotAvailable(this, "التنبيهات");
        topBar.ThemeToggleRequested += (_, _) => UiFeedback.ShowFeatureNotAvailable(this, "تبديل المظهر في التطبيق");
        topBar.AccountClicked += (_, _) => UiFeedback.ShowFeatureNotAvailable(this, "صفحة الحساب");

        FormClosed += (_, _) => _authService.Logout();
    }

    private void ShowPage(AppNavigation navigation)
    {
        sidebar.SetActive(navigation);
        var page = GetOrCreatePage(navigation);
        if (_activePage == page)
        {
            return;
        }

        contentHost.SuspendLayout();
        contentHost.Controls.Clear();
        contentHost.Controls.Add(page);
        page.Dock = DockStyle.Fill;
        contentHost.ResumeLayout(true);
        _activePage = page;

        Text = navigation switch
        {
            AppNavigation.Dashboard => "PharmaCare — لوحة التحكم",
            AppNavigation.Inventory => "PharmaCare — المخزون",
            AppNavigation.PointOfSale => "PharmaCare — نقطة البيع",
            AppNavigation.Purchases => "PharmaCare — المشتريات",
            AppNavigation.Customers => "PharmaCare — الزبائن",
            AppNavigation.Suppliers => "PharmaCare — الموردين",
            AppNavigation.Reports => "PharmaCare — التقارير",
            AppNavigation.Users => "PharmaCare — المستخدمين",
            AppNavigation.Settings => "PharmaCare — الإعدادات",
            _ => "PharmaCare"
        };
    }

    private Control GetOrCreatePage(AppNavigation navigation)
    {
        if (_pages.TryGetValue(navigation, out var existing))
        {
            return existing;
        }

        Control page = navigation switch
        {
            AppNavigation.Dashboard => CreateDashboard(),
            AppNavigation.Inventory => new PlaceholderPageControl(NavigationLabels.Get(AppNavigation.Inventory)),
            AppNavigation.PointOfSale => new PlaceholderPageControl(NavigationLabels.Get(AppNavigation.PointOfSale)),
            AppNavigation.Purchases => new PlaceholderPageControl(NavigationLabels.Get(AppNavigation.Purchases)),
            AppNavigation.Customers => new PlaceholderPageControl(NavigationLabels.Get(AppNavigation.Customers)),
            AppNavigation.Suppliers => new PlaceholderPageControl(NavigationLabels.Get(AppNavigation.Suppliers)),
            AppNavigation.Reports => new PlaceholderPageControl(NavigationLabels.Get(AppNavigation.Reports)),
            AppNavigation.Users => new PlaceholderPageControl(NavigationLabels.Get(AppNavigation.Users)),
            AppNavigation.Settings => new SettingsControl(),
            _ => new PlaceholderPageControl("الصفحة")
        };

        _pages[navigation] = page;
        return page;
    }

    private DashboardControl CreateDashboard()
    {
        _dashboard = new DashboardControl();
        _dashboard.QuickActionRequested += (_, action) =>
            UiFeedback.ShowFeatureNotAvailable(this, action);
        return _dashboard;
    }

    private void RequestLogout()
    {
        var confirm = MessageBox.Show(
            this,
            "هل تريد تسجيل الخروج؟",
            "تسجيل الخروج",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (confirm == DialogResult.Yes)
        {
            _authService.Logout();
            Close();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            foreach (var page in _pages.Values)
            {
                page.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
