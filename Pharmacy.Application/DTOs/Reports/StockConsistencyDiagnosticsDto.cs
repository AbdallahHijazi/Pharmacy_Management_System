namespace Pharmacy.Application.DTOs.Reports
{
    public sealed class StockConsistencyDiagnosticsDto
    {
        public bool HasIssues { get; set; }
        public IReadOnlyList<StockBatchConsistencyIssueDto> BatchIssues { get; set; } = Array.Empty<StockBatchConsistencyIssueDto>();
        public IReadOnlyList<ProductStockConsistencyIssueDto> ProductIssues { get; set; } = Array.Empty<ProductStockConsistencyIssueDto>();
    }

    public sealed class StockBatchConsistencyIssueDto
    {
        public Guid StockBatchId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public string IssueCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
    }

    public sealed class ProductStockConsistencyIssueDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string IssueCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int TotalAvailableQuantity { get; set; }
        public int TotalReceivedQuantity { get; set; }
        public int BatchCount { get; set; }
    }
}
