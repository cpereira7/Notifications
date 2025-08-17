using Microsoft.Extensions.Logging;

namespace Monitor.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(LogLevel.Information, "New notification: {@notificationData}")]
    public static partial void LogNotification(this ILogger logger, object notificationData);

    [LoggerMessage(LogLevel.Warning, "Non-conforming notification received: {notification}")]
    public static partial void LogNonConformNotification(this ILogger logger, Exception ex, string notification);

    [LoggerMessage(LogLevel.Information, "Started the Background service for {@serviceName}")]
    public static partial void LogStartedListener(this ILogger logger, string serviceName);
}