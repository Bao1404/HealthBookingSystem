using BusinessObject.Models;
namespace HealthBookingSystem.Models
{
    public class DashboardViewModel
    {
        public UserDTO CurrentUser { get; set; }
        public List<AppointmentDTO> UpcomingAppointments { get; set; } = new List<AppointmentDTO>();
        public List<AppointmentViewModel> RecentAppointments { get; set; } = new List<AppointmentViewModel>();
        public List<AppointmentDTO> TodayAppointments { get; set; } = new List<AppointmentDTO>();
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