using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Forms.Dashboard;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;

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

            MessageBox.Show(
                this,
                $"البحث عن \"{query}\" سيتوفر عند ربط الوحدات.",
                "بحث",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        };

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
            AppNavigation.Inventory => new PlaceholderPageControl("المخزون"),
            AppNavigation.PointOfSale => new PlaceholderPageControl("نقطة البيع"),
            AppNavigation.Purchases => new PlaceholderPageControl("المشتريات"),
            AppNavigation.Customers => new PlaceholderPageControl("الزبائن"),
            AppNavigation.Suppliers => new PlaceholderPageControl("الموردين"),
            AppNavigation.Reports => new PlaceholderPageControl("التقارير"),
            AppNavigation.Users => new PlaceholderPageControl("المستخدمين"),
            AppNavigation.Settings => new PlaceholderPageControl("الإعدادات"),
            _ => new PlaceholderPageControl("الصفحة")
        };

        _pages[navigation] = page;
        return page;
    }

    private DashboardControl CreateDashboard()
    {
        _dashboard = new DashboardControl();
        _dashboard.QuickActionRequested += (_, action) =>
        {
            MessageBox.Show(
                this,
                $"الاختصار \"{action}\" سيتوفر عند ربط الوحدة.",
                "اختصار سريع",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        };
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
