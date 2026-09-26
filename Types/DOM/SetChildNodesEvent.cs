namespace CDPClient.Types.DOM;

public sealed record SetChildNodesEvent(
    NodeId ParentId,
    IReadOnlyList<Node> Nodes
) : DOMEvent;