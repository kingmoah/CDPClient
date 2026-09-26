using System.Text.Json;
using CDPClient.Boundary;
using CDPClient.Types;
using CDPClient.Types.Page;

namespace CDPClient.Controllers;

public sealed class PageController : IPage, IDisposable
{
    private readonly CDPConnection _connection;

    public event EventHandler<FrameNavigatedEvent>?
        FrameNavigated;

    public event EventHandler<LoadEventFiredEvent>?
        LoadEventFired;

    public PageController(
        CDPConnection connection)
    {
        ArgumentNullException.ThrowIfNull(
            connection
        );

        _connection = connection;

        _connection.EventReceived +=
            OnEventReceived;
    }

    public async Task EnableAsync(
        CancellationToken cancellationToken = default)
    {
        CDPResponse response =
            await _connection.SendAsync(
                "Page.enable",
                cancellationToken: cancellationToken
            );

        response.EnsureSuccess();
    }

    public async Task<NavigationResult> NavigateAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException(
                "URL cannot be empty.",
                nameof(url)
            );
        }

        CDPResponse response =
            await _connection.SendAsync(
                "Page.navigate",
                new
                {
                    url
                },
                cancellationToken
            );

        response.EnsureSuccess();

        if (!response.Result.HasValue)
        {
            throw new CDPException(
                "Page.navigate returned no result."
            );
        }

        JsonElement result =
            response.Result.Value;

        string? frameId = null;
        string? loaderId = null;
        string? errorText = null;

        if (result.TryGetProperty(
                "frameId",
                out JsonElement frameIdElement))
        {
            frameId =
                frameIdElement.GetString();
        }

        if (result.TryGetProperty(
                "loaderId",
                out JsonElement loaderIdElement))
        {
            loaderId =
                loaderIdElement.GetString();
        }

        if (result.TryGetProperty(
                "errorText",
                out JsonElement errorElement))
        {
            errorText =
                errorElement.GetString();
        }

        return new NavigationResult
        {
            FrameId = frameId,
            LoaderId = loaderId,
            ErrorText = errorText
        };
    }

    public async Task ReloadAsync(
        bool ignoreCache = false,
        CancellationToken cancellationToken = default)
    {
        CDPResponse response =
            await _connection.SendAsync(
                "Page.reload",
                new
                {
                    ignoreCache
                },
                cancellationToken
            );

        response.EnsureSuccess();
    }

    private void OnEventReceived(
        object? sender,
        CDPEvent @event)
    {
        switch (@event.Method)
        {
            case "Page.frameNavigated":

                if (@event.Params.HasValue)
                {
                    FrameNavigated?.Invoke(
                        this,
                        new FrameNavigatedEvent(
                            ParseFrameNavigated(
                                @event.Params.Value
                            )
                        )
                    );
                }

                break;

            case "Page.loadEventFired":

                if (@event.Params.HasValue)
                {
                    LoadEventFired?.Invoke(
                        this,
                        new LoadEventFiredEvent(
                            ParseLoadEvent(
                                @event.Params.Value
                            )
                        )
                    );
                }

                break;
        }
    }

    private static Frame ParseFrameNavigated(
        JsonElement parameters)
    {
        if (!parameters.TryGetProperty(
                "frame",
                out JsonElement frameElement))
        {
            throw new CDPException(
                "Page.frameNavigated event did not contain a frame."
            );
        }

        return ParseFrame(
            frameElement
        );
    }

    private static Frame ParseFrame(
        JsonElement element)
    {
        string id =
            element.GetProperty("id").GetString()
            ?? string.Empty;

        string? parentId = null;
        string? loaderId = null;
        string? domainAndRegistry = null;
        string? securityOrigin = null;
        string? mimeType = null;

        if (element.TryGetProperty(
                "parentId",
                out JsonElement parentElement))
        {
            parentId =
                parentElement.GetString();
        }

        if (element.TryGetProperty(
                "loaderId",
                out JsonElement loaderElement))
        {
            loaderId =
                loaderElement.GetString();
        }

        if (element.TryGetProperty(
                "domainAndRegistry",
                out JsonElement domainElement))
        {
            domainAndRegistry =
                domainElement.GetString();
        }

        if (element.TryGetProperty(
                "securityOrigin",
                out JsonElement originElement))
        {
            securityOrigin =
                originElement.GetString();
        }

        if (element.TryGetProperty(
                "mimeType",
                out JsonElement mimeTypeElement))
        {
            mimeType =
                mimeTypeElement.GetString();
        }

        string url =
            element.TryGetProperty(
                "url",
                out JsonElement urlElement)
                ? urlElement.GetString() ?? string.Empty
                : string.Empty;

        return new Frame
        {
            Id = id,
            ParentId = parentId,
            LoaderId = loaderId,
            Url = url,
            DomainAndRegistry = domainAndRegistry,
            SecurityOrigin = securityOrigin,
            MimeType = mimeType
        };
    }

    private static double ParseLoadEvent(
        JsonElement parameters)
    {
        if (!parameters.TryGetProperty(
                "timestamp",
                out JsonElement timestamp))
        {
            throw new CDPException(
                "Page.loadEventFired event did not contain a timestamp."
            );
        }

        return timestamp.GetDouble();
    }

    public void Dispose()
    {
        _connection.EventReceived -=
            OnEventReceived;
    }
}