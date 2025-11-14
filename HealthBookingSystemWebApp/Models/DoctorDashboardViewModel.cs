using BusinessObject.Models;
namespace HealthBookingSystem.Models
{
    public class DoctorDashboardViewModel
    {
        public Doctor CurrentDoctor { get; set; } = new Doctor();
        public DoctorDashboardStats Stats { get; set; } = new DoctorDashboardStats();
        public List<AppointmentViewModel> TodaySchedule { get; set; } = new List<AppointmentViewModel>();
        public List<PatientInfo> RecentPatients { get; set; } = new List<PatientInfo>();
        public List<NotificationItem> UrgentNotifications { get; set; } = new List<NotificationItem>();
        public WeeklyStats WeeklyStats { get; set; } = new WeeklyStats();
        public List<AppointmentViewModel> PendingAppointments { get; set; } = new List<AppointmentViewModel>();
    }

    public class DoctorDashboardStats
    {
        public int TodayAppointmentCount { get; set; }
        public int TotalPatientCount { get; set; }
        public int UnreadMessageCount { get; set; } = 12; // Placeholder
        public decimal AverageRating { get; set; } = 4.9m; // Placeholder
        public int NewPatientCount { get; set; }
        public int CompletedVisitCount { get; set; }
        public int PendingReviewCount { get; set; }
        public int FollowUpCount { get; set; }
    }

    public class WeeklyStats
    {
        public int WeeklyAppointmentCount { get; set; }
        public int WeeklyNewPatientCount { get; set; }
        public int WeeklyFollowUpCount { get; set; }
    }

    public class NotificationItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // "appointment", "message", "alert"
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string TimeDisplay => GetTimeDisplay();

        private string GetTimeDisplay()
        {
            var timeSpan = DateTime.Now - CreatedAt;
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            return CreatedAt.ToString("MMM dd");
        }
    }


}