namespace Monitor.Model;

public abstract record NotificationPayload(string Table, string Action, object Data);