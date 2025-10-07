using System.IO;

namespace Baluma.Emblue.ApiConsumer.Application.Abstractions;

public interface IFileStorage
{
    Task SaveAsync(Stream content, string fileName, CancellationToken cancellationToken);
}
