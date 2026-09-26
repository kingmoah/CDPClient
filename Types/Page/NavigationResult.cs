namespace CDPClient.Types.Page;

public sealed class NavigationResult
{
    public string? FrameId { get; init; }

    public string? LoaderId { get; init; }

    public string? ErrorText { get; init; }

    public bool IsSuccess =>
        string.IsNullOrWhiteSpace(ErrorText);
}