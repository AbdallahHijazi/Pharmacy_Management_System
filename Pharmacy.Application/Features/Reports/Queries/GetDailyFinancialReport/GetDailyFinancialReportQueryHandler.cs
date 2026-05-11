using MediatR;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Reports;
using Pharmacy.Application.Features.Reports.Queries.GetBranchProfitReport;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Features.Reports.Queries.GetDailyFinancialReport
{
    public sealed class GetDailyFinancialReportQueryHandler : IRequestHandler<GetDailyFinancialReportQuery, DailyFinancialReportDto>
    {
        private readonly ISender _mediator;
        private readonly ICurrentUserService _currentUserService;

        public GetDailyFinancialReportQueryHandler(ISender mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        public async Task<DailyFinancialReportDto> Handle(GetDailyFinancialReportQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var dayUtc = ToUtcDate(request.Date);
            var from = dayUtc;
            var to = dayUtc.AddDays(1).AddTicks(-1);

            var profit = await _mediator.Send(
                new GetBranchProfitReportQuery { FromDate = from, ToDate = to },
                cancellationToken);

            return new DailyFinancialReportDto
            {
                ReportDateUtc = dayUtc,
                Profit = profit
            };
        }

        private static DateTime ToUtcDate(DateTime d)
        {
            if (d.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
            return d.ToUniversalTime().Date;
        }
    }
}
