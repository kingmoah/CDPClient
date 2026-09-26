namespace CDPClient.Controllers;

internal sealed class CDPPendingCommand
{
    public TaskCompletionSource<Types.CDPResponse> Completion { get; }

    public CDPPendingCommand()
    {
        Completion =
            new TaskCompletionSource<Types.CDPResponse>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
    }
}