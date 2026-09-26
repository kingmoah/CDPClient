using System.Data;
using System.Text.Json;
using CDPClient.Boundary;
using CDPClient.Types;
using CDPClient.Types.Runtime;

namespace CDPClient.Controllers;

public sealed class RuntimeController : IRuntime, IDisposable
{
    private readonly CDPConnection _connection;

    public RuntimeController(
        CDPConnection connection)
    {
        ArgumentNullException.ThrowIfNull(
            connection
        );

        _connection = connection;
    }

    

    public async Task<RuntimeEvaluateResult> EvaluateAsync(
        string expression,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException(
                "JavaScript expression cannot be empty.",
                nameof(expression)
            );
        }

        CDPResponse response =
            await _connection.SendAsync(
                "Runtime.evaluate",
                new
                {
                    expression,
                    returnByValue = true
                },
                cancellationToken
            );

        response.EnsureSuccess();

        if (!response.Result.HasValue)
        {
            throw new CDPException(
                "Runtime.evaluate returned no result."
            );
        }

        JsonElement result =
            response.Result.Value;

        if (!result.TryGetProperty(
                "result",
                out JsonElement remoteObject))
        {
            throw new CDPException(
                "Runtime.evaluate response did not contain a result object."
            );
        }

        return ParseRemoteObject(
            remoteObject
        );
    }

    

    private static RuntimeEvaluateResult ParseRemoteObject(
        JsonElement remoteObject)
    {
        string? type = null;
        string? subtype = null;
        string? className = null;
        string? description = null;

        JsonElement? value = null;

        if (remoteObject.TryGetProperty(
                "type",
                out JsonElement typeElement))
        {
            type =
                typeElement.GetString();
        }

        if (remoteObject.TryGetProperty(
                "subtype",
                out JsonElement subtypeElement))
        {
            subtype =
                subtypeElement.GetString();
        }

        if (remoteObject.TryGetProperty(
                "className",
                out JsonElement classNameElement))
        {
            className =
                classNameElement.GetString();
        }

        if (remoteObject.TryGetProperty(
                "description",
                out JsonElement descriptionElement))
        {
            description =
                descriptionElement.GetString();
        }

        if (remoteObject.TryGetProperty(
                "value",
                out JsonElement valueElement))
        {
            value =
                valueElement.Clone();
        }

        return new RuntimeEvaluateResult(
            result: new RemoteObject
            {
                Type = type,
                Subtype = subtype,
                ClassName = className,
                Description = description,
                Value = value
            }
        );

    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}