using BusinessObject.Models;
namespace HealthBookingSystem.Models
{
    public class DashboardViewModel
    {
        public User CurrentUser { get; set; }
        public List<AppointmentViewModel> UpcomingAppointments { get; set; } = new List<AppointmentViewModel>();
        public List<AppointmentViewModel> RecentAppointments { get; set; } = new List<AppointmentViewModel>();
        public List<AppointmentViewModel> TodayAppointments { get; set; } = new List<AppointmentViewModel>();
        public DashboardStats Stats { get; set; } = new DashboardStats();
    }

    public class DashboardStats
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