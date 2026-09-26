using System.Runtime.InteropServices;
using CDPClient.Controllers;
using CDPClient.Types;
using CDPClient.Types.DOM;
using CDPClient.Types.Page;
using CDPClient.Types.Runtime;

// =========================================================
// Browser
// =========================================================

Browser edge = new(
Name: "Edge",
Path: RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
    ? @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
    : throw new PlatformNotSupportedException(
        "Edge path is only configured for Windows."
    )
);

BrowserLaunchOptions options = new(
    DebugPort: new DebugPort(9222),
    ProfileDirectory: new TempPath("edge-cdp")
);

using BrowserController browserController = new();

using BrowserSession session =
await browserController.GetOrCreateAsync(
    edge,
    options
);

Console.WriteLine("========================================");
Console.WriteLine(" BROWSER SESSION");
Console.WriteLine("========================================");

Console.WriteLine(
    $"Browser:     {session.Browser.Name}"
);

Console.WriteLine(
    $"CDP Port:    {session.DebugPort.Value}"
);

Console.WriteLine(
    $"Session:     {session.Ownership}"
);

if (session.Process is not null)
{
    Console.WriteLine(
    $"Process ID:  {session.Process.Id}"
    );
}

Console.WriteLine();

// =========================================================
// Target discovery
// =========================================================

using TargetController targetController = new();

IReadOnlyList<Target> targets =
    await targetController.GetTargetsAsync(
        session
    );

Console.WriteLine("========================================");
Console.WriteLine(" TARGETS");
Console.WriteLine("========================================");

Console.WriteLine(
    $"Found {targets.Count} target(s):"
);

foreach (Target target in targets)
{
    Console.WriteLine();

    Console.WriteLine(
        $"ID:        {target.Id}"
    );

    Console.WriteLine(
        $"Type:      {target.Type}"
    );

    Console.WriteLine(
        $"Title:     {target.Title}"
    );

    Console.WriteLine(
        $"URL:       {target.Url}"
    );

    Console.WriteLine(
        $"WebSocket: {target.WebSocketDebuggerUrl}"
    );

}

// =========================================================
// Select page target
// =========================================================

Target? page =
targets.FirstOrDefault(
    target =>
        target.Type.Equals(
            "page",
            StringComparison.OrdinalIgnoreCase
        )
);

if (page is null)
{
    throw new InvalidOperationException(
        "No page target was found."
    );
}

Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine(" SELECTED PAGE");
Console.WriteLine("========================================");

Console.WriteLine(
$"Title: {page.Title}"
);

Console.WriteLine(
$"URL:   {page.Url}"
);

// =========================================================
// CDP connection
// =========================================================

using CDPConnection connection = new();

connection.EventReceived +=
(_, @event) =>
{
    Console.WriteLine();
    Console.WriteLine("----------------------------------------");
    Console.WriteLine(
        $"EVENT: {@event.Method}"
    );
    Console.WriteLine("----------------------------------------");

    if (@event.Params.HasValue)
    {
        Console.WriteLine(
            @event.Params.Value
        );
    }
};

await connection.ConnectAsync(page);

Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine(" CDP CONNECTION");
Console.WriteLine("========================================");

Console.WriteLine(
    "Connected to CDP."
);

// =========================================================
// RAW CDP TEST
// =========================================================
//
// Keep this test because it verifies that the low-level
// CDP transport still works independently of the domain
// controllers.
//

// ---------------------------------------------------------
// Network.enable
// ---------------------------------------------------------

Console.WriteLine();

Console.WriteLine(
    "Sending raw Network.enable..."
);

CDPResponse networkResponse =
    await connection.SendAsync(
        "Network.enable"
    );

networkResponse.EnsureSuccess();

Console.WriteLine(
    "Network.enable succeeded."
);

// ---------------------------------------------------------
// Runtime.evaluate
// ---------------------------------------------------------

Console.WriteLine();

Console.WriteLine(
    "Sending raw Runtime.evaluate..."
);

CDPResponse rawRuntimeResponse =
    await connection.SendAsync(
        "Runtime.evaluate",
    new
    {
        expression = "document.title",
        returnByValue = true
    }
);

rawRuntimeResponse.EnsureSuccess();

Console.WriteLine();

Console.WriteLine(
    "Raw Runtime.evaluate response:"
);

Console.WriteLine(
    rawRuntimeResponse.Result
);

// =========================================================
// RUNTIME DOMAIN TEST
// =========================================================

using RuntimeController runtime =
    new(connection);

Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine(" RUNTIME DOMAIN");
Console.WriteLine("========================================");

// ---------------------------------------------------------
// document.title
// ---------------------------------------------------------

RuntimeEvaluateResult title =
    await runtime.EvaluateAsync(
        "document.title"
    );

Console.WriteLine(
    $"document.title = {title.Result.GetString()}"
);

// ---------------------------------------------------------
// location.href
// ---------------------------------------------------------

RuntimeEvaluateResult url =
    await runtime.EvaluateAsync(
        "location.href"
    );

Console.WriteLine(
    $"location.href = {url.Result.GetString()}"
);

// ---------------------------------------------------------
// navigator.userAgent
// ---------------------------------------------------------

RuntimeEvaluateResult userAgent =
    await runtime.EvaluateAsync(
        "navigator.userAgent"
    );

Console.WriteLine(
    $"navigator.userAgent = {userAgent.Result.GetString()}"
);

// ---------------------------------------------------------
// JavaScript number
// ---------------------------------------------------------

RuntimeEvaluateResult number =
    await runtime.EvaluateAsync(
        "6 * 7"
    );

Console.WriteLine(
    $"6 * 7 = {number.Result.GetNumber()}"
);

// ---------------------------------------------------------
// JavaScript boolean
// ---------------------------------------------------------

RuntimeEvaluateResult boolean =
    await runtime.EvaluateAsync(
        "document.readyState === 'complete'"
    );

Console.WriteLine(
    $"Page loaded = {boolean.Result.GetBoolean()}"
);

// =========================================================
// PAGE DOMAIN TEST
// =========================================================

using PageController pageController =
    new(connection);

Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine(" PAGE DOMAIN");
Console.WriteLine("========================================");

// ---------------------------------------------------------
// Page events
// ---------------------------------------------------------

pageController.FrameNavigated +=
    (_, @event) =>
    {
        Console.WriteLine();


        Console.WriteLine(
            "PAGE EVENT: frame navigated"
        );

        Console.WriteLine(
            $"Frame ID: {@event.Frame.Id}"
        );

        Console.WriteLine(
            $"URL:      {@event.Frame.Url}"
        );
    };

pageController.LoadEventFired +=
    (_, @event) =>
    {
        Console.WriteLine();


        Console.WriteLine(
            "PAGE EVENT: load event fired"
        );

        Console.WriteLine(
            $"Timestamp: {@event.Timestamp}"
        );
    };


// ---------------------------------------------------------
// Page.enable
// ---------------------------------------------------------

await pageController.EnableAsync();

Console.WriteLine(
    "Page domain enabled."
);

// ---------------------------------------------------------
// Navigate
// ---------------------------------------------------------

Console.WriteLine();

Console.WriteLine(
    "Navigating to https://example.com..."
);

NavigationResult navigation =
    await pageController.NavigateAsync(
        "https://example.com"
    );

if (navigation.IsSuccess)
{
    Console.WriteLine(
        $"Navigation started."
    );

    Console.WriteLine(
        $"Frame ID: {navigation.FrameId}"
    );

    Console.WriteLine(
        $"Loader ID: {navigation.LoaderId}"
    );


}
else
{
    Console.WriteLine(
        $"Navigation failed: {navigation.ErrorText}"
    );
}

// =========================================================
// Verify navigation through Runtime
// =========================================================
//
// Give the browser a moment to process navigation before
// evaluating the document.
// =========================================================

await Task.Delay(1000);

RuntimeEvaluateResult newTitle =
    await runtime.EvaluateAsync(
        "document.title"
    );

RuntimeEvaluateResult newUrl =
    await runtime.EvaluateAsync(
        "location.href"
    );

Console.WriteLine();

Console.WriteLine(
    $"Current title: {newTitle.Result.GetString()}"
);

Console.WriteLine(
    $"Current URL:   {newUrl.Result.GetString()}"
);

// =========================================================
// Interactive session
// =========================================================

Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine(" CDP SESSION ACTIVE");
Console.WriteLine("========================================");

Console.WriteLine(
    "The browser is now connected through CDP."
);

Console.WriteLine(
    "Type /exit to terminate the client."
);

Console.WriteLine(
    "Navigate or interact with the browser to generate events."
);

Console.WriteLine();

// =========================================================
// DOM DOMAIN TEST
// =========================================================

using DOMController dom =
    new(connection);

Console.WriteLine();

Console.WriteLine("========================================");
Console.WriteLine(" DOM DOMAIN");
Console.WriteLine("========================================");

dom.SetChildNodes +=
    (_, @event) =>
    {
        Console.WriteLine();

        Console.WriteLine(
            "DOM EVENT: child nodes set"
        );

        Console.WriteLine(
            $"Parent Node ID: {@event.ParentId.Value}"
        );

        Console.WriteLine(
            $"Node count: {@event.Nodes.Count}"
        );

        foreach (Node node in @event.Nodes)
        {
            Console.WriteLine(
                $"  {node.NodeName} " +
                $"NodeId={node.NodeId.Value} " +
                $"Children={node.ChildNodeCount}"
            );

            foreach (DOMAttribute attribute in node.Attributes)
            {
                Console.WriteLine(
                    $"    {attribute.Name} = {attribute.Value}"
                );
            }
        }
    };

await dom.EnableAsync();

Console.WriteLine(
    "DOM domain enabled."
);

Document document =
    await dom.GetDocumentAsync();

Console.WriteLine();

Console.WriteLine(
    $"Document Node ID: {document.Root.NodeId.Value}"
);

Console.WriteLine(
    $"Document Node Name: {document.Root.NodeName}"
);

Console.WriteLine(
    $"Child count: {document.Root.Children.Count}"
);

Node? body =
    await dom.QuerySelectorAsync(
        document.Root.NodeId,
        "body"
    );

if (body is null)
{
    Console.WriteLine(
        "Body element was not found."
    );
}
else
{
    Console.WriteLine();

    Console.WriteLine(
        $"Body Node ID: {body.NodeId.Value}"
    );

    string bodyHtml =
        await dom.GetOuterHTMLAsync(
            body.NodeId
        );

    Console.WriteLine();

    Console.WriteLine(
        "BODY HTML:"
    );

    Console.WriteLine(
        bodyHtml
    );
}

IReadOnlyList<Node> links =
    await dom.QuerySelectorAllAsync(
        document.Root.NodeId,
        "a"
    );

Console.WriteLine();

Console.WriteLine(
    $"Found {links.Count} link(s)."
);

foreach (Node link in links)
{
    Console.WriteLine(
        $"Link Node ID: {link.NodeId.Value}"
    );
}

while (true)
{
    Console.Write("> ");


    string? input =
        Console.ReadLine();

    if (input is null)
    {
        break;
    }

    if (input.Equals(
            "/exit",
            StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    Console.WriteLine(
        $"Unknown command: {input}"
    );
}