using CDPClient.Types.Page;

namespace CDPClient.Boundary;

public interface IPage
{
    Task EnableAsync(
        CancellationToken cancellationToken = default
    );

    Task<NavigationResult> NavigateAsync(
        string url,
        CancellationToken cancellationToken = default
    );

    Task ReloadAsync(
        bool ignoreCache = false,
        CancellationToken cancellationToken = default
    );

    event EventHandler<FrameNavigatedEvent>? FrameNavigated;

    event EventHandler<LoadEventFiredEvent>? LoadEventFired;
}