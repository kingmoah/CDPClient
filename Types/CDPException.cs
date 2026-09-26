using System.Text.Json;

namespace CDPClient.Types;

public sealed class CDPException : Exception
{
    public JsonElement? Error { get; }

    public CDPException(
        string message,
        JsonElement? error = null)
        : base(message)
    {
        Error = error;
    }
}