namespace HealthBookingSystem.Models
{
    public class TimeOffRequest
    {
        public int TimeOffId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public bool IsAllDay { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}