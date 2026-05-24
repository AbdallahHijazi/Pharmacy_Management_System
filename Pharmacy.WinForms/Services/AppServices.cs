namespace Pharmacy.WinForms.Services;

internal static class AppServices
{
    public static ApiClient ApiClient { get; } = new();
    public static AuthService AuthService { get; } = new(ApiClient);
    public static DashboardService DashboardService { get; } = new(ApiClient);
    public static SettingsService SettingsService { get; } = new(ApiClient);
    public static PointOfSaleService PointOfSaleService { get; } = new(ApiClient);
    public static InventoryService InventoryService { get; } = new(ApiClient);
    public static PurchaseService PurchaseService { get; } = new(ApiClient);
    public static CustomerService CustomerService { get; } = new(ApiClient);
    public static SupplierService SupplierService { get; } = new(ApiClient);
}
