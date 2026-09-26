using System.Text.Json;
using CDPClient.Boundary;
using CDPClient.Types;

namespace CDPClient.Controllers;

public sealed class TargetController : ITarget, IDisposable
{
    private readonly HttpClient _httpClient;

    public TargetController(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<IReadOnlyList<Target>> GetTargetsAsync(
        BrowserSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        using HttpResponseMessage response =
            await _httpClient.GetAsync(
                session.DebugPort.JsonEndpoint
            );

        response.EnsureSuccessStatusCode();

        await using Stream stream =
            await response.Content.ReadAsStreamAsync();

        List<Target>? targets =
            await JsonSerializer.DeserializeAsync<List<Target>>(
                stream,
                JsonOptions
            );

        return targets ?? [];
    }

    public async Task<Target?> GetTargetAsync(
        BrowserSession session,
        string targetId)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (string.IsNullOrWhiteSpace(targetId))
        {
            throw new ArgumentException(
                "Target ID cannot be empty.",
                nameof(targetId)
            );
        }

        IReadOnlyList<Target> targets =
            await GetTargetsAsync(session);

        return targets.FirstOrDefault(
            target => target.Id == targetId
        );
    }

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}