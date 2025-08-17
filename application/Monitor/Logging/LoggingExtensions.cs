using Microsoft.Extensions.Logging;

namespace Monitor.Logging;

public static partial class LoggingExtensions
{
    [LoggerMessage(LogLevel.Information, "New notification: {@notificationData}")]
    public static partial void LogNotification(this ILogger logger, object notificationData);

    [LoggerMessage(LogLevel.Warning, "Non-conforming notification received: {notification}")]
    public static partial void LogNonConformNotification(this ILogger logger, Exception ex, string notification);

    [LoggerMessage(LogLevel.Error, "Error occurred while listening for database notifications.")]
    public static partial void LogDatabaseConnectionFail(this ILogger logger, Exception ex);
    
    [LoggerMessage(LogLevel.Information, "Database connection opened successfully.")]
    public static partial void LogDatabaseConnectionOpen(this ILogger logger);
    
    [LoggerMessage(LogLevel.Error, "An error occurred while starting the database listener.")]
    public static partial void LogDatabaseListenerStartError(this ILogger logger, Exception ex);
}