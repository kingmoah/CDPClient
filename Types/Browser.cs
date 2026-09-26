namespace CDPClient.Types;

public record Browser(
    String Name, 
    String Path
);

public sealed record BrowserLaunchOptions(
    DebugPort DebugPort,
    TempPath ProfileDirectory
);




