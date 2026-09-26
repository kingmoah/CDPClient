using CDPClient.Types;

namespace CDPClient.Boundary;

public interface ITarget
{
    Task<IReadOnlyList<Target>> GetTargetsAsync(
        BrowserSession session
    );

    Task<Target?> GetTargetAsync(
        BrowserSession session,
        string targetId
    );
}