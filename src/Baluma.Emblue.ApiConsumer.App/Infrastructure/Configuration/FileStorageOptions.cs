namespace Baluma.Emblue.ApiConsumer.Infrastructure.Configuration;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string Folder { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
