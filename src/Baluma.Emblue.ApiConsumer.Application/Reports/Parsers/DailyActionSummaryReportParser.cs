using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using Baluma.Emblue.ApiConsumer.Application.Abstractions;
using Baluma.Emblue.ApiConsumer.Domain.AutomaticReports;
using Baluma.Emblue.ApiConsumer.Domain.Reports;

namespace Baluma.Emblue.ApiConsumer.Application.Reports.Parsers;

public sealed class DailyActionSummaryReportParser : IReportContentParser
{
    private readonly IDailyReportRepository _repository;

    public DailyActionSummaryReportParser(IDailyReportRepository repository)
    {
        _repository = repository;
    }

    public AutomaticReportType ReportType => AutomaticReportType.DailyActionSummary;

    public async Task ParseAndPersistAsync(Stream reportStream, CancellationToken cancellationToken)
    {
        if (reportStream.CanSeek)
        {
            reportStream.Seek(0, SeekOrigin.Begin);
        }

        var summaries = new List<DailyActionSummary>();
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
                if (columns.Length < 27)
                {
                    continue;
                }

                summaries.Add(new DailyActionSummary
                {
                    Campaign = columns[0],
                    Action = columns[1],
                    Type = columns[2],
                    Subject = columns[3],
                    Sender = columns[4],
                    TrustedSender = GetValue(columns, 5),
                    TouchRules = GetValue(columns, 6),
                    Date = ParseDateTime(columns[7]),
                    Recipients = GetValue(columns, 8),
                    Sent = GetValue(columns, 9),
                    Bounces = GetValue(columns, 10),
                    Effective = GetValue(columns, 11),
                    Opens = GetValue(columns, 12),
                    UniqueOpens = GetValue(columns, 13),
                    Clicks = GetValue(columns, 14),
                    UniqueClicks = GetValue(columns, 15),
                    Virals = GetValue(columns, 16),
                    Subscribers = GetValue(columns, 17),
                    Unsubscribers = GetValue(columns, 18),
                    Dr = GetValue(columns, 19),
                    Br = GetValue(columns, 20),
                    Or = GetValue(columns, 21),
                    Uor = GetValue(columns, 22),
                    Ctr = GetValue(columns, 23),
                    Ctor = GetValue(columns, 24),
                    Ctuor = GetValue(columns, 25),
                    Vr = GetValue(columns, 26)
                });
            }
        }

        if (summaries.Count > 0)
        {
            await _repository.SaveDailyActionSummariesAsync(summaries, cancellationToken);
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
