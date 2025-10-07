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

    public async Task<bool> ExistsDataForDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        var hasDetails = await _dbContext.DailyActivityDetails.AnyAsync(
            detail =>
                (detail.ActivityDate.HasValue && detail.ActivityDate >= start && detail.ActivityDate < end) ||
                (detail.SendDate.HasValue && detail.SendDate >= start && detail.SendDate < end),
            cancellationToken);

        if (hasDetails)
        {
            return true;
        }

        var hasSummaries = await _dbContext.DailyActionSummaries.AnyAsync(
            summary => summary.Date.HasValue && summary.Date >= start && summary.Date < end,
            cancellationToken);

        return hasSummaries;
    }

    public async Task DeleteDataForDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = start.AddDays(1);

        await _dbContext.DailyActivityDetails
            .Where(detail =>
                (detail.ActivityDate.HasValue && detail.ActivityDate >= start && detail.ActivityDate < end) ||
                (detail.SendDate.HasValue && detail.SendDate >= start && detail.SendDate < end))
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.DailyActionSummaries
            .Where(summary => summary.Date.HasValue && summary.Date >= start && summary.Date < end)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
