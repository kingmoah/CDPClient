using System.Text.Json;
using CDPClient.Boundary;
using CDPClient.Types;
using CDPClient.Types.DOM;

namespace CDPClient.Controllers;

public sealed class DOMController : IDOM, IDisposable
{
    private readonly CDPConnection _connection;

    public DOMController(CDPConnection connection)
    {
        _connection = connection;

        _connection.EventReceived +=
            OnEventReceived;
    }

    public async Task EnableAsync(
        CancellationToken cancellationToken = default)
    {
        CDPResponse response =
            await _connection.SendAsync(
                "DOM.enable",
                cancellationToken: cancellationToken
            );

        response.EnsureSuccess();
    }

    public async Task<Document> GetDocumentAsync(
        CancellationToken cancellationToken = default)
    {
        CDPResponse response =
            await _connection.SendAsync(
                "DOM.getDocument",
                cancellationToken: cancellationToken
            );

        response.EnsureSuccess();

        if (!response.Result.HasValue)
        {
            throw new CDPException(
                "DOM.getDocument returned no result."
            );
        }

        JsonElement result =
            response.Result.Value;

        JsonElement root =
            result.GetProperty("root");

        Node node =
            ParseNode(root);

        return new Document(node);
    }

    public async Task<Node?> QuerySelectorAsync(
        NodeId nodeId,
        string selector,
        CancellationToken cancellationToken = default)
    {
        CDPResponse response =
            await _connection.SendAsync(
                "DOM.querySelector",
                new
                {
                    nodeId = nodeId.Value,
                    selector
                },
                cancellationToken
            );

        response.EnsureSuccess();

        if (!response.Result.HasValue)
        {
            throw new CDPException(
                "DOM.querySelector returned no result."
            );
        }

        JsonElement result =
            response.Result.Value;

        int resultNodeId =
            result.GetProperty("nodeId").GetInt32();

        if (resultNodeId == 0)
        {
            return null;
        }

        return new Node
        {
            NodeId =
                new NodeId(resultNodeId)
        };
    }

    public async Task<IReadOnlyList<Node>> QuerySelectorAllAsync(
        NodeId nodeId,
        string selector,
        CancellationToken cancellationToken = default)
    {
        CDPResponse response =
            await _connection.SendAsync(
                "DOM.querySelectorAll",
                new
                {
                    nodeId = nodeId.Value,
                    selector
                },
                cancellationToken
            );

        response.EnsureSuccess();

        if (!response.Result.HasValue)
        {
            throw new CDPException(
                "DOM.querySelectorAll returned no result."
            );
        }

        JsonElement result =
            response.Result.Value;

        JsonElement nodeIds =
            result.GetProperty("nodeIds");

        List<Node> nodes = new();

        foreach (JsonElement element in nodeIds.EnumerateArray())
        {
            nodes.Add(
                new Node
                {
                    NodeId =
                        new NodeId(
                            element.GetInt32()
                        )
                }
            );
        }

        return nodes;
    }

    public async Task<string> GetOuterHTMLAsync(
        NodeId nodeId,
        CancellationToken cancellationToken = default)
    {
        CDPResponse response =
            await _connection.SendAsync(
                "DOM.getOuterHTML",
                new
                {
                    nodeId = nodeId.Value
                },
                cancellationToken
            );

        response.EnsureSuccess();

        if (!response.Result.HasValue)
        {
            throw new CDPException(
                "DOM.getOuterHTML returned no result."
            );
        }

        JsonElement result =
            response.Result.Value;

        return result
            .GetProperty("outerHTML")
            .GetString()
            ?? string.Empty;
    }

    private static Node ParseNode(
        JsonElement element)
    {
        Node node = new()
        {
            NodeId =
                new NodeId(
                    element.GetProperty("nodeId").GetInt32()
                ),

            BackendNodeId =
                new BackendNodeId(
                    element
                        .GetProperty("backendNodeId")
                        .GetInt32()
                ),

            NodeType =
                element.GetProperty("nodeType").GetInt32(),

            NodeName =
                element.GetProperty("nodeName").GetString()
                ?? string.Empty,

            LocalName =
                element.GetProperty("localName").GetString()
                ?? string.Empty,

            NodeValue =
                element.GetProperty("nodeValue").GetString()
                ?? string.Empty,

            DocumentUrl =
                GetString(element, "documentURL"),

            BaseUrl =
                GetString(element, "baseURL"),

            PublicId =
                GetString(element, "publicId"),

            SystemId =
                GetString(element, "systemId"),

            InternalSubset =
                GetString(element, "internalSubset"),

            XmlVersion =
                GetString(element, "xmlVersion"),

            Name =
                GetString(element, "name"),

            Value =
                GetString(element, "value")
        };

        if (element.TryGetProperty(
                "children",
                out JsonElement children))
        {
            Node[] childNodes =
                children
                    .EnumerateArray()
                    .Select(ParseNode)
                    .ToArray();

            node = node with
            {
                Children = childNodes
            };
        }

        return node;
    }

    private static string? GetString(
        JsonElement element,
        string property)
    {
        if (!element.TryGetProperty(
                property,
                out JsonElement value))
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private void OnEventReceived(
        object? sender,
        CDPEvent @event)
    {
        // DOM events will be handled here later.
    }

    public void Dispose()
    {
        _connection.EventReceived -=
            OnEventReceived;
    }
}