namespace Pharmacy.Application.Common.Interfaces
{
    /// <summary>
    /// Retries an operation when EF optimistic concurrency fails on <see cref="Pharmacy.Domain.Entities.Catalog.StockBatch"/> updates.
    /// </summary>
    public interface IStockBatchConcurrencyRetryPolicy
    {
        Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
    }
}
