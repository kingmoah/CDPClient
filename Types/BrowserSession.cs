using System.Diagnostics;

namespace CDPClient.Types;

public enum BrowserSessionOwnership
{
    Owned,
    Attached
}

public sealed class BrowserSession : IDisposable
{
    public Browser Browser { get; }

    public Process? Process { get; }

    public DebugPort DebugPort { get; }

    public BrowserSessionOwnership Ownership { get; }

    public bool IsOwned =>
        Ownership == BrowserSessionOwnership.Owned;

    public bool IsAttached =>
        Ownership == BrowserSessionOwnership.Attached;

    public BrowserSession(
        Browser browser,
        Process? process,
        DebugPort debugPort,
        BrowserSessionOwnership ownership)
    {
        Browser = browser;
        Process = process;
        DebugPort = debugPort;
        Ownership = ownership;
    }

    public void Dispose()
    {
        // We only terminate processes that we started.
        if (!IsOwned)
        {
            return;
        }

        if (Process is null)
        {
            return;
        }

        if (Process.HasExited)
        {
            Process.Dispose();
            return;
        }

        try
        {
            Process.Kill(entireProcessTree: true);
            Process.WaitForExit();
        }
        finally
        {
            Process.Dispose();
        }
    }
}