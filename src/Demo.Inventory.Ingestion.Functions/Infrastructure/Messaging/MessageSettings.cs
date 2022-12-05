using System;

namespace Demo.Inventory.Ingestion.Functions.Infrastructure.Messaging;

public record MessageSettings
{
    private MessageSettings(TimeSpan visibility, TimeSpan timeToLive)
    {
        Visibility = visibility;
        TimeToLive = timeToLive;
    }

    public TimeSpan Visibility { get; }
    public TimeSpan TimeToLive { get; }

    public static MessageSettings DefaultSettings => new(TimeSpan.Zero, TimeSpan.Zero);

    public static MessageSettings New(int visibilityInSeconds, int timeToLiveInSeconds) =>
        new(TimeSpan.FromSeconds(visibilityInSeconds), TimeSpan.FromSeconds(timeToLiveInSeconds));

    public bool IsDefaultSettings() => Visibility == TimeSpan.Zero && TimeToLive == TimeSpan.Zero;
}