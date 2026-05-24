using Pharmacy.WinForms.Models;
using Pharmacy.WinForms.Ui;

namespace Pharmacy.WinForms.Services;

internal sealed class ReportsService
{
    private const int DebtsPageSize = 50;
    private const int MaxDebtPages = 10;

    private readonly ApiClient _apiClient;

    public ReportsService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ReportLoadResult> LoadReportAsync(ReportKind kind, CancellationToken cancellationToken = default) =>
        kind switch
        {
            ReportKind.Sales => LoadSalesReportAsync(cancellationToken),
            ReportKind.TopSellingProducts => LoadTopSellingProductsAsync(cancellationToken),
            ReportKind.ProfitLoss => LoadProfitLossReportAsync(cancellationToken),
            ReportKind.ExpiringMedicines => LoadExpiringMedicinesReportAsync(cancellationToken),
            ReportKind.CustomerDebts => LoadCustomerDebtsReportAsync(cancellationToken),
            ReportKind.SupplierPayables => LoadSupplierPayablesReportAsync(cancellationToken),
            _ => Task.FromResult(Unavailable("التقرير غير معروف."))
        };

    private async Task<ReportLoadResult> LoadSalesReportAsync(CancellationToken cancellationToken)
    {
        _apiClient.EnsureSessionAuthorization();
        var to = DateTime.UtcNow.Date;
        var from = to.AddDays(-30);
        var url =
            $"api/v1/reports/sales?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}";

        var result = await _apiClient.GetAsync<SalesReportApiModel>(
            url, "reports/sales", cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return Fail(result.ErrorMessage, result.IsConnectionError);
        }

        if (result.Data is null)
        {
            return Fail("استجابة غير صالحة من الخادم.");
        }

        var data = result.Data;
        return new ReportLoadResult
        {
            Success = true,
            PeriodText = $"من {ReportDisplayHelper.FormatDate(from)} إلى {ReportDisplayHelper.FormatDate(to)}",
            Content = new ReportDetailsContentView
            {
                Summary =
                [
                    ("عدد الفواتير", ReportDisplayHelper.FormatQuantity(data.TotalInvoices)),
                    ("إجمالي المبيعات", ReportDisplayHelper.FormatMoney(data.TotalSales)),
                    ("المدفوع", ReportDisplayHelper.FormatMoney(data.TotalPaid)),
                    ("المتبقي", ReportDisplayHelper.FormatMoney(data.TotalRemaining))
                ]
            }
        };
    }

    private async Task<ReportLoadResult> LoadTopSellingProductsAsync(CancellationToken cancellationToken)
    {
        _apiClient.EnsureSessionAuthorization();
        var result = await _apiClient.GetAsync<List<TopSellingProductApiModel>>(
            "api/v1/dashboard/top-selling-products",
            "reports/top-selling",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return Fail(result.ErrorMessage, result.IsConnectionError);
        }

        var rows = result.Data ?? [];
        return new ReportLoadResult
        {
            Success = true,
            PeriodText = "أعلى الأصناف مبيعًا في الفرع",
            Content = new ReportDetailsContentView
            {
                TableHeaders = ["المنتج", "الكمية المباعة", "إجمالي المبيعات"],
                TableRows = rows.Select(r => new ReportDetailsRowView
                {
                    Cells =
                    [
                        r.ProductName,
                        ReportDisplayHelper.FormatQuantity(r.TotalSoldQuantity),
                        ReportDisplayHelper.FormatMoney(r.TotalSalesAmount)
                    ]
                }).ToList(),
                EmptyMessage = rows.Count == 0 ? "لا توجد بيانات مبيعات للأصناف." : string.Empty
            }
        };
    }

    private async Task<ReportLoadResult> LoadProfitLossReportAsync(CancellationToken cancellationToken)
    {
        _apiClient.EnsureSessionAuthorization();
        var now = DateTime.UtcNow;
        var url =
            $"api/v1/reports/financial/monthly?year={now.Year}&month={now.Month}";

        var result = await _apiClient.GetAsync<MonthlyFinancialReportApiModel>(
            url, "reports/financial/monthly", cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return Fail(result.ErrorMessage, result.IsConnectionError);
        }

        if (result.Data?.Profit is null)
        {
            return Fail("استجابة غير صالحة من الخادم.");
        }

        var profit = result.Data.Profit;
        return new ReportLoadResult
        {
            Success = true,
            PeriodText = ReportDisplayHelper.FormatMonthPeriod(result.Data.Year, result.Data.Month),
            Content = new ReportDetailsContentView
            {
                Summary =
                [
                    ("عدد فواتير البيع", ReportDisplayHelper.FormatQuantity(profit.SalesInvoiceCount)),
                    ("صافي المبيعات", ReportDisplayHelper.FormatMoney(profit.NetSalesAfterReturns)),
                    ("تكلفة البضاعة", ReportDisplayHelper.FormatMoney(profit.NetCogs)),
                    ("إجمالي الربح", ReportDisplayHelper.FormatMoney(profit.GrossProfit)),
                    ("صافي الربح", ReportDisplayHelper.FormatMoney(profit.NetProfit)),
                    ("هامش الربح", ReportDisplayHelper.FormatPercent(profit.GrossProfitMarginPercent))
                ]
            }
        };
    }

    private async Task<ReportLoadResult> LoadExpiringMedicinesReportAsync(CancellationToken cancellationToken)
    {
        _apiClient.EnsureSessionAuthorization();

        var inventoryTask = _apiClient.GetAsync<InventoryReportApiModel>(
            "api/v1/reports/inventory", "reports/inventory", cancellationToken);
        var batchesTask = _apiClient.GetAsync<List<ExpiringSoonBatchApiModel>>(
            "api/v1/dashboard/expiring-soon-batches", "reports/expiring", cancellationToken);

        await Task.WhenAll(inventoryTask, batchesTask).ConfigureAwait(false);
        var inventory = await inventoryTask.ConfigureAwait(false);
        var batches = await batchesTask.ConfigureAwait(false);

        if (!inventory.Success && !batches.Success)
        {
            return Fail(
                inventory.ErrorMessage ?? batches.ErrorMessage,
                inventory.IsConnectionError || batches.IsConnectionError);
        }

        var inv = inventory.Data;
        var list = batches.Data ?? [];

        var summary = new List<(string, string)>();
        if (inv is not null)
        {
            summary.Add(("دفعات قريبة الانتهاء", ReportDisplayHelper.FormatQuantity(inv.ExpiringSoonBatchesCount)));
            summary.Add(("دفعات منتهية", ReportDisplayHelper.FormatQuantity(inv.ExpiredBatchesCount)));
            summary.Add(("دفعات مخزون منخفض", ReportDisplayHelper.FormatQuantity(inv.LowStockBatchesCount)));
        }

        return new ReportLoadResult
        {
            Success = true,
            PeriodText = "قريبة الانتهاء خلال 30 يومًا",
            Content = new ReportDetailsContentView
            {
                Summary = summary,
                TableHeaders = ["المنتج", "التشغيلة", "تاريخ الصلاحية", "الكمية"],
                TableRows = list.Select(b => new ReportDetailsRowView
                {
                    Cells =
                    [
                        b.ProductName,
                        string.IsNullOrWhiteSpace(b.BatchNumber) ? "—" : b.BatchNumber,
                        ReportDisplayHelper.FormatDate(b.ExpiryDate),
                        ReportDisplayHelper.FormatQuantity(b.AvailableQuantity)
                    ]
                }).ToList(),
                EmptyMessage = list.Count == 0
                    ? "لا توجد دفعات قريبة من الانتهاء. (قائمة المنتهية غير متوفرة من API حاليًا.)"
                    : string.Empty
            }
        };
    }

    private async Task<ReportLoadResult> LoadCustomerDebtsReportAsync(CancellationToken cancellationToken)
    {
        _apiClient.EnsureSessionAuthorization();

        var debtors = new List<CustomerListItemApiModel>();
        decimal totalDebt = 0;
        var page = 1;

        while (page <= MaxDebtPages)
        {
            var url =
                $"api/v1/customers?pageNumber={page}&pageSize={DebtsPageSize}&sortBy=debtamount&sortDirection=desc";
            var result = await _apiClient.GetAsync<PagedCustomersApiModel>(
                url, "reports/customer-debts", cancellationToken).ConfigureAwait(false);

            if (!result.Success || result.Data is null)
            {
                return Fail(result.ErrorMessage, result.IsConnectionError);
            }

            var withDebt = result.Data.Items.Where(c => c.DebtAmount > 0).ToList();
            debtors.AddRange(withDebt);
            totalDebt += withDebt.Sum(c => c.DebtAmount);

            if (result.Data.Items.Count == 0 || page * DebtsPageSize >= result.Data.TotalCount)
            {
                break;
            }

            if (withDebt.Count == 0 && result.Data.Items.All(c => c.DebtAmount <= 0))
            {
                break;
            }

            page++;
        }

        return new ReportLoadResult
        {
            Success = true,
            PeriodText = "ذمم الزبائن الحالية",
            Content = new ReportDetailsContentView
            {
                Summary =
                [
                    ("عدد الزبائن المدينين", ReportDisplayHelper.FormatQuantity(debtors.Count)),
                    ("إجمالي الديون", ReportDisplayHelper.FormatMoney(totalDebt))
                ],
                TableHeaders = ["الزبون", "الهاتف", "الدين"],
                TableRows = debtors.Select(c => new ReportDetailsRowView
                {
                    Cells =
                    [
                        CustomerDisplayHelper.ResolveDisplayName(c.FullName),
                        CustomerDisplayHelper.ResolvePhone(c.Phone),
                        ReportDisplayHelper.FormatMoney(c.DebtAmount)
                    ]
                }).ToList(),
                EmptyMessage = debtors.Count == 0 ? "لا توجد ديون على الزبائن." : string.Empty
            }
        };
    }

    private async Task<ReportLoadResult> LoadSupplierPayablesReportAsync(CancellationToken cancellationToken)
    {
        _apiClient.EnsureSessionAuthorization();

        var suppliers = new List<SupplierListItemApiModel>();
        decimal totalPayable = 0;
        var page = 1;

        while (page <= MaxDebtPages)
        {
            var url =
                $"api/v1/suppliers?pageNumber={page}&pageSize={DebtsPageSize}&sortBy=payableamount&sortDirection=desc";
            var result = await _apiClient.GetAsync<PagedSupplierItemsApiModel>(
                url, "reports/supplier-payables", cancellationToken).ConfigureAwait(false);

            if (!result.Success || result.Data is null)
            {
                return Fail(result.ErrorMessage, result.IsConnectionError);
            }

            var withPayable = result.Data.Items.Where(s => s.PayableAmount > 0).ToList();
            suppliers.AddRange(withPayable);
            totalPayable += withPayable.Sum(s => s.PayableAmount);

            if (result.Data.Items.Count == 0 || page * DebtsPageSize >= result.Data.TotalCount)
            {
                break;
            }

            if (withPayable.Count == 0 && result.Data.Items.All(s => s.PayableAmount <= 0))
            {
                break;
            }

            page++;
        }

        return new ReportLoadResult
        {
            Success = true,
            PeriodText = "مستحقات الموردين الحالية",
            Content = new ReportDetailsContentView
            {
                Summary =
                [
                    ("عدد الموردين", ReportDisplayHelper.FormatQuantity(suppliers.Count)),
                    ("إجمالي المستحقات", ReportDisplayHelper.FormatMoney(totalPayable))
                ],
                TableHeaders = ["المورد", "الهاتف", "المستحقات"],
                TableRows = suppliers.Select(s => new ReportDetailsRowView
                {
                    Cells =
                    [
                        SupplierDisplayHelper.ResolveSupplierDisplayName(s.Name),
                        SupplierDisplayHelper.ResolvePhone(s.Phone),
                        ReportDisplayHelper.FormatMoney(s.PayableAmount)
                    ]
                }).ToList(),
                EmptyMessage = suppliers.Count == 0 ? "لا توجد مستحقات على الموردين." : string.Empty
            }
        };
    }

    private static ReportLoadResult Fail(string? message, bool isConnection = false) =>
        new()
        {
            Success = false,
            ErrorMessage = message ?? "تعذر تحميل التقرير.",
            IsConnectionError = isConnection
        };

    private static ReportLoadResult Unavailable(string message) =>
        new()
        {
            Success = false,
            IsAvailable = false,
            ErrorMessage = message
        };
}
