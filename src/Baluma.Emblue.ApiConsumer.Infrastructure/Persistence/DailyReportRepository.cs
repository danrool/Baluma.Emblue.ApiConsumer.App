using Baluma.Emblue.ApiConsumer.Application.Abstractions;
using Baluma.Emblue.ApiConsumer.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace Baluma.Emblue.ApiConsumer.Infrastructure.Persistence;

public sealed class DailyReportRepository : IDailyReportRepository
{
    private readonly ApiConsumerDbContext _dbContext;

    public DailyReportRepository(ApiConsumerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveDailyActivityDetailsAsync(IEnumerable<DailyActivityDetail> details, CancellationToken cancellationToken)
    {
        await _dbContext.DailyActivityDetails.AddRangeAsync(details, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveDailyActionSummariesAsync(IEnumerable<DailyActionSummary> summaries, CancellationToken cancellationToken)
    {
        await _dbContext.DailyActionSummaries.AddRangeAsync(summaries, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
