using CDPClient.Types;

namespace CDPClient.Boundary;

public interface IBrowser
{
    BrowserSession Launch(
        Browser browser,
        BrowserLaunchOptions options
    );

    Task<BrowserSession> AttachAsync(
        Browser browser,
        DebugPort debugPort
    );

    Task<BrowserSession> GetOrCreateAsync(
        Browser browser,
        BrowserLaunchOptions options
    );
}