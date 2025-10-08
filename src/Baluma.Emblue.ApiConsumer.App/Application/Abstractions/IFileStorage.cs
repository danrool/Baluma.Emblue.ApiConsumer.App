using System.IO;

namespace Baluma.Emblue.ApiConsumer.Application.Abstractions;

public interface IFileStorage
{
    Task SaveAsync(Stream content, string fileName, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string fileName, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string fileName, CancellationToken cancellationToken);
}
