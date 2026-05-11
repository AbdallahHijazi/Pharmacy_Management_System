namespace Pharmacy.Application.Common.Accounting
{
    public static class FinancialSaleMetrics
    {
        /// <summary>توزيع خصم الفاتورة على السطر بنسبة الإجمالي قبل الخصم.</summary>
        public static decimal AllocatedLineDiscount(decimal invoiceSubtotal, decimal invoiceDiscount, decimal lineSubtotal) =>
            invoiceSubtotal > 0 ? decimal.Round(invoiceDiscount * lineSubtotal / invoiceSubtotal, 4, MidpointRounding.AwayFromZero) : 0m;
    }
}
