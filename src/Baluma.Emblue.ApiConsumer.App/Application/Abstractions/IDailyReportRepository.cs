using Baluma.Emblue.ApiConsumer.Domain.Reports;

namespace Baluma.Emblue.ApiConsumer.Application.Abstractions;

public interface IDailyReportRepository
{
    Task SaveDailyActivityDetailsAsync(IEnumerable<DailyActivityDetail> details, CancellationToken cancellationToken);
    Task SaveDailyActionSummariesAsync(IEnumerable<DailyActionSummary> summaries, CancellationToken cancellationToken);
}
