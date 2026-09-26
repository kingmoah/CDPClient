namespace CDPClient.Types.Page;

public sealed class FrameNavigatedEvent
{
    public Frame Frame { get; }

    public FrameNavigatedEvent(Frame frame)
    {
        Frame = frame;
    }
}