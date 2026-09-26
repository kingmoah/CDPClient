using CDPClient.Types;

namespace CDPClient.Boundary;

public interface ICDPClient
{
    Task ConnectAsync(Target target);
}