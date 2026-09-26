namespace CDPClient.Types.Runtime;

public sealed class ExceptionDetails
{
    public int? ExceptionId { get; init; }

    public string? Text { get; init; }

    public RemoteObject? Exception { get; init; }

    public int? LineNumber { get; init; }

    public int? ColumnNumber { get; init; }

    public override string ToString()
    {
        return Text ?? "JavaScript evaluation failed.";
    }
}