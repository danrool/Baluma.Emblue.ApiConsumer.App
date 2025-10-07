namespace Baluma.Emblue.ApiConsumer.Domain.Reports;

public sealed class DailyActionSummary
{
    public int Id { get; set; }
    public int TaskExecutionFileId { get; set; }
    public string Campaign { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string? TrustedSender { get; set; }
    public string? TouchRules { get; set; }
    public DateTime? Date { get; set; }
    public string? Recipients { get; set; }
    public string? Sent { get; set; }
    public string? Bounces { get; set; }
    public string? Effective { get; set; }
    public string? Opens { get; set; }
    public string? UniqueOpens { get; set; }
    public string? Clicks { get; set; }
    public string? UniqueClicks { get; set; }
    public string? Virals { get; set; }
    public string? Subscribers { get; set; }
    public string? Unsubscribers { get; set; }
    public string? Dr { get; set; }
    public string? Br { get; set; }
    public string? Or { get; set; }
    public string? Uor { get; set; }
    public string? Ctr { get; set; }
    public string? Ctor { get; set; }
    public string? Ctuor { get; set; }
    public string? Vr { get; set; }
}
