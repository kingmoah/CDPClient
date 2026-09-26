
/// <summary>
/// Helper object that takes in a string and appends the systems TempPath with it.
/// </summary>
/// <param name="path"></param>
public sealed record TempPath(string Name = "edge-cdp")
{
    public string FullPath =>
        System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            Name
        );

    public static implicit operator string(
        TempPath path
    ) => path.FullPath;
}
