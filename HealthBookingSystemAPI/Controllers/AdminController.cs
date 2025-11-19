using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace HealthBookingSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAppointmentService _appointmentService;
        public AdminController(IUserService userService, IAppointmentService appointmentService)
        {
            _userService = userService;
            _appointmentService = appointmentService;
        }

        [HttpGet("GetDashboardStatistics")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            try
            {

                var allUsers = await _userService.GetAllUsers();

                // Patient statistics
                var patients = allUsers.Where(u => u.Role == "patient").ToList();
                var totalPatients = patients.Count;
                var activePatients = patients.Count(p => p.IsActive == true);
                var newPatientsThisMonth = patients.Count(p => p.CreatedAt >= DateTime.Now.AddDays(-30));
                var newPatientsThisWeek = patients.Count(p => p.CreatedAt >= DateTime.Now.AddDays(-7));

                // Real appointment data from database
                var allAppointments = await _appointmentService.GetAllAppointmentsAsync();
                var totalAppointments = allAppointments.Count;
                var confirmedAppointments = allAppointments.Count(a => a.Status == "Confirmed");
                var pendingAppointments = allAppointments.Count(a => a.Status == "Pending");
                var newAppointmentsThisMonth = allAppointments.Count(a => a.CreatedAt >= DateTime.Now.AddDays(-30));
                var newAppointmentsThisWeek = allAppointments.Count(a => a.CreatedAt >= DateTime.Now.AddDays(-7));

                var appointmentTrends = new List<object>();
                for (int i = 11; i >= 0; i--)
                {
                    var monthStart = DateTime.Now.AddMonths(-i).Date.AddDays(1 - DateTime.Now.AddMonths(-i).Day);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    var count = allAppointments.Count(a =>
                        a.CreatedAt >= monthStart &&
                        a.CreatedAt <= monthEnd);
                    appointmentTrends.Add(new
                    {
                        month = monthStart.ToString("MMM"),
                        count = count
                    });
                }

                // Daily appointment trends (last 7 days)
                var dailyAppointmentTrends = new List<object>();
                for (int i = 6; i >= 0; i--)
                {
                    var day = DateTime.Now.AddDays(-i).Date;
                    var count = allAppointments.Count(a => a.CreatedAt?.Date == day);
                    dailyAppointmentTrends.Add(new
                    {
                        day = day.ToString("ddd"),
                        date = day.ToString("MMM dd"),
                        count = count
                    });
                }

                // Weekly appointment trends (last 12 weeks)
                var weeklyAppointmentTrends = new List<object>();
                for (int i = 11; i >= 0; i--)
                {
                    var weekStart = DateTime.Now.AddDays(-(i * 7)).Date;
                    var weekEnd = weekStart.AddDays(6);
                    var count = allAppointments.Count(a => a.CreatedAt >= weekStart && a.CreatedAt <= weekEnd);
                    weeklyAppointmentTrends.Add(new
                    {
                        week = $"Week {12 - i}",
                        period = $"{weekStart:MMM dd} - {weekEnd:MMM dd}",
                        count = count
                    });
                }

                // Patient age distribution (mock data for now)
                var ageDistribution = new List<object>
                {
                    new { age = "18-25", count = 25 },
                    new { age = "26-35", count = 35 },
                    new { age = "36-45", count = 28 },
                    new { age = "46-55", count = 20 },
                    new { age = "56-65", count = 15 },
                    new { age = "65+", count = 12 }
                };

                // Patient status distribution
                var statusDistribution = new List<object>
                {
                    new { status = "Active", count = activePatients, color = "#10b981" },
                    new { status = "Inactive", count = totalPatients - activePatients, color = "#6b7280" }
                };

                // Recent patient registrations
                var recentPatients = patients
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(p => new
                    {
                        name = p.FullName,
                        email = p.Email,
                        registeredAt = p.CreatedAt?.ToString("MMM dd, yyyy"),
                        status = p.IsActive == true ? "Active" : "Inactive"
                    })
                    .ToList();

                // Appointment overview data (using real data from database)
                var appointmentOverviewData = new List<object>();
                for (int i = 11; i >= 0; i--)
                {
                    var monthStart = DateTime.Now.AddMonths(-i).Date.AddDays(1 - DateTime.Now.AddMonths(-i).Day);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    var confirmedCount = allAppointments.Count(a =>
                        a.Status == "Confirmed" &&
                        a.CreatedAt >= monthStart &&
                        a.CreatedAt <= monthEnd);
                    var pendingCount = allAppointments.Count(a =>
                        a.Status == "Pending" &&
                        a.CreatedAt >= monthStart &&
                        a.CreatedAt <= monthEnd);

                    appointmentOverviewData.Add(new
                    {
                        month = monthStart.ToString("MMM"),
                        confirmed = confirmedCount,
                        pending = pendingCount
                    });
                }

                var statistics = new
                {
                    totalPatients = totalPatients,
                    activePatients = activePatients,
                    newPatientsThisMonth = newPatientsThisMonth,
                    newPatientsThisWeek = newPatientsThisWeek,
                    totalAppointments = totalAppointments,
                    confirmedAppointments = confirmedAppointments,
                    pendingAppointments = pendingAppointments,
                    newAppointmentsThisMonth = newAppointmentsThisMonth,
                    newAppointmentsThisWeek = newAppointmentsThisWeek,
                    appointmentTrends = appointmentTrends,
                    dailyAppointmentTrends = dailyAppointmentTrends,
                    weeklyAppointmentTrends = weeklyAppointmentTrends,
                    appointmentOverviewData = appointmentOverviewData,
                    ageDistribution = ageDistribution,
                    statusDistribution = statusDistribution,
                    recentPatients = recentPatients
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving dashboard statistics.", error = ex.Message });
            }

        }

        [HttpGet("ManageDoctors")]
        public async Task<IActionResult> ManageDoctors()
        {
            var users = await _userService.GetAllUsers();
            var doctors = users.Where(u => u.Role != null && u.Role.ToLower() == "doctor").ToList();
            return Ok(doctors);
        }

        [HttpGet("ManagePatients")]
        public async Task<IActionResult> ManagePatients()
        {
            var users = await _userService.GetAllUsers();
            var patients = users.Where(u => u.Role != null && u.Role.ToLower() == "patient").ToList();
            return Ok(patients);
        }


    }
}
