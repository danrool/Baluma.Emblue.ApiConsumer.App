using System.Text.Json.Serialization;

namespace Baluma.Emblue.ApiConsumer.Infrastructure.AutomaticReports;

internal sealed class AutomaticReportResponse
{
    [JsonPropertyName("archivo_id")]
    public int FileId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("fecha_creacion")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("empresa_id")]
    public int CompanyId { get; set; }
}
