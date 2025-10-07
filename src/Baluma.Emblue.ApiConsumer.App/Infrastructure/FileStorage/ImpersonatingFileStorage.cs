using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using Baluma.Emblue.ApiConsumer.Application.Abstractions;
using Baluma.Emblue.ApiConsumer.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32.SafeHandles;

namespace Baluma.Emblue.ApiConsumer.Infrastructure.FileStorage;

[SupportedOSPlatform("windows")]
public sealed class ImpersonatingFileStorage : IFileStorage
{
    private const int Logon32LogonNewCredentials = 9;
    private const int Logon32ProviderWinnt50 = 3;

    private readonly FileStorageOptions _options;
    private readonly ILogger<ImpersonatingFileStorage> _logger;

    public ImpersonatingFileStorage(IOptions<FileStorageOptions> options, ILogger<ImpersonatingFileStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SaveAsync(Stream content, string fileName, CancellationToken cancellationToken)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name must be provided.", nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(_options.Folder))
        {
            throw new InvalidOperationException("File storage folder is not configured.");
        }

        fileName = Path.GetFileName(fileName);
        var targetDirectory = _options.Folder;
        var targetPath = Path.Combine(targetDirectory, fileName);

        await RunWithImpersonationAsync(async () =>
        {
            _logger.LogDebug("Saving report to {Path}", targetPath);

            Directory.CreateDirectory(targetDirectory);

            if (content.CanSeek)
            {
                content.Seek(0, SeekOrigin.Begin);
            }

            await using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            await content.CopyToAsync(fileStream, cancellationToken);

            if (content.CanSeek)
            {
                content.Seek(0, SeekOrigin.Begin);
            }
        });
    }

    private async Task RunWithImpersonationAsync(Func<Task> action)
    {
        if (string.IsNullOrWhiteSpace(_options.Username) || string.IsNullOrWhiteSpace(_options.Password))
        {
            await action();
            return;
        }

        if (!LogonUser(_options.Username, string.IsNullOrWhiteSpace(_options.Domain) ? null : _options.Domain, _options.Password, Logon32LogonNewCredentials, Logon32ProviderWinnt50, out var accessToken))
        {
            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, "Failed to impersonate the configured user.");
        }

        using (accessToken)
        {
            WindowsIdentity.RunImpersonated(accessToken, () => action().GetAwaiter().GetResult());
        }
    }

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern bool LogonUser(
        string lpszUsername,
        string? lpszDomain,
        string lpszPassword,
        int dwLogonType,
        int dwLogonProvider,
        out SafeAccessTokenHandle phToken);
}
