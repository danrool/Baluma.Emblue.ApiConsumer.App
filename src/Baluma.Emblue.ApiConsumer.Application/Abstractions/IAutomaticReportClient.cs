using Baluma.Emblue.ApiConsumer.Domain.AutomaticReports;

namespace Baluma.Emblue.ApiConsumer.Application.Abstractions;

public interface IAutomaticReportClient
{
    Task<IReadOnlyList<AutomaticReportFileDescriptor>> GetDailyReportsAsync(DateOnly date, CancellationToken cancellationToken);
    Task<Stream> DownloadReportAsync(AutomaticReportFileDescriptor descriptor, CancellationToken cancellationToken);
}
