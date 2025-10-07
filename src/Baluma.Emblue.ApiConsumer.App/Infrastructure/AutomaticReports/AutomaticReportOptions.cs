namespace Baluma.Emblue.ApiConsumer.Infrastructure.AutomaticReports;

public sealed class AutomaticReportOptions
{
    public const string SectionName = "AutomaticReports";

    public string BaseUrl { get; set; } = string.Empty;
    public string ReportsEndpoint { get; set; } = "automatic_reports/";
    public string ApiBearerToken { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
