using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

public class ImpersonationHelper : IDisposable
{
    private IntPtr _userHandle = IntPtr.Zero;
    private bool _disposed = false;

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private extern static bool CloseHandle(IntPtr handle);

    public bool ImpersonateUser(string username, string domain, string password, out WindowsIdentity? identity)
    {
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_NEW_CREDENTIALS = 9; // Use this for network access

        identity = null;

        if (LogonUser(username, domain, password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT, out _userHandle))
        {
            identity = new WindowsIdentity(_userHandle);
            return true;
        }
        else
        {
            int errorCode = Marshal.GetLastWin32Error();
            throw new UnauthorizedAccessException($"Impersonation failed. Error code: {errorCode}");
        }
    }

    public void UndoImpersonation()
    {
        if (_userHandle != IntPtr.Zero)
        {
            if (!CloseHandle(_userHandle))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to close handle. Error code: {errorCode}");
            }
            _userHandle = IntPtr.Zero;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                try
                {
                    UndoImpersonation();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to dispose impersonation helper", ex);
                }
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
