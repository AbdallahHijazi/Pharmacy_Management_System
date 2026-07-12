using Pharmacy.WinForms.Controls;
using Pharmacy.WinForms.Forms.Customers;
using Pharmacy.WinForms.Forms.Dashboard;
using Pharmacy.WinForms.Forms.Inventory;
using Pharmacy.WinForms.Forms.PointOfSale;
using Pharmacy.WinForms.Forms.Purchases;
using Pharmacy.WinForms.Forms.Reports;
using Pharmacy.WinForms.Forms.Settings;
using Pharmacy.WinForms.Forms.Suppliers;
using Pharmacy.WinForms.Forms.Users;
using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Services;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Forms;

/// <summary>
/// Application shell: one <see cref="SidebarControl"/> and one <see cref="TopBarControl"/>.
/// Navigation swaps only the child inside <c>contentHost</c>.
/// </summary>
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
        ApplyGlobalChrome();
    }

    private void ApplyGlobalChrome()
    {
        if (IsDisposed)
        {
            return;
        }

        ApplyShellChrome();
        _dashboard?.RefreshVisualTheme();

        foreach (var page in _pages.Values)
        {
            switch (page)
            {
                case SettingsControl settings:
                    settings.ApplyThemeAndFontVisuals();
                    break;
                case PointOfSaleControl pointOfSale:
                    pointOfSale.ApplyThemeVisuals();
                    break;
                case InventoryControl inventory:
                    inventory.ApplyThemeVisuals();
                    break;
                case PurchasesControl purchases:
                    purchases.ApplyThemeVisuals();
                    break;
                case CustomersControl customers:
                    customers.ApplyThemeVisuals();
                    break;
                case SuppliersControl suppliers:
                    suppliers.ApplyThemeVisuals();
                    break;
                case ReportsControl reports:
                    reports.ApplyThemeVisuals();
                    break;
                case UsersControl users:
                    users.ApplyThemeVisuals();
                    break;
                case PlaceholderPageControl placeholder:
                    placeholder.ApplyThemeVisuals();
                    break;
            }
        }

        ThemeApplier.ApplyThemeRecursive(contentHost);
        Invalidate(true);
        Refresh();
    }

    /// <summary>Theme refresh for fixed shell chrome only (sidebar + top bar + hosts).</summary>
    private void ApplyShellChrome()
    {
        BackColor = PharmaTheme.Background;
        Font = PharmaTheme.BodyFont;
        shellLayout.BackColor = PharmaTheme.Background;
        mainShell.BackColor = PharmaTheme.Background;
        contentHost.BackColor = PharmaTheme.Background;
        sidebar.RefreshChrome();
        topBar.RefreshChrome();
    }

    private void OnGlobalThemeOrFontChanged(object? sender, EventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(new Action(ApplyGlobalChrome));
            return;
        }

        ApplyGlobalChrome();
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

        ThemeManager.ThemeChanged += OnGlobalThemeOrFontChanged;
        FontScaleManager.Changed += OnGlobalThemeOrFontChanged;

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

        if (page is SettingsControl settings)
        {
            settings.ApplyThemeAndFontVisuals();
        }
        else if (page is PointOfSaleControl pointOfSale)
        {
            pointOfSale.ApplyThemeVisuals();
        }
        else if (page is InventoryControl inventory)
        {
            inventory.ApplyThemeVisuals();
        }
        else if (page is PurchasesControl purchases)
        {
            purchases.ApplyThemeVisuals();
        }
        else if (page is CustomersControl customers)
        {
            customers.ApplyThemeVisuals();
        }
        else if (page is SuppliersControl suppliers)
        {
            suppliers.ApplyThemeVisuals();
        }
        else if (page is ReportsControl reports)
        {
            reports.ApplyThemeVisuals();
        }
        else if (page is UsersControl users)
        {
            users.ApplyThemeVisuals();
        }
        else if (page is PlaceholderPageControl placeholder)
        {
            placeholder.ApplyThemeVisuals();
        }

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
            AppNavigation.Inventory => new InventoryControl(),
            AppNavigation.PointOfSale => new PointOfSaleControl(),
            AppNavigation.Purchases => new PurchasesControl(),
            AppNavigation.Customers => new CustomersControl(),
            AppNavigation.Suppliers => new SuppliersControl(),
            AppNavigation.Reports => new ReportsControl(),
            AppNavigation.Users => new UsersControl(),
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
            ThemeManager.ThemeChanged -= OnGlobalThemeOrFontChanged;
            FontScaleManager.Changed -= OnGlobalThemeOrFontChanged;
            components?.Dispose();
            foreach (var page in _pages.Values)
            {
                page.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
