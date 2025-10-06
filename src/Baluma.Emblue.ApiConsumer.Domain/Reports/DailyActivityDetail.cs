namespace Baluma.Emblue.ApiConsumer.Domain.Reports;

public sealed class DailyActivityDetail
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime? SendDate { get; set; }
    public DateTime? ActivityDate { get; set; }
    public string Campaign { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Activity { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Tag { get; set; }
    public string? Account { get; set; }
    public string? Category { get; set; }
    public string? SegmentCategory { get; set; }
}
