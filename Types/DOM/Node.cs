namespace CDPClient.Types.DOM;

public sealed record Node
{
    public NodeId NodeId { get; init; }

    public NodeId? ParentId { get; init; }

    public BackendNodeId BackendNodeId { get; init; }

    public int NodeType { get; init; }

    public string NodeName { get; init; } = string.Empty;

    public string LocalName { get; init; } = string.Empty;

    public string NodeValue { get; init; } = string.Empty;

    public string? DocumentUrl { get; init; }

    public string? BaseUrl { get; init; }

    public string? PublicId { get; init; }

    public string? SystemId { get; init; }

    public string? InternalSubset { get; init; }

    public string? XmlVersion { get; init; }

    public string? Name { get; init; }

    public string? Value { get; init; }

    public int ChildNodeCount { get; init; }

    public IReadOnlyList<DOMAttribute> Attributes { get; init; }
        = Array.Empty<DOMAttribute>();

    public IReadOnlyList<Node> Children { get; init; }
        = Array.Empty<Node>();
}
