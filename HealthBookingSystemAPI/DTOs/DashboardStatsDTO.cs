namespace HealthBookingSystemAPI.DTOs
{
    public class DashboardStatsDTO
    {
        public int UpcomingAppointmentCount { get; set; }
        public int CompletedVisitCount { get; set; }
        public int UnreadMessageCount { get; set; }
        public int HealthScore { get; set; } = 85;
        public int ThisWeekAppointmentCount { get; set; }
        public int ThisWeekCheckupCount { get; set; }
        public int ThisWeekFollowupCount { get; set; }
    }
}
