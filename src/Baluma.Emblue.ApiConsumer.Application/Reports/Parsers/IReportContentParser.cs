using Baluma.Emblue.ApiConsumer.Domain.AutomaticReports;

namespace Baluma.Emblue.ApiConsumer.Application.Reports.Parsers;

public interface IReportContentParser
{
    AutomaticReportType ReportType { get; }
    Task ParseAndPersistAsync(Stream reportStream, CancellationToken cancellationToken);
}
