using CDPClient.Types.DOM;

namespace CDPClient.Boundary;

public interface IDOM
{
    Task EnableAsync(
        CancellationToken cancellationToken = default
    );

    Task<Document> GetDocumentAsync(
        CancellationToken cancellationToken = default
    );

    Task<Node?> QuerySelectorAsync(
        NodeId nodeId,
        string selector,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Node>> QuerySelectorAllAsync(
        NodeId nodeId,
        string selector,
        CancellationToken cancellationToken = default
    );

    Task<string> GetOuterHTMLAsync(
        NodeId nodeId,
        CancellationToken cancellationToken = default
    );

    event EventHandler<SetChildNodesEvent>? SetChildNodes;
}