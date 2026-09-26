using System.Text.Json;

namespace CDPClient.Types;

public sealed class CDPResponse
{
    public int Id { get; }

    public JsonElement? Result { get; }

    public JsonElement? Error { get; }

    public bool IsSuccess =>
        !Error.HasValue;

    public CDPResponse(
        int id,
        JsonElement? result,
        JsonElement? error)
    {
        Id = id;
        Result = result;
        Error = error;
    }

    public void EnsureSuccess()
    {
        if (IsSuccess)
        {
            return;
        }

        throw new CDPException(
            $"CDP command {Id} failed.",
            Error
        );
    }
}
