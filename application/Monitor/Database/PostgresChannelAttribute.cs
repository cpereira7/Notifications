namespace Monitor.Database;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PostgresChannelAttribute : Attribute
{
    public string ChannelName { get; }

    public PostgresChannelAttribute(string channelName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(channelName);

        ChannelName = channelName;
    }
}