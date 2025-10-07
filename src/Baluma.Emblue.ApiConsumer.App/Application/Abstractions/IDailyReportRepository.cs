using Baluma.Emblue.ApiConsumer.Domain.AutomaticReports;
using Baluma.Emblue.ApiConsumer.Domain.Reports;

namespace Baluma.Emblue.ApiConsumer.Application.Abstractions;

public interface IDailyReportRepository
{
    Task<int> EnsureReportFileAsync(
        AutomaticReportFileDescriptor descriptor,
        DateOnly reportDate,
        int taskExecutionLogId,
        CancellationToken cancellationToken);

    Task MarkReportFileAsProcessedAsync(int taskExecutionFileId, DateTime processedAtUtc, CancellationToken cancellationToken);

    Task SaveDailyActivityDetailsAsync(
        int taskExecutionFileId,
        IEnumerable<DailyActivityDetail> details,
        CancellationToken cancellationToken);

    Task SaveDailyActionSummariesAsync(
        int taskExecutionFileId,
        IEnumerable<DailyActionSummary> summaries,
        CancellationToken cancellationToken);
    Task<bool> ExistsDataForDateAsync(DateOnly date, CancellationToken cancellationToken);
    Task DeleteDataForDateAsync(DateOnly date, CancellationToken cancellationToken);
}
