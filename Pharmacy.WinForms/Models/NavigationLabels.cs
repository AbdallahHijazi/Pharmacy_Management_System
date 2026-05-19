namespace Pharmacy.WinForms.Models;

internal static class NavigationLabels
{
    public static string Get(AppNavigation navigation) => navigation switch
    {
        AppNavigation.Dashboard => "لوحة التحكم",
        AppNavigation.Inventory => "المخزون",
        AppNavigation.PointOfSale => "نقطة البيع",
        AppNavigation.Purchases => "المشتريات",
        AppNavigation.Customers => "الزبائن",
        AppNavigation.Suppliers => "الموردين",
        AppNavigation.Reports => "التقارير",
        AppNavigation.Users => "المستخدمين",
        AppNavigation.Settings => "الإعدادات",
        _ => "الصفحة"
    };
}
