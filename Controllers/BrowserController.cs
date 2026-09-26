using System.Diagnostics;
using System.Threading.Tasks;
using CDPClient.Boundary;
using CDPClient.Types;

namespace CDPClient.Controllers;

public sealed class BrowserController : IBrowser, IDisposable
{
    private readonly HttpClient _httpClient;

    public BrowserController()
    {
        _httpClient = new HttpClient();
    }

    public BrowserSession Launch(
        Browser browser,
        BrowserLaunchOptions options)
    {
        ArgumentNullException.ThrowIfNull(browser);
        ArgumentNullException.ThrowIfNull(options);

        ValidateBrowser(browser);

        Directory.CreateDirectory(
            options.ProfileDirectory.FullPath
        );

        string arguments =
            $"--remote-debugging-port={options.DebugPort.Value} " +
            $"--user-data-dir=\"{options.ProfileDirectory.FullPath}\"";

        Process process = StartProcess(
            browser.Path,
            arguments
        );

        if (process.HasExited)
        {
            process.Dispose();

            throw new InvalidOperationException(
                $"Browser '{browser.Name}' exited immediately."
            );
        }

        return new BrowserSession(
            browser: browser,
            process: process,
            debugPort: options.DebugPort,
            ownership: BrowserSessionOwnership.Owned
        );
    }

    public async Task<BrowserSession> AttachAsync(
        Browser browser,
        DebugPort debugPort)
    {
        ArgumentNullException.ThrowIfNull(browser);

        if (!await IsCdpAvailableAsync(debugPort))
        {
            throw new InvalidOperationException(
                $"No CDP endpoint is available at {debugPort.VersionEndpoint}."
            );
        }

        return new BrowserSession(
            browser: browser,
            process: null,
            debugPort: debugPort,
            ownership: BrowserSessionOwnership.Attached
        );
    }

    public async Task<BrowserSession> GetOrCreateAsync(
        Browser browser,
        BrowserLaunchOptions options)
    {
        ArgumentNullException.ThrowIfNull(browser);
        ArgumentNullException.ThrowIfNull(options);

        if (await IsCdpAvailableAsync(options.DebugPort))
        {
            Console.WriteLine(
                $"Existing CDP browser found on port {options.DebugPort.Value}."
            );

            return await AttachAsync(
                browser,
                options.DebugPort
            );
        }

        Console.WriteLine(
            $"No CDP browser found on port {options.DebugPort.Value}."
        );

        Console.WriteLine(
            $"Starting {browser.Name}..."
        );

        return Launch(
            browser,
            options
        );
    }

    private async Task<bool> IsCdpAvailableAsync(DebugPort debugPort)
    {
        try
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync(
                    debugPort.VersionEndpoint
                );

            return response.IsSuccessStatusCode;
        }
        catch (
            HttpRequestException
        )
        {
            return false;
        }
        catch (
            TaskCanceledException
        )
        {
            return false;
        }
    }

    private static void ValidateBrowser(Browser browser)
    {
        if (string.IsNullOrWhiteSpace(browser.Name))
        {
            throw new ArgumentException(
                "Browser name cannot be empty.",
                nameof(browser)
            );
        }

        if (string.IsNullOrWhiteSpace(browser.Path))
        {
            throw new ArgumentException(
                "Browser executable path cannot be empty.",
                nameof(browser)
            );
        }

        if (!File.Exists(browser.Path))
        {
            throw new FileNotFoundException(
                $"Browser executable was not found: {browser.Path}",
                browser.Path
            );
        }
    }

    private static Process StartProcess(
        string path,
        string arguments)
    {
        Process process = new();

        process.StartInfo.FileName = path;
        process.StartInfo.Arguments = arguments;

        process.StartInfo.UseShellExecute = true;
        process.StartInfo.CreateNoWindow = false;

        if (!process.Start())
        {
            process.Dispose();

            throw new InvalidOperationException(
                $"Failed to start browser: {path}"
            );
        }

        return process;
    }

    public void Dispose()
    {
        
    }
}