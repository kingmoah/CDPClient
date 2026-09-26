
using System.Text.Json;

namespace CDPClient.Types;

public sealed class CDPMessage
{
    public int? Id { get; init; }

    public string? Method { get; init; }

    public JsonElement? Params { get; init; }

    public JsonElement? Result { get; init; }

    public JsonElement? Error { get; init; }

    public bool IsResponse =>
        Id.HasValue;

    public bool IsEvent =>
        !string.IsNullOrWhiteSpace(Method) &&
        !Id.HasValue;

    public bool IsError =>
        Error.HasValue;
}