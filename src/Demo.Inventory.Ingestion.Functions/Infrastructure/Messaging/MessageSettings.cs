using System;

namespace Demo.Inventory.Ingestion.Functions.Infrastructure.Messaging;

public record MessageSettings
{
    public TimeSpan Visibility { get; }
    public TimeSpan TimeToLive { get; }

    MessageSettings(TimeSpan visibility, TimeSpan timeToLive)
    {
        Visibility = visibility;
        TimeToLive = timeToLive;
    }

    public static MessageSettings New(int visibilityInSeconds, int timeToLiveInSeconds) =>
        new(TimeSpan.FromSeconds(visibilityInSeconds), TimeSpan.FromSeconds(timeToLiveInSeconds));

    public static MessageSettings DefaultSettings => new(TimeSpan.Zero, TimeSpan.Zero);

    public bool IsDefaultSettings() => Visibility == TimeSpan.Zero && TimeToLive == TimeSpan.Zero;
}