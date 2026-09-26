namespace CDPClient.Types.DOM;

public sealed class Document
{
    public Node Root { get; }

    public Document(Node root)
    {
        Root = root;
    }
}