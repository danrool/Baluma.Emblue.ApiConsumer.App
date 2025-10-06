namespace Baluma.Emblue.ApiConsumer.Domain.AutomaticReports;

public sealed record AutomaticReportFileDescriptor(
    int FileId,
    Uri Url,
    DateTime CreatedAtUtc,
    int CompanyId,
    AutomaticReportType ReportType,
    string FileName
);
