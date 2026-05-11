using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Infrastructure.Persistence
{
    public sealed class StockBatchConcurrencyRetryPolicy : IStockBatchConcurrencyRetryPolicy
    {
        private readonly AppDbContext _context;
        private const int MaxAttempts = 3;

        public StockBatchConcurrencyRetryPolicy(AppDbContext context)
        {
            _context = context;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
        {
            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == MaxAttempts - 1)
                    {
                        throw new BadRequestException(
                            "تعارض تحديث المخزون (بيع متزامن على نفس الدفعة). يرجى إعادة المحاولة.");
                    }

                    _context.ChangeTracker.Clear();
                }
            }

            throw new InvalidOperationException("Stock batch concurrency retry policy exited without result.");
        }
    }
}
