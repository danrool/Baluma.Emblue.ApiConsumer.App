using Baluma.Emblue.ApiConsumer.Application.Abstractions;

namespace Baluma.Emblue.ApiConsumer.Infrastructure.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
