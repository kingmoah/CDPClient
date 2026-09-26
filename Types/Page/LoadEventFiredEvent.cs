namespace CDPClient.Types.Page;

public sealed class LoadEventFiredEvent
{
    //special note: please use CDP MonotonicTime not unix time... 
    public double Timestamp { get; }

    /// <summary>
    /// special note: please use CDP MonotonicTime not unix time... 
    /// </summary>
    /// <param name="timestamp"></param>
    public LoadEventFiredEvent(double timestamp)
    {
        Timestamp = timestamp;
    }
}