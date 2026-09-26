namespace CDPClient.Types.Page;

public sealed class Frame
{
    public string Id { get; init; } = string.Empty;

    public string? ParentId { get; init; }

    public string? LoaderId { get; init; }

    public string Url { get; init; } = string.Empty;

    public string? DomainAndRegistry { get; init; }

    public string? SecurityOrigin { get; init; }

    public string? MimeType { get; init; }
}