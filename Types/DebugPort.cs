namespace CDPClient.Types;

public readonly record struct DebugPort(int Value)
{
    public Uri BaseUri =>
        new($"http://localhost:{Value}/");

    public Uri JsonEndpoint =>
        new(BaseUri, "json");

    public Uri VersionEndpoint =>
        new(BaseUri, "json/version");
}