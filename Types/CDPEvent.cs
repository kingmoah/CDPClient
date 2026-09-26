using System.Text.Json;

namespace CDPClient.Types;

public sealed class CDPEvent
{
    public string Method { get; }

    public JsonElement? Params { get; }

    public CDPEvent(
        string method,
        JsonElement? parameters)
    {
        Method = method;
        Params = parameters;
    }
}