namespace CDPClient.Types;

public sealed record Target(
    string Id,
    string Type,
    string Url,
    string Title,
    string? Description,
    string? DevtoolsFrontendUrl,
    string? WebSocketDebuggerUrl
);