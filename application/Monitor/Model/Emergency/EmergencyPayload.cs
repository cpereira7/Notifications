using Monitor.Database;
using Monitor.Model;
using Monitor.Model.Emergency;

namespace Monitor;

[PostgresChannel("datachange")]
public record EmergencyPayload : NotificationPayload
{
    public EmergencyPayload(string table, string action, EmergencyEvent data)
        : base(table, action, data)
    {
    }
}