namespace Monitor.Model.Emergency;

public record EmergencyEvent(int Id, string Type, string Location, Regions Region, string Description, DateTime Timestamp);