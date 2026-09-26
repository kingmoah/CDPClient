using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CDPClient.Types;

namespace CDPClient.Controllers;

public sealed class CDPConnection : IDisposable
{
    private readonly ClientWebSocket _socket = new();

    private readonly ConcurrentDictionary<
        int,
        CDPPendingCommand
    > _pendingCommands = new();

    private CancellationTokenSource? _receiveCancellation;

    private Task? _receiveTask;

    private int _nextId;

    public bool IsConnected =>
        _socket.State == WebSocketState.Open;

    public event EventHandler<CDPEvent>? EventReceived;

    public async Task ConnectAsync(
        Target target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (string.IsNullOrWhiteSpace(
            target.WebSocketDebuggerUrl))
        {
            throw new InvalidOperationException(
                $"Target '{target.Id}' does not expose a WebSocket debugger URL."
            );
        }

        if (IsConnected)
        {
            throw new InvalidOperationException(
                "CDP connection is already open."
            );
        }

        await _socket.ConnectAsync(
            new Uri(target.WebSocketDebuggerUrl),
            cancellationToken
        );

        _receiveCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken
            );

        _receiveTask =
            ReceiveLoopAsync(
                _receiveCancellation.Token
            );
    }

    public async Task<CDPResponse> SendAsync(
        string method,
        object? parameters = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException(
                "CDP connection is not open."
            );
        }

        if (string.IsNullOrWhiteSpace(method))
        {
            throw new ArgumentException(
                "CDP method cannot be empty.",
                nameof(method)
            );
        }

        int id =
            Interlocked.Increment(
                ref _nextId
            );

        CDPPendingCommand pending =
            new();

        if (!_pendingCommands.TryAdd(
                id,
                pending))
        {
            throw new InvalidOperationException(
                $"A pending CDP command with ID {id} already exists."
            );
        }

        try
        {
            var message = new
            {
                id,
                method,
                @params = parameters
            };

            string json =
                JsonSerializer.Serialize(message);

            byte[] bytes =
                Encoding.UTF8.GetBytes(json);

            await _socket.SendAsync(
                bytes,
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken
            );

            using CancellationTokenRegistration registration =
                cancellationToken.Register(
                    () =>
                    {
                        pending.Completion.TrySetCanceled(
                            cancellationToken
                        );
                    }
                );

            return await pending.Completion.Task;
        }
        finally
        {
            _pendingCommands.TryRemove(
                id,
                out _
            );
        }
    }

    private async Task ReceiveLoopAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string json =
                    await ReceiveMessageAsync(
                        cancellationToken
                    );

                ProcessMessage(json);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            FailPendingCommands(exception);
        }
    }

    private async Task<string> ReceiveMessageAsync(
        CancellationToken cancellationToken)
    {
        byte[] buffer =
            new byte[8192];

        using MemoryStream message =
            new();

        while (true)
        {
            WebSocketReceiveResult result =
                await _socket.ReceiveAsync(
                    buffer,
                    cancellationToken
                );

            if (result.MessageType ==
                WebSocketMessageType.Close)
            {
                throw new WebSocketException(
                    "CDP WebSocket was closed by the browser."
                );
            }

            if (result.MessageType !=
                WebSocketMessageType.Text)
            {
                continue;
            }

            message.Write(
                buffer,
                0,
                result.Count
            );

            if (result.EndOfMessage)
            {
                break;
            }
        }

        return Encoding.UTF8.GetString(
            message.ToArray()
        );
    }

    private void ProcessMessage(
        string json)
    {
        using JsonDocument document =
            JsonDocument.Parse(json);

        JsonElement root =
            document.RootElement;

        int? id = null;

        if (root.TryGetProperty(
                "id",
                out JsonElement idElement))
        {
            id = idElement.GetInt32();
        }

        if (id.HasValue)
        {
            ProcessResponse(
                id.Value,
                root
            );

            return;
        }

        if (root.TryGetProperty(
                "method",
                out JsonElement methodElement))
        {
            ProcessEvent(
                methodElement.GetString()!,
                root
            );
        }
    }

    private void ProcessResponse(
        int id,
        JsonElement root)
    {
        if (!_pendingCommands.TryGetValue(
                id,
                out CDPPendingCommand? pending))
        {
            return;
        }

        JsonElement? result = null;
        JsonElement? error = null;

        if (root.TryGetProperty(
                "result",
                out JsonElement resultElement))
        {
            result =
                resultElement.Clone();
        }

        if (root.TryGetProperty(
                "error",
                out JsonElement errorElement))
        {
            error =
                errorElement.Clone();
        }

        CDPResponse response =
            new(
                id,
                result,
                error
            );

        pending.Completion.TrySetResult(
            response
        );
    }

    private void ProcessEvent(
        string method,
        JsonElement root)
    {
        JsonElement? parameters = null;

        if (root.TryGetProperty(
                "params",
                out JsonElement paramsElement))
        {
            parameters =
                paramsElement.Clone();
        }

        CDPEvent @event =
            new(
                method,
                parameters
            );

        EventReceived?.Invoke(
            this,
            @event
        );
    }

    private void FailPendingCommands(
        Exception exception)
    {
        foreach (
            KeyValuePair<int, CDPPendingCommand> pair
            in _pendingCommands)
        {
            pair.Value.Completion.TrySetException(
                exception
            );
        }
    }

    public void Dispose()
    {


        try
        {
            _receiveCancellation?.Cancel();
            _receiveTask?.Wait();
        }
        catch (Exception ex)
        {
            if (ex is ObjectDisposedException)
            {
                //No worries
            }
            // Disposal should not throw because
            // the receive loop has terminated.
        }

        _receiveCancellation?.Dispose();
        _socket.Dispose();
    }
}