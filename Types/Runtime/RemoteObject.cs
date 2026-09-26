using System.Text.Json;

namespace CDPClient.Types.Runtime;

public sealed class RemoteObject
{
    public string? Type { get; init; }

    public string? Subtype { get; init; }

    public string? ClassName { get; init; }

    public string? Description { get; init; }

    public JsonElement? Value { get; init; }

    public string? ObjectId { get; init; }

    public bool IsRemote =>
        !string.IsNullOrWhiteSpace(
            ObjectId
        );

    public bool IsPrimitive =>
        Value.HasValue;

    public string? GetString()
    {
        if (!Value.HasValue)
        {
            return null;
        }

        return Value.Value.ValueKind switch
        {
            JsonValueKind.String =>
                Value.Value.GetString(),

            _ =>
                Value.Value.ToString()
        };
    }

    public double? GetNumber()
    {
        if (!Value.HasValue)
        {
            return null;
        }

        if (Value.Value.ValueKind !=
            JsonValueKind.Number)
        {
            return null;
        }

        return Value.Value.GetDouble();
    }

    public bool? GetBoolean()
    {
        if (!Value.HasValue)
        {
            return null;
        }

        if (Value.Value.ValueKind !=
            JsonValueKind.True &&
            Value.Value.ValueKind !=
            JsonValueKind.False)
        {
            return null;
        }

        return Value.Value.GetBoolean();
    }
}