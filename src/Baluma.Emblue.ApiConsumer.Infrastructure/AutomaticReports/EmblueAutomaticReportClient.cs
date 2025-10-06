using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Baluma.Emblue.ApiConsumer.Application.Abstractions;
using Baluma.Emblue.ApiConsumer.Domain.AutomaticReports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Baluma.Emblue.ApiConsumer.Infrastructure.AutomaticReports;

public sealed class EmblueAutomaticReportClient : IAutomaticReportClient
{
    private readonly HttpClient _httpClient;
    private readonly AutomaticReportOptions _options;
    private readonly ILogger<EmblueAutomaticReportClient> _logger;

    public EmblueAutomaticReportClient(HttpClient httpClient, IOptions<AutomaticReportOptions> options, ILogger<EmblueAutomaticReportClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        ConfigureHttpClient();
    }

    public async Task<IReadOnlyList<AutomaticReportFileDescriptor>> GetDailyReportsAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var requestUri = new Uri(new Uri(_options.BaseUrl), _options.ReportsEndpoint);
        var payload = new { date = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) };
        _logger.LogInformation("Requesting automatic reports for {Date} from {Url}", date, requestUri);
        using var response = await _httpClient.PostAsJsonAsync(requestUri, payload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var reports = await response.Content.ReadFromJsonAsync<IReadOnlyList<AutomaticReportResponse>>(cancellationToken: cancellationToken);
        if (reports is null || reports.Count == 0)
        {
            return Array.Empty<AutomaticReportFileDescriptor>();
        }

        return reports
            .Select(MapToDescriptor)
            .ToList();
    }

    public async Task<Stream> DownloadReportAsync(AutomaticReportFileDescriptor descriptor, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Downloading report {FileName} from {Url}", descriptor.FileName, descriptor.Url);
        using var response = await _httpClient.GetAsync(descriptor.Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return new MemoryStream(bytes, writable: false);
    }

    private void ConfigureHttpClient()
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    private static AutomaticReportFileDescriptor MapToDescriptor(AutomaticReportResponse response)
    {
        var url = new Uri(response.Url);
        var fileName = Path.GetFileNameWithoutExtension(url.LocalPath);
        return new AutomaticReportFileDescriptor(
            response.FileId,
            url,
            response.CreatedAt,
            response.CompanyId,
            ResolveReportType(fileName),
            fileName);
    }

    private static AutomaticReportType ResolveReportType(string fileName)
    {
        if (fileName.Contains("ACTIVIDADDETALLEDIARIO", StringComparison.OrdinalIgnoreCase))
        {
            return AutomaticReportType.DailyActivityDetail;
        }

        if (fileName.Contains("CONSOLIDADOACCIONESDIARIO", StringComparison.OrdinalIgnoreCase))
        {
            return AutomaticReportType.DailyActionSummary;
        }

        return AutomaticReportType.Unknown;
    }
}
