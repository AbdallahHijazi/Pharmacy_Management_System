using MediatR;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Reports;
using Pharmacy.Application.Features.Reports.Queries.GetBranchProfitReport;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Features.Reports.Queries.GetMonthlyFinancialReport
{
    public sealed class GetMonthlyFinancialReportQueryHandler : IRequestHandler<GetMonthlyFinancialReportQuery, MonthlyFinancialReportDto>
    {
        private readonly ISender _mediator;
        private readonly ICurrentUserService _currentUserService;

        public GetMonthlyFinancialReportQueryHandler(ISender mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        public async Task<MonthlyFinancialReportDto> Handle(GetMonthlyFinancialReportQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            if (request.Year < 2000 || request.Year > 2100)
                throw new BadRequestException("السنة غير صالحة");

            if (request.Month is < 1 or > 12)
                throw new BadRequestException("الشهر يجب أن يكون بين 1 و 12");

            var start = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1).AddTicks(-1);

            var profit = await _mediator.Send(
                new GetBranchProfitReportQuery { FromDate = start, ToDate = end },
                cancellationToken);

            return new MonthlyFinancialReportDto
            {
                Year = request.Year,
                Month = request.Month,
                PeriodStartUtc = start,
                PeriodEndUtc = end,
                Profit = profit
            };
        }
    }
}
