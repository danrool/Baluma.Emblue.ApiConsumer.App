using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using Baluma.Emblue.ApiConsumer.Application.Abstractions;
using Baluma.Emblue.ApiConsumer.Domain.AutomaticReports;
using Baluma.Emblue.ApiConsumer.Domain.Reports;

namespace Baluma.Emblue.ApiConsumer.Application.Reports.Parsers;

public sealed class DailyActivityDetailReportParser : IReportContentParser
{
    private readonly IDailyReportRepository _repository;

    public DailyActivityDetailReportParser(IDailyReportRepository repository)
    {
        _repository = repository;
    }

    public AutomaticReportType ReportType => AutomaticReportType.DailyActivityDetail;

    public async Task ParseAndPersistAsync(Stream reportStream, CancellationToken cancellationToken)
    {
        if (reportStream.CanSeek)
        {
            reportStream.Seek(0, SeekOrigin.Begin);
        }

        var details = new List<DailyActivityDetail>();
        using var archive = new ZipArchive(reportStream, ZipArchiveMode.Read, leaveOpen: true);
        foreach (var entry in archive.Entries)
        {
            if (entry.Length == 0)
            {
                continue;
            }

            using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var content = await reader.ReadToEndAsync();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (lines.Length <= 1)
            {
                continue;
            }

            for (var i = 1; i < lines.Length; i++)
            {
                var columns = lines[i].Split(';');
                if (columns.Length < 12)
                {
                    continue;
                }

                details.Add(new DailyActivityDetail
                {
                    Email = columns[0],
                    SendDate = ParseDateTime(columns[1]),
                    ActivityDate = ParseDateTime(columns[2]),
                    Campaign = columns[3],
                    Action = columns[4],
                    ActionType = columns[5],
                    Activity = columns[6],
                    Description = GetValue(columns, 7),
                    Tag = GetValue(columns, 8),
                    Account = GetValue(columns, 9),
                    Category = GetValue(columns, 10),
                    SegmentCategory = GetValue(columns, 11)
                });
            }
        }

        if (details.Count > 0)
        {
            await _repository.SaveDailyActivityDetailsAsync(details, cancellationToken);
        }
    }

    private static DateTime? ParseDateTime(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static string? GetValue(string[] columns, int index)
    {
        if (index >= columns.Length)
        {
            return null;
        }

        var value = columns[index].Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
