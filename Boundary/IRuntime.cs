using CDPClient.Types.Runtime;

namespace CDPClient.Boundary;

public interface IRuntime
{
    Task<RuntimeEvaluateResult> EvaluateAsync(
        string expression,
        CancellationToken cancellationToken = default
    );
}