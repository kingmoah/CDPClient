namespace CDPClient.Types.Runtime;

public sealed class RuntimeEvaluateResult
{
    public RemoteObject Result { get; }

    public ExceptionDetails? ExceptionDetails { get; }

    public RuntimeEvaluateResult(
        RemoteObject result,
        ExceptionDetails? exceptionDetails = null)
    {
        Result = result;
        ExceptionDetails = exceptionDetails;
    }

    public bool IsException =>
        ExceptionDetails is not null;
}